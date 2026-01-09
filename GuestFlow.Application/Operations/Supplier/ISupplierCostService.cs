using GuestFlow.Application.Models;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Supplier
{
    public interface ISupplierCostService
    {
        /// <summary>
        /// Sync supplier costs from external source or recalculate internal supplier cost records.
        /// </summary>
        Task<ApiResponse<bool>> SyncSupplierCostsAsync();
        Task<ApiResponse<List<GuestFlow.Domain.Entities.Operations.SupplierCost>>> GetAllAsync();
        Task<ApiResponse<GuestFlow.Domain.Entities.Operations.SupplierCost>> GetByIdAsync(int id);
        Task<ApiResponse<GuestFlow.Domain.Entities.Operations.SupplierCost>> CreateAsync(GuestFlow.Application.Models.Requests.Supplier.CreateSupplierCostRequest request);
        Task<ApiResponse<GuestFlow.Domain.Entities.Operations.SupplierCost>> UpdateAsync(int id, GuestFlow.Application.Models.Requests.Supplier.UpdateSupplierCostRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}

