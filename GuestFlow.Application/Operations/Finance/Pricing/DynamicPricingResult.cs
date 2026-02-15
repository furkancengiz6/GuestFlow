using System.Collections.Generic;

namespace GuestFlow.Application.Operations.Finance.Pricing
{
    public class DynamicPricingResult
    {
        public decimal FinalRate { get; set; }
        public decimal BaseRate { get; set; }
        public bool IsStopSell { get; set; }
        public List<string> AppliedRules { get; set; } = new List<string>();
        public List<AppliedRuleDetail> RuleDetails { get; set; } = new List<AppliedRuleDetail>();
    }

    public class AppliedRuleDetail
    {
        public string RuleName { get; set; } = string.Empty;
        public string RuleType { get; set; } = string.Empty;
        public string AdjustmentType { get; set; } = string.Empty;
        public decimal AdjustmentValue { get; set; }
        public decimal ResultingRate { get; set; }
    }
}
