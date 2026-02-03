using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly IServiceProvider _serviceProvider;

        public DynamicPricingService(
            IUnitOfWork unitOfWork,
            ILogger<DynamicPricingService> logger,
            IServiceProvider serviceProvider)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
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
            
            // Context data
            var occupancyRate = await GetOccupancyRateAsync(roomTypeId, date);
            var daysUntilArrival = (date.Date - DateTime.UtcNow.Date).Days;

            foreach (var rule in rules)
            {
                bool conditionMet = false;

                switch (rule.RuleType)
                {
                    case PricingRuleType.Occupancy:
                        // ConditionValue = 0.80 (80%)
                        // Assuming rule means "If Occupancy >= ConditionValue"
                        if (occupancyRate >= rule.ConditionValue)
                        {
                            conditionMet = true;
                        }
                        break;
                    case PricingRuleType.LeadTime:
                        // ConditionValue = 60 (days)
                        // Assuming rule means "If LeadTime >= ConditionValue" (Early Bird)
                        // OR we might need separate types for EarlyBird vs LastMinute.
                        // Let's assume ConditionValue > 10 means "Advance booking" and < 10 means "Last minute" logic?
                        // Better: Rule specific logic.
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
                        // Simple check: Is month equal to condition value?
                        // Or complex ranges. Let's assume ConditionValue 1-12 = Month.
                        if (date.Month == (int)rule.ConditionValue)
                        {
                            conditionMet = true;
                        }
                        break;
                    case PricingRuleType.DayOfWeek:
                         // 0=Sunday, 1=Monday... 6=Saturday usually.
                         // Or 1=Monday... 7=Sunday. C# DayOfWeek is Sunday=0.
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
                        // +10 means +10%. -5 means -5%.
                        // Formula: Rate + (Rate * (Value / 100))
                        // Or applied to BaseRate? usually compounded or applied to base.
                        // Let's apply to *current* rate (compounded) as rules are prioritized.
                        currentRate += currentRate * (rule.AdjustmentValue / 100m);
                        appliedRules.Add($"{rule.RuleName} ({rule.AdjustmentValue}%)");
                    }
                    else if (rule.AdjustmentType == PriceAdjustmentType.FixedAmount)
                    {
                        currentRate += rule.AdjustmentValue;
                        appliedRules.Add($"{rule.RuleName} ({rule.AdjustmentValue} {rule.AdjustmentValue:C})");
                    }
                    
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
                AppliedRules = appliedRules
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

        private async Task<decimal> GetOccupancyRateAsync(int roomTypeId, DateTime date)
        {
            // Placeholder: Retrieve real occupancy from ReservationRepository/OccupancyService
            // For Sprint 8 MVP, we might mock this or implement a basic query.
            // Let's assume 50% for now to unblock testing.
            return 0.50m;
        }
    }
}
