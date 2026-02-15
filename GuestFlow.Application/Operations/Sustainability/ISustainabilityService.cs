using System.Threading.Tasks;
using GuestFlow.Domain.Entities.Enum;

namespace GuestFlow.Application.Operations.Sustainability
{
    public interface ISustainabilityService
    {
        Task<int> RecordActionAsync(int guestId, SustainabilityActionType actionType, string? notes = null);
        Task<string> GetAIRewardRecommendationAsync(int guestId);
        Task<int> GetGuestScoreAsync(int guestId);
    }
}
