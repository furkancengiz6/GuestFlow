namespace GuestFlow.Application.Models.Responses.Profitability
{
    public class ProfitabilityReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime GeneratedAt { get; set; }

        // Overall totals
        public decimal TotalRevenue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal ProfitMargin { get; set; } // Percentage

        // Breakdowns
        public List<SupplierBreakdown> SupplierBreakdown { get; set; } = new();
        public List<ServiceTypeBreakdown> ServiceTypeBreakdown { get; set; } = new();
    }

    public class SupplierBreakdown
    {
        public string SupplierName { get; set; }
        public int ServiceCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public decimal Profit { get; set; }
        public decimal ProfitMargin { get; set; } // Percentage
    }

    public class ServiceTypeBreakdown
    {
        public string ServiceType { get; set; } // Transfer, Tour, Restaurant
        public int ServiceCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public decimal Profit => Revenue - Cost;
        public decimal ProfitMargin => Revenue > 0 ? (Profit / Revenue) * 100 : 0;
    }

    public class SupplierProfitability
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public decimal Profit { get; set; }
        public decimal ProfitMargin { get; set; } // Percentage
    }

    // Individual service profitability classes
    public class TransferProfitability
    {
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public string Currency { get; set; }
        public decimal Profit => Revenue - Cost;
        public decimal ProfitMargin => Revenue > 0 ? (Profit / Revenue) * 100 : 0;
    }

    public class TourProfitability
    {
        public int Id { get; set; }
        public string Type { get; set; } // CityTour, YachtTour
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public string Currency { get; set; }
        public decimal Profit => Revenue - Cost;
        public decimal ProfitMargin => Revenue > 0 ? (Profit / Revenue) * 100 : 0;
    }

    public class ReservationProfitability
    {
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public string Currency { get; set; }
        public decimal Profit => Revenue - Cost;
        public decimal ProfitMargin => Revenue > 0 ? (Profit / Revenue) * 100 : 0;
    }
}