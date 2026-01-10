using GuestFlow.Application.Models.Requests.Supplier;
using GuestFlow.Application.Models.Responses;
using GuestFlow.Domain.Entities.Core;
using SupplierEntity = GuestFlow.Domain.Entities.Core.Supplier;
using GuestFlow.Domain.UnitOfWork;

namespace GuestFlow.Application.Operations.Supplier
{
    public class SupplierManager : ISupplierService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly GuestFlow.Domain.Entities.Repositories.IRepository<SupplierEntity> _supplierRepository;

        public SupplierManager(IUnitOfWork unitOfWork, GuestFlow.Domain.Entities.Repositories.IRepository<SupplierEntity> supplierRepository)
        {
            _unitOfWork = unitOfWork;
            _supplierRepository = supplierRepository;
        }

        public async Task<ApiResponse<SupplierEntity>> CreateSupplierAsync(CreateSupplierRequest request)
        {
            try
            {
                var supplier = new SupplierEntity
                {
                    Name = request.Name,
                    Type = request.Type,
                    ContactName = request.ContactName,
                    PhoneNumber = request.PhoneNumber,
                    Email = request.Email,
                    Address = request.Address,
                    Website = request.Website,
                    Notes = request.Notes,
                    IsActive = request.IsActive,
                    DefaultCurrency = request.DefaultCurrency,
                    DefaultCost = request.DefaultCost
                };

                await _supplierRepository.AddAsync(supplier);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<SupplierEntity>.SuccessResponse(supplier, "Supplier created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<SupplierEntity>.Fail($"Failed to create supplier: {ex.Message}");
            }
        }

        public async Task<ApiResponse<SupplierEntity>> UpdateSupplierAsync(int id, UpdateSupplierRequest request)
        {
            try
            {
                var supplier = await _supplierRepository.GetByIdAsync(id);
                if (supplier == null)
                return ApiResponse<SupplierEntity>.Fail("Supplier not found");

                supplier.Name = request.Name ?? supplier.Name;
                supplier.Type = request.Type ?? supplier.Type;
                supplier.ContactName = request.ContactName ?? supplier.ContactName;
                supplier.PhoneNumber = request.PhoneNumber ?? supplier.PhoneNumber;
                supplier.Email = request.Email ?? supplier.Email;
                supplier.Address = request.Address ?? supplier.Address;
                supplier.Website = request.Website ?? supplier.Website;
                supplier.Notes = request.Notes ?? supplier.Notes;
                supplier.IsActive = request.IsActive ?? supplier.IsActive;
                supplier.DefaultCurrency = request.DefaultCurrency ?? supplier.DefaultCurrency;
                supplier.DefaultCost = request.DefaultCost ?? supplier.DefaultCost;

                _supplierRepository.Update(supplier);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<SupplierEntity>.SuccessResponse(supplier, "Supplier updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<SupplierEntity>.Fail($"Failed to update supplier: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteSupplierAsync(int id)
        {
            try
            {
                var supplier = await _supplierRepository.GetByIdAsync(id);
                if (supplier == null)
                    return ApiResponse<bool>.Fail("Supplier not found");

                supplier.IsDeleted = true;
                _supplierRepository.Update(supplier);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Supplier deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail($"Failed to delete supplier: {ex.Message}");
            }
        }

        public async Task<ApiResponse<SupplierEntity>> GetSupplierByIdAsync(int id)
        {
            try
            {
                var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
                if (supplier == null)
                    return ApiResponse<SupplierEntity>.Fail("Supplier not found");

                return ApiResponse<SupplierEntity>.SuccessResponse(supplier);
            }
            catch (Exception ex)
            {
                return ApiResponse<SupplierEntity>.Fail($"Failed to get supplier: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<SupplierEntity>>> GetAllSuppliersAsync(string? type = null, bool? isActive = null)
        {
            try
            {
                var query = _supplierRepository.GetAll();

                if (!string.IsNullOrEmpty(type))
                    query = query.Where(s => s.Type == type);

                if (isActive.HasValue)
                    query = query.Where(s => s.IsActive == isActive.Value);

                var suppliers = await query.ToListAsync();
                return ApiResponse<List<SupplierEntity>>.SuccessResponse(suppliers);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<SupplierEntity>>.Fail($"Failed to get suppliers: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<SupplierEntity>>> GetSuppliersByTypeAsync(string type)
        {
            try
            {
                var suppliers = await _supplierRepository
                    .GetAll(s => s.Type == type && s.IsActive)
                    .ToListAsync();

                return ApiResponse<List<SupplierEntity>>.SuccessResponse(suppliers);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<SupplierEntity>>.Fail($"Failed to get suppliers by type: {ex.Message}");
            }
        }
    }
}