using GuestFlow.Application.Models;
using GuestFlow.Domain.Entities.Operations;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace GuestFlow.Application.Operations.Supplier
{
    public class SupplierCostService : ISupplierCostService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SupplierCostService> _logger;

        public SupplierCostService(IUnitOfWork unitOfWork, ILogger<SupplierCostService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> SyncSupplierCostsAsync()
        {
            try
            {
                // 1) Get current suppliers to map by name
                var suppliers = await _unitOfWork.Suppliers.GetAll().ToListAsync();

                // 2) Collect service records that include supplier cost info
                var transfers = await _unitOfWork.Transfers.GetAll(t => t.SupplierCost.HasValue).ToListAsync();
                var cityTours = await _unitOfWork.CityTours.GetAll(ct => ct.SupplierCost.HasValue).ToListAsync();
                var yachtTours = await _unitOfWork.YachtTours.GetAll(yt => yt.SupplierCost.HasValue).ToListAsync();

                // 3) Insert or update SupplierCost records for each service
                foreach (var t in transfers)
                {
                    var supplier = suppliers.FirstOrDefault(s => s.Name == t.SupplierName);
                    if (supplier == null)
                    {
                        _logger.LogWarning("Supplier not found for transfer {TransferId} supplierName={SupplierName}", t.Id, t.SupplierName);
                        continue;
                    }

                    var existing = await _unitOfWork.SupplierCosts.GetAll(sc => sc.TransferId == t.Id).FirstOrDefaultAsync();
                    if (existing == null)
                    {
                        var sc = new SupplierCost
                        {
                            SupplierId = supplier.Id,
                            TransferId = t.Id,
                            CostAmount = t.SupplierCost!.Value,
                            Currency = t.SupplierCurrency ?? t.Currency ?? "USD",
                            CostType = "BaseCost",
                            Description = $"Imported from transfer {t.Id}",
                            ValidFrom = t.TransferDate,
                            IsActive = true
                        };
                        await _unitOfWork.SupplierCosts.AddAsync(sc);
                    }
                    else
                    {
                        existing.CostAmount = t.SupplierCost!.Value;
                        existing.Currency = t.SupplierCurrency ?? t.Currency ?? existing.Currency;
                        existing.Description = $"Updated from transfer {t.Id}";
                        existing.ValidFrom = t.TransferDate;
                        existing.IsActive = true;
                        _unitOfWork.SupplierCosts.Update(existing);
                    }
                }

                // Similar logic for cityTours, yachtTours, reservations
                foreach (var ct in cityTours)
                {
                    var supplier = suppliers.FirstOrDefault(s => s.Name == ct.SupplierName);
                    if (supplier == null) { _logger.LogWarning("Supplier not found for city tour {Id}", ct.Id); continue; }
                    var existing = await _unitOfWork.SupplierCosts.GetAll(sc => sc.CityTourId == ct.Id).FirstOrDefaultAsync();
                    if (existing == null)
                    {
                        var sc = new SupplierCost
                        {
                            SupplierId = supplier.Id,
                            CityTourId = ct.Id,
                            CostAmount = ct.SupplierCost!.Value,
                            Currency = ct.SupplierCurrency ?? ct.Currency ?? "USD",
                            CostType = "BaseCost",
                            Description = $"Imported from city tour {ct.Id}",
                            ValidFrom = ct.TourDate,
                            IsActive = true
                        };
                        await _unitOfWork.SupplierCosts.AddAsync(sc);
                    }
                    else
                    {
                        existing.CostAmount = ct.SupplierCost!.Value;
                        existing.Currency = ct.SupplierCurrency ?? ct.Currency ?? existing.Currency;
                        existing.Description = $"Updated from city tour {ct.Id}";
                        existing.ValidFrom = ct.TourDate;
                        existing.IsActive = true;
                        _unitOfWork.SupplierCosts.Update(existing);
                    }
                }

                foreach (var yt in yachtTours)
                {
                    var supplier = suppliers.FirstOrDefault(s => s.Name == yt.SupplierName);
                    if (supplier == null) { _logger.LogWarning("Supplier not found for yacht tour {Id}", yt.Id); continue; }
                    var existing = await _unitOfWork.SupplierCosts.GetAll(sc => sc.YachtTourId == yt.Id).FirstOrDefaultAsync();
                    if (existing == null)
                    {
                        var sc = new SupplierCost
                        {
                            SupplierId = supplier.Id,
                            YachtTourId = yt.Id,
                            CostAmount = yt.SupplierCost!.Value,
                            Currency = yt.SupplierCurrency ?? yt.Currency ?? "USD",
                            CostType = "BaseCost",
                            Description = $"Imported from yacht tour {yt.Id}",
                            ValidFrom = yt.TourDate,
                            IsActive = true
                        };
                        await _unitOfWork.SupplierCosts.AddAsync(sc);
                    }
                    else
                    {
                        existing.CostAmount = yt.SupplierCost!.Value;
                        existing.Currency = yt.SupplierCurrency ?? yt.Currency ?? existing.Currency;
                        existing.Description = $"Updated from yacht tour {yt.Id}";
                        existing.ValidFrom = yt.TourDate;
                        existing.IsActive = true;
                        _unitOfWork.SupplierCosts.Update(existing);
                    }
                }

                // Note: RestaurantReservationEntity does not include supplier cost fields in domain model;
                // only Transfers, CityTours and YachtTours are supported for supplier cost import at this time.

                // Commit once after batch operations
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Supplier costs synchronized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while syncing supplier costs");
                return ApiResponse<bool>.Fail($"Failed to sync supplier costs: {ex.Message}");
            }
        }
        public async Task<ApiResponse<List<SupplierCost>>> GetAllAsync()
        {
            try
            {
                var list = await _unitOfWork.SupplierCosts.GetAll().ToListAsync();
                return ApiResponse<List<SupplierCost>>.SuccessResponse(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllAsync failed");
                return ApiResponse<List<SupplierCost>>.Fail($"Failed to retrieve supplier costs: {ex.Message}");
            }
        }

        public async Task<ApiResponse<SupplierCost>> GetByIdAsync(int id)
        {
            try
            {
                var entity = await _unitOfWork.SupplierCosts.GetByIdAsync(id);
                if (entity == null) return ApiResponse<SupplierCost>.Fail("Supplier cost not found", statusCode:404);
                return ApiResponse<SupplierCost>.SuccessResponse(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByIdAsync failed");
                return ApiResponse<SupplierCost>.Fail($"Failed to get supplier cost: {ex.Message}");
            }
        }

        public async Task<ApiResponse<SupplierCost>> CreateAsync(GuestFlow.Application.Models.Requests.Supplier.CreateSupplierCostRequest request)
        {
            try
            {
                var entity = new SupplierCost
                {
                    SupplierId = request.SupplierId,
                    TransferId = request.TransferId,
                    CityTourId = request.CityTourId,
                    YachtTourId = request.YachtTourId,
                    CostAmount = request.CostAmount,
                    Currency = request.Currency ?? "USD",
                    CostType = request.CostType,
                    Description = request.Description,
                    ValidFrom = request.ValidFrom,
                    ValidTo = request.ValidTo,
                    IsActive = true
                };
                await _unitOfWork.SupplierCosts.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<SupplierCost>.SuccessResponse(entity, "Supplier cost created");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateAsync failed");
                return ApiResponse<SupplierCost>.Fail($"Failed to create supplier cost: {ex.Message}");
            }
        }

        public async Task<ApiResponse<SupplierCost>> UpdateAsync(int id, GuestFlow.Application.Models.Requests.Supplier.UpdateSupplierCostRequest request)
        {
            try
            {
                var existing = await _unitOfWork.SupplierCosts.GetByIdAsync(id);
                if (existing == null) return ApiResponse<SupplierCost>.Fail("Supplier cost not found", statusCode:404);
                if (request.CostAmount.HasValue) existing.CostAmount = request.CostAmount.Value;
                if (!string.IsNullOrEmpty(request.Currency)) existing.Currency = request.Currency!;
                if (!string.IsNullOrEmpty(request.CostType)) existing.CostType = request.CostType;
                if (!string.IsNullOrEmpty(request.Description)) existing.Description = request.Description;
                if (request.ValidFrom.HasValue) existing.ValidFrom = request.ValidFrom;
                if (request.ValidTo.HasValue) existing.ValidTo = request.ValidTo;
                if (request.IsActive.HasValue) existing.IsActive = request.IsActive.Value;
                _unitOfWork.SupplierCosts.Update(existing);
                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<SupplierCost>.SuccessResponse(existing, "Supplier cost updated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateAsync failed");
                return ApiResponse<SupplierCost>.Fail($"Failed to update supplier cost: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            try
            {
                var existing = await _unitOfWork.SupplierCosts.GetByIdAsync(id);
                if (existing == null) return ApiResponse<bool>.Fail("Supplier cost not found", statusCode:404);
                existing.IsDeleted = true;
                _unitOfWork.SupplierCosts.Update(existing);
                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResponse(true, "Supplier cost deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteAsync failed");
                return ApiResponse<bool>.Fail($"Failed to delete supplier cost: {ex.Message}");
            }
        }
    }
}

