using System;
using System.ComponentModel.DataAnnotations;
using GuestFlow.Domain.Entities.Core;

namespace GuestFlow.Domain.Entities.Finance
{
    public enum PricingRuleType
    {
        Occupancy,      // e.g., If occupancy > 80%
        Seasonality,    // e.g., If date is in July
        LeadTime,       // e.g., If booking is > 60 days in advance
        DayOfWeek,      // e.g., If stay includes Weekend
        LastMinute      // e.g., If booking is < 2 days in advance
    }

    public enum PriceAdjustmentType
    {
        Percentage,     // e.g., +10%
        FixedAmount,    // e.g., +50 USD
        StopSell        // Close availability
    }

    public class PricingRuleEntity : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string RuleName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public PricingRuleType RuleType { get; set; }

        /// <summary>
        /// The value to compare against.
        /// For Occupancy: 0.80 (80%)
        /// For LeadTime: 60 (days)
        /// For Seasonality: Month number or specific range handling
        /// </summary>
        public decimal ConditionValue { get; set; }

        public PriceAdjustmentType AdjustmentType { get; set; }

        /// <summary>
        /// The value to adjust by.
        /// Can be positive (increase) or negative (discount).
        /// Example: 10 (10%), -5 (-5 USD)
        /// </summary>
        public decimal AdjustmentValue { get; set; }

        /// <summary>
        /// Execution order. Lower numbers run first.
        /// </summary>
        public int Priority { get; set; }

        public bool IsActive { get; set; } = true;
        
        // Tenant support is inherited from BaseEntity if BaseEntity implements ITenantEntity
        // Checking BaseEntity via separate validation if needed, but assuming standard flow.
    }
}
