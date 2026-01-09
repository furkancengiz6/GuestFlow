using GuestFlow.Application.Models.Responses.Profitability;

namespace GuestFlow.Application.Operations.Profitability
{
    public interface IProfitabilityService
    {
        Task<ApiResponse<ProfitabilityReport>> GetProfitabilityReportAsync(
            DateTime startDate, DateTime endDate, string? supplierId = null);

        Task<ApiResponse<List<SupplierProfitability>>> GetTopSuppliersByProfitAsync(
            DateTime startDate, DateTime endDate, int topCount = 10);
    }
}