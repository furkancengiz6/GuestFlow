using GuestFlow.Application.Models.Responses.Profitability;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Operations;
using GuestFlow.Domain.UnitOfWork;
using GuestFlow.Application.Operations.Currency;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Application.Operations.Profitability
{
    public class ProfitabilityService : IProfitabilityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrencyService _currencyService;

        public ProfitabilityService(IUnitOfWork unitOfWork, ICurrencyService currencyService)
        {
            _unitOfWork = unitOfWork;
            _currencyService = currencyService;
        }

        public async Task<ApiResponse<ProfitabilityReport>> GetProfitabilityReportAsync(
            DateTime startDate, DateTime endDate, string? supplierId = null)
        {
            try
            {
                var report = new ProfitabilityReport
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    GeneratedAt = DateTime.UtcNow
                };

                // Get all transfers with costs in date range
                var transfers = await GetTransfersWithCostsAsync(startDate, endDate, supplierId);
                var tours = await GetToursWithCostsAsync(startDate, endDate, supplierId);
                var reservations = await GetReservationsWithCostsAsync(startDate, endDate, supplierId);

                // Calculate totals
                report.TotalRevenue = transfers.Sum(t => t.Revenue) +
                                    tours.Sum(t => t.Revenue) +
                                    reservations.Sum(r => r.Revenue);

                report.TotalCost = transfers.Sum(t => t.Cost) +
                                 tours.Sum(t => t.Cost) +
                                 reservations.Sum(r => r.Cost);

                report.TotalProfit = report.TotalRevenue - report.TotalCost;
                report.ProfitMargin = report.TotalRevenue > 0 ?
                    (report.TotalProfit / report.TotalRevenue) * 100 : 0;

                // Group by supplier
                report.SupplierBreakdown = await GetSupplierBreakdownAsync(
                    transfers, tours, reservations);

                // Group by service type
                report.ServiceTypeBreakdown = GetServiceTypeBreakdown(
                    transfers, tours, reservations);

                return ApiResponse<ProfitabilityReport>.SuccessResponse(report);
            }
            catch (Exception ex)
            {
                return ApiResponse<ProfitabilityReport>.Fail($"Failed to generate profitability report: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<SupplierProfitability>>> GetTopSuppliersByProfitAsync(
            DateTime startDate, DateTime endDate, int topCount = 10)
        {
            try
            {
                var transfers = await GetTransfersWithCostsAsync(startDate, endDate);
                var tours = await GetToursWithCostsAsync(startDate, endDate);
                var reservations = await GetReservationsWithCostsAsync(startDate, endDate);

                var supplierProfits = new Dictionary<int, SupplierProfitability>();

                // Aggregate data by supplier
                foreach (var transfer in transfers)
                {
                    if (!supplierProfits.ContainsKey(transfer.SupplierId))
                    {
                        supplierProfits[transfer.SupplierId] = new SupplierProfitability
                        {
                            SupplierId = transfer.SupplierId,
                            SupplierName = transfer.SupplierName
                        };
                    }
                    supplierProfits[transfer.SupplierId].Revenue += transfer.Revenue;
                    supplierProfits[transfer.SupplierId].Cost += transfer.Cost;
                }

                // Calculate profit and margin for each supplier
                var result = supplierProfits.Values
                    .Select(s =>
                    {
                        s.Profit = s.Revenue - s.Cost;
                        s.ProfitMargin = s.Revenue > 0 ? (s.Profit / s.Revenue) * 100 : 0;
                        return s;
                    })
                    .OrderByDescending(s => s.Profit)
                    .Take(topCount)
                    .ToList();

                return ApiResponse<List<SupplierProfitability>>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<SupplierProfitability>>.Fail($"Failed to get top suppliers: {ex.Message}");
            }
        }

        private async Task<List<TransferProfitability>> GetTransfersWithCostsAsync(
            DateTime startDate, DateTime endDate, string? supplierId = null)
        {
            var transfers = await _unitOfWork.Transfers.GetAll(
                t => t.CreatedDate >= startDate &&
                     t.CreatedDate <= endDate &&
                     t.Status == "Completed" &&
                     (supplierId == null || t.SupplierName == supplierId))
                .ToListAsync();

            return transfers.Select(t => new TransferProfitability
            {
                Id = t.Id,
                SupplierId = 0, // Will be populated from supplier cost
                SupplierName = t.SupplierName ?? "Unknown",
                Revenue = t.FinalPrice,
                Cost = t.SupplierCost ?? 0m,
                Currency = t.Currency ?? "USD"
            }).ToList();
        }

        private async Task<List<TourProfitability>> GetToursWithCostsAsync(
            DateTime startDate, DateTime endDate, string? supplierId = null)
        {
            var cityTours = await _unitOfWork.CityTours.GetAll(
                t => t.TourDate >= startDate &&
                     t.TourDate <= endDate &&
                     t.Status == "Completed")
                .ToListAsync();

            var yachtTours = await _unitOfWork.YachtTours.GetAll(
                t => t.TourDate >= startDate &&
                     t.TourDate <= endDate &&
                     t.Status == "Completed")
                .ToListAsync();

            var result = new List<TourProfitability>();

            // Add city tours
            result.AddRange(cityTours.Select(t => new TourProfitability
            {
                Id = t.Id,
                Type = "CityTour",
                SupplierId = 0,
                SupplierName = "City Tour Supplier",
                Revenue = t.FinalPrice,
                Cost = t.SupplierCost ?? 0m,
                Currency = t.Currency ?? "USD"
            }));

            // Add yacht tours
            result.AddRange(yachtTours.Select(t => new TourProfitability
            {
                Id = t.Id,
                Type = "YachtTour",
                SupplierId = 0,
                SupplierName = "Yacht Tour Supplier",
                Revenue = t.FinalPrice,
                Cost = t.SupplierCost ?? 0m,
                Currency = t.Currency ?? "USD"
            }));

            return result;
        }

        private async Task<List<ReservationProfitability>> GetReservationsWithCostsAsync(
            DateTime startDate, DateTime endDate, string? supplierId = null)
        {
            var reservations = await _unitOfWork.RestaurantReservations.GetAll(
                r => r.CreatedDate >= startDate &&
                     r.CreatedDate <= endDate &&
                     r.Status == GuestFlow.Domain.Entities.Enum.ReservationStatus.Completed)
                .ToListAsync();

            return reservations.Select(r => new ReservationProfitability
            {
                Id = r.Id,
                SupplierId = 0,
                SupplierName = "Restaurant Supplier",
                Revenue = 0m,
                Cost = 0m,
                Currency = "USD"
            }).ToList();
        }

        private async Task<List<SupplierBreakdown>> GetSupplierBreakdownAsync(
            List<TransferProfitability> transfers,
            List<TourProfitability> tours,
            List<ReservationProfitability> reservations)
        {
            var breakdown = new Dictionary<string, SupplierBreakdown>();

            // Aggregate transfers
            foreach (var transfer in transfers)
            {
                if (!breakdown.ContainsKey(transfer.SupplierName))
                {
                    breakdown[transfer.SupplierName] = new SupplierBreakdown
                    {
                        SupplierName = transfer.SupplierName,
                        ServiceCount = 0,
                        Revenue = 0,
                        Cost = 0
                    };
                }
                breakdown[transfer.SupplierName].ServiceCount++;
                breakdown[transfer.SupplierName].Revenue += transfer.Revenue;
                breakdown[transfer.SupplierName].Cost += transfer.Cost;
            }

            // Calculate profit margins
            foreach (var item in breakdown.Values)
            {
                item.Profit = item.Revenue - item.Cost;
                item.ProfitMargin = item.Revenue > 0 ? (item.Profit / item.Revenue) * 100 : 0;
            }

            return breakdown.Values.OrderByDescending(b => b.Profit).ToList();
        }

        private List<ServiceTypeBreakdown> GetServiceTypeBreakdown(
            List<TransferProfitability> transfers,
            List<TourProfitability> tours,
            List<ReservationProfitability> reservations)
        {
            return new List<ServiceTypeBreakdown>
            {
                new ServiceTypeBreakdown
                {
                    ServiceType = "Transfer",
                    ServiceCount = transfers.Count,
                    Revenue = transfers.Sum(t => t.Revenue),
                    Cost = transfers.Sum(t => t.Cost)
                },
                new ServiceTypeBreakdown
                {
                    ServiceType = "Tour",
                    ServiceCount = tours.Count,
                    Revenue = tours.Sum(t => t.Revenue),
                    Cost = tours.Sum(t => t.Cost)
                },
                new ServiceTypeBreakdown
                {
                    ServiceType = "Restaurant",
                    ServiceCount = reservations.Count,
                    Revenue = reservations.Sum(r => r.Revenue),
                    Cost = reservations.Sum(r => r.Cost)
                }
            };
        }
    }
}