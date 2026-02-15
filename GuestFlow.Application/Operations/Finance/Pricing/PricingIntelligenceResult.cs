using System;
using System.Collections.Generic;

namespace GuestFlow.Application.Operations.Finance.Pricing
{
    public class PricingIntelligenceResult
    {
        public DateTime Date { get; set; }
        public double ForecastedOccupancy { get; set; }
        public decimal BaseRate { get; set; }
        public decimal DynamicRate { get; set; }
        public bool IsStopSell { get; set; }
        public List<string> AppliedRules { get; set; } = new List<string>();
        public List<AppliedRuleDetail> RuleDetails { get; set; } = new List<AppliedRuleDetail>();
    }
}
