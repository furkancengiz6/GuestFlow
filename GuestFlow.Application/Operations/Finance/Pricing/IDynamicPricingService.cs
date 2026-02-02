using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Finance.Pricing
{
    public interface IDynamicPricingService
    {
        /// <summary>
        /// Calculates the dynamic rate and status for a specific room type and date based on active rules.
        /// </summary>
        /// <param name="roomTypeId">The room type identifier.</param>
        /// <param name="date">The date for which to calculate the price.</param>
        /// <param name="baseRate">The base rate (usually from Rate Plan).</param>
        /// <returns>The calculated dynamic rate result.</returns>
        Task<DynamicPricingResult> CalculateRateAsync(int roomTypeId, DateTime date, decimal baseRate);

        /// <summary>
        /// Triggers the dynamic pricing engine to recalculate rates for upcoming dates and push them to OTAs.
        /// </summary>
        /// <param name="daysAhead">Number of days ahead to process (default 30).</param>
        Task PushDynamicRatesToOTAsAsync(int daysAhead = 30);
    }
}
