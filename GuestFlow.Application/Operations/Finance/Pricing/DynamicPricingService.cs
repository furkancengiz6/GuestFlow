using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GuestFlow.Application.Operations.Intelligence.Predictive;
using GuestFlow.Domain.Entities.Finance;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.Finance.Pricing
{
    public class DynamicPricingService : IDynamicPricingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DynamicPricingService> _logger;
        private readonly IPredictiveAnalyticsService _predictiveAnalyticsService;
        private readonly IServiceProvider _serviceProvider;

        public DynamicPricingService(
            IUnitOfWork unitOfWork,
            ILogger<DynamicPricingService> logger,
            IPredictiveAnalyticsService predictiveAnalyticsService,
            IServiceProvider serviceProvider)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _predictiveAnalyticsService = predictiveAnalyticsService;
            _serviceProvider = serviceProvider;
        }

        public async Task<DynamicPricingResult> CalculateRateAsync(int roomTypeId, DateTime date, decimal baseRate)
        {
            // Fetch active rules from DB, ordered by priority
            var rules = await _unitOfWork.PricingRules
                .GetAll()
                .Where(r => r.IsActive)
                .OrderBy(r => r.Priority)
                .ToListAsync();

            decimal currentRate = baseRate;
            bool isStopSell = false;
            var appliedRules = new List<string>();
            var ruleDetails = new List<AppliedRuleDetail>();
            
            // Context data
            var occupancyRate = await GetOccupancyRateAsync(roomTypeId, date);
            var daysUntilArrival = (date.Date - DateTime.UtcNow.Date).Days;

            foreach (var rule in rules)
            {
                bool conditionMet = false;

                switch (rule.RuleType)
                {
                    case PricingRuleType.Occupancy:
                        if (occupancyRate >= rule.ConditionValue)
                        {
                            conditionMet = true;
                        }
                        break;
                    case PricingRuleType.LeadTime:
                        if (daysUntilArrival >= rule.ConditionValue)
                        {
                            conditionMet = true;
                        }
                        break;
                    case PricingRuleType.LastMinute:
                        if (daysUntilArrival <= rule.ConditionValue)
                        {
                            conditionMet = true;
                        }
                        break;
                    case PricingRuleType.Seasonality:
                        if (date.Month == (int)rule.ConditionValue)
                        {
                            conditionMet = true;
                        }
                        break;
                    case PricingRuleType.DayOfWeek:
                         if ((int)date.DayOfWeek == (int)rule.ConditionValue)
                         {
                             conditionMet = true;
                         }
                        break;
                }

                if (conditionMet)
                {
                    if (rule.AdjustmentType == PriceAdjustmentType.StopSell)
                    {
                        isStopSell = true;
                        appliedRules.Add($"{rule.RuleName} (STOP SELL)");
                    }
                    else if (rule.AdjustmentType == PriceAdjustmentType.Percentage)
                    {
                        currentRate += currentRate * (rule.AdjustmentValue / 100m);
                        appliedRules.Add($"{rule.RuleName} ({rule.AdjustmentValue}%)");
                    }
                    else if (rule.AdjustmentType == PriceAdjustmentType.FixedAmount)
                    {
                        currentRate += rule.AdjustmentValue;
                        appliedRules.Add($"{rule.RuleName} ({rule.AdjustmentValue} {rule.AdjustmentValue:C})");
                    }

                    ruleDetails.Add(new AppliedRuleDetail
                    {
                        RuleName = rule.RuleName,
                        RuleType = rule.RuleType.ToString(),
                        AdjustmentType = rule.AdjustmentType.ToString(),
                        AdjustmentValue = rule.AdjustmentValue,
                        ResultingRate = currentRate
                    });
                    
                    if (rule.AdjustmentType != PriceAdjustmentType.StopSell)
                    {
                        _logger.LogInformation("Applied Rule {RuleName} ({RuleType}): Adjusted rate from {OldRate} to {NewRate}", 
                            rule.RuleName, rule.RuleType, baseRate, currentRate);
                    }
                }
            }

            return new DynamicPricingResult
            {
                BaseRate = baseRate,
                FinalRate = Math.Round(currentRate, 2),
                IsStopSell = isStopSell,
                AppliedRules = appliedRules,
                RuleDetails = ruleDetails
            };
        }

        public async Task PushDynamicRatesToOTAsAsync(int daysAhead = 30)
        {
            _logger.LogInformation("Pushing dynamic rates to OTAs for the next {Days} days...", daysAhead);

            // 1. Get Active OTA Integrations
            var activeIntegrations = await _unitOfWork.OTAIntegrations
                .GetAll(i => i.IsActive && !i.IsDeleted)
                .ToListAsync();

            if (!activeIntegrations.Any())
            {
                _logger.LogWarning("No active OTA integrations found.");
                return;
            }

            // 2. Define Room Types to Sync (MVP: Hardcoded or fetch from mappings)
            // Ideally, iterate over mapped rooms in OTAHotelMapping
            var roomTypeIds = new List<int> { 1, 2, 3 }; // Example Room Type IDs

            var startDate = DateTime.UtcNow.Date;
            var endDate = startDate.AddDays(daysAhead);

            int totalUpdates = 0;

            foreach (var integration in activeIntegrations)
            {
                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    foreach (var roomTypeId in roomTypeIds)
                    {
                        try
                        {
                            // 3. Calculate Rate
                            // Base Rate assumption: 100 or fetch from RatePlanService
                            decimal baseRate = 100m; 

                            var calculationResult = await CalculateRateAsync(roomTypeId, date, baseRate);

                            // 4. Push to OTA
                            // Mapping internal RoomTypeId to OTA Room Type ID string
                            var otaRoomTypeId = $"Room_{roomTypeId}";

                            // Resolve channel manager lazily to break circular dependency
                            var channelManager = _serviceProvider.GetService(typeof(GuestFlow.Application.Operations.OTA.IOTAChannelManagerService)) 
                                as GuestFlow.Application.Operations.OTA.IOTAChannelManagerService;

                            if (channelManager == null)
                            {
                                _logger.LogError("Could not resolve IOTAChannelManagerService from service provider.");
                                continue;
                            }

                            if (calculationResult.IsStopSell)
                            {
                                // Send Close Availability
                                var stopSellResult = await channelManager.SyncAvailabilityToOTAAsync(integration.Id, 1, date); 
                                // ... rest of existing logic ...
                                _logger.LogInformation("STOP SELL triggered for Room {Room}, Date {Date}. Closing availability.", roomTypeId, date);
                            }
                            else
                            {
                                var result = await channelManager.PushRateUpdateAsync(integration.Id, otaRoomTypeId, date, calculationResult.FinalRate);
                                if (result.Success)
                                {
                                    totalUpdates++;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to push dynamic rate for Integration {Integration}, Room {Room}, Date {Date}", 
                                integration.ProviderName, roomTypeId, date);
                        }
                    }
                }
            }

            _logger.LogInformation("Completed dynamic rate push. Total updates: {TotalUpdates}", totalUpdates);
        }

        public async Task<List<PricingIntelligenceResult>> GetPricingIntelligenceAsync(int roomTypeId, DateTime startDate, DateTime endDate)
        {
            _logger.LogInformation("Generating pricing intelligence for RoomType {RoomType} from {Start} to {End}", roomTypeId, startDate, endDate);
            
            var results = new List<PricingIntelligenceResult>();
            var occupancyForecasts = await _predictiveAnalyticsService.PredictOccupancyAsync(startDate, endDate);
            
            // MVP: Using a standard base rate to demonstrate delta. 
            // Better: Could fetch from current RatePlans if available.
            const decimal MockBaseRate = 100m; 

            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                var forecast = occupancyForecasts.FirstOrDefault(f => f.Date.Date == date);
                var calculation = await CalculateRateAsync(roomTypeId, date, MockBaseRate);

                results.Add(new PricingIntelligenceResult
                {
                    Date = date,
                    ForecastedOccupancy = forecast?.ForecastedOccupancyRate ?? 0.5,
                    BaseRate = MockBaseRate,
                    DynamicRate = calculation.FinalRate,
                    IsStopSell = calculation.IsStopSell,
                    AppliedRules = calculation.AppliedRules,
                    RuleDetails = calculation.RuleDetails
                });
            }

            return results;
        }

        private async Task<decimal> GetOccupancyRateAsync(int roomTypeId, DateTime date)
        {
            try
            {
                // Fetch AI-powered occupancy forecast for the specific date
                var forecasts = await _predictiveAnalyticsService.PredictOccupancyAsync(date.Date, date.Date);
                var forecast = forecasts.FirstOrDefault(f => f.Date.Date == date.Date);
                
                if (forecast != null)
                {
                    _logger.LogInformation("AI Occupancy Forecast retrieved for Date={Date}, RoomType={RoomType}, Rate={Rate}", 
                        date.ToShortDateString(), roomTypeId, forecast.ForecastedOccupancyRate);
                    return (decimal)forecast.ForecastedOccupancyRate;
                }

                _logger.LogWarning("No AI occupancy forecast found for {Date}. Falling back to 50%.", date.ToShortDateString());
                return 0.50m;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching AI occupancy forecast for {Date}. Falling back to 50%.", date.ToShortDateString());
                return 0.50m;
            }
        }
    }
}
