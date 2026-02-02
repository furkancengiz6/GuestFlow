using System.Collections.Generic;

namespace GuestFlow.Application.Operations.Finance.Pricing
{
    public class DynamicPricingResult
    {
        public decimal FinalRate { get; set; }
        public decimal BaseRate { get; set; }
        public bool IsStopSell { get; set; }
        public List<string> AppliedRules { get; set; } = new List<string>();
    }
}
