using System;
using System.Linq;
using System.Threading.Tasks;
using GuestFlow.Application.Operations.AI;
using GuestFlow.Application.Models.AI;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using GuestFlow.Domain.Entities.Sustainability;
using GuestFlow.Domain.Entities.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.Sustainability
{
    public class SustainabilityService : ISustainabilityService
    {
        private readonly IRepository<SustainabilityAction> _actionRepository;
        private readonly IRepository<GuestEntity> _guestRepository;
        private readonly IAIAssistantService _aiAssistantService;
        private readonly ILogger<SustainabilityService> _logger;

        public SustainabilityService(
            IRepository<SustainabilityAction> actionRepository,
            IRepository<GuestEntity> guestRepository,
            IAIAssistantService aiAssistantService,
            ILogger<SustainabilityService> logger)
        {
            _actionRepository = actionRepository;
            _guestRepository = guestRepository;
            _aiAssistantService = aiAssistantService;
            _logger = logger;
        }

        public async Task<int> RecordActionAsync(int guestId, SustainabilityActionType actionType, string? notes = null)
        {
            try
            {
                var guest = await _guestRepository.GetByIdAsync(guestId);
                if (guest == null) throw new Exception("Guest not found");

                int points = actionType switch
                {
                    SustainabilityActionType.ReuseTowels => 10,
                    SustainabilityActionType.DeclineHousekeeping => 20,
                    SustainabilityActionType.EnergySavingAction => 15,
                    SustainabilityActionType.WaterConservation => 15,
                    SustainabilityActionType.LocalProductConsumption => 20,
                    SustainabilityActionType.WasteReduction => 25,
                    SustainabilityActionType.UseBikeRental => 30,
                    SustainabilityActionType.PublicTransportUse => 20,
                    _ => 5
                };

                var action = new SustainabilityAction
                {
                    GuestId = guestId,
                    ActionType = actionType,
                    ImpactScore = points,
                    ActionDate = DateTime.UtcNow,
                    Description = notes
                };

                await _actionRepository.AddAsync(action);
                
                // Update guest total score
                guest.SustainabilityScore += points;
                await _guestRepository.UpdateAsync(guest);

                return guest.SustainabilityScore;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error recording sustainability action for guest {guestId}");
                throw;
            }
        }

        public async Task<string> GetAIRewardRecommendationAsync(int guestId)
        {
            try
            {
                var guest = await _guestRepository.GetByIdAsync(guestId);
                if (guest == null) return "Misafir bulunamadı.";

                var actions = await _actionRepository.GetAll()
                    .Where(a => a.GuestId == guestId)
                    .OrderByDescending(a => a.ActionDate)
                    .Take(10)
                    .ToListAsync();

                var actionsSummary = string.Join(", ", actions.Select(a => $"{a.ActionType} ({a.ImpactScore} pts)"));

                var request = new AIChatRequest
                {
                    Message = $"Suggest a sustainability reward for this guest. Guest Name: {guest.FullName}, Total Sustainability Score: {guest.SustainabilityScore}, Recent Actions: {actionsSummary}. The reward should encourage more green actions. Use a friendly tone in Turkish.",
                    Context = "SustainabilityRewards"
                };

                var response = await _aiAssistantService.ProcessMessageAsync(request);
                return response.Response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting AI reward recommendation for guest {guestId}");
                return "Şu an için bir ödül önerilemiyor, ancak yeşil adımlarınız için teşekkürler!";
            }
        }

        public async Task<int> GetGuestScoreAsync(int guestId)
        {
            var guest = await _guestRepository.GetByIdAsync(guestId);
            return guest?.SustainabilityScore ?? 0;
        }
    }
}
