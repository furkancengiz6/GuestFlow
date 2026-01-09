using GuestFlow.Application.Models.Requests.Supplier;
using GuestFlow.Application.Models.Responses;
using GuestFlow.Domain.Entities.Core;
using SupplierEntity = GuestFlow.Domain.Entities.Core.Supplier;

namespace GuestFlow.Application.Operations.Supplier
{
    public interface ISupplierService
    {
        Task<ApiResponse<SupplierEntity>> CreateSupplierAsync(CreateSupplierRequest request);
        Task<ApiResponse<SupplierEntity>> UpdateSupplierAsync(int id, UpdateSupplierRequest request);
        Task<ApiResponse<bool>> DeleteSupplierAsync(int id);
        Task<ApiResponse<SupplierEntity>> GetSupplierByIdAsync(int id);
        Task<ApiResponse<List<SupplierEntity>>> GetAllSuppliersAsync(string? type = null, bool? isActive = null);
        Task<ApiResponse<List<SupplierEntity>>> GetSuppliersByTypeAsync(string type);
    }
}