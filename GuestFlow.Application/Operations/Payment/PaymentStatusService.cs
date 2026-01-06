using GuestFlow.Application.Operations.Payment.Dtos;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using GuestFlow.Domain.Entities.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Payment
{
    /// <summary>
    /// Canonical payment status calculation service using PaymentEntity as source of truth
    /// </summary>
    public class PaymentStatusService : IPaymentStatusService
    {
        private readonly IRepository<PaymentEntity> _paymentRepository;
        private readonly IRepository<TransferEntity> _transferRepository;
        private readonly IRepository<CityTourEntity> _cityTourRepository;
        private readonly IRepository<YachtTourEntity> _yachtTourRepository;
        private readonly IRepository<InvoicesEntity> _invoiceRepository;
        private readonly ILogger<PaymentStatusService> _logger;

        public PaymentStatusService(
            IRepository<PaymentEntity> paymentRepository,
            IRepository<TransferEntity> transferRepository,
            IRepository<CityTourEntity> cityTourRepository,
            IRepository<YachtTourEntity> yachtTourRepository,
            IRepository<InvoicesEntity> invoiceRepository,
            ILogger<PaymentStatusService> logger)
        {
            _paymentRepository = paymentRepository;
            _transferRepository = transferRepository;
            _cityTourRepository = cityTourRepository;
            _yachtTourRepository = yachtTourRepository;
            _invoiceRepository = invoiceRepository;
            _logger = logger;
        }

        /// <summary>
        /// Calculate payment status for a specific service (Transfer/CityTour/YachtTour)
        /// Only considers completed payments in the same currency as the service
        /// </summary>
        public async Task<ServicePaymentStatusDto> GetServicePaymentStatusAsync(int serviceId, string serviceType)
        {
            try
            {
                // Get service details based on type
                decimal serviceAmount = 0;
                string currency = string.Empty;
                string guestName = string.Empty;
                DateTime serviceDate = DateTime.MinValue;

                switch (serviceType.ToLower())
                {
                    case "transfer":
                        var transfer = await _transferRepository.GetAll()
                            .Include(t => t.Guest)
                            .FirstOrDefaultAsync(t => t.Id == serviceId && !t.IsDeleted);

                        if (transfer == null)
                            return CreateEmptyServiceStatus(serviceId, serviceType, "Transfer bulunamadı");

                        serviceAmount = transfer.FinalPrice;
                        currency = transfer.Currency ?? "TRY";
                        guestName = transfer.Guest?.FullName ?? "Bilinmiyor";
                        serviceDate = transfer.TransferDate;
                        break;

                    case "citytour":
                        var cityTour = await _cityTourRepository.GetAll()
                            .Include(ct => ct.OwnerGuest)
                            .FirstOrDefaultAsync(ct => ct.Id == serviceId && !ct.IsDeleted);

                        if (cityTour == null)
                            return CreateEmptyServiceStatus(serviceId, serviceType, "Şehir turu bulunamadı");

                        serviceAmount = cityTour.FinalPrice;
                        currency = cityTour.Currency ?? "TRY";
                        guestName = cityTour.OwnerGuest?.FullName ?? "Bilinmiyor";
                        serviceDate = cityTour.TourDate;
                        break;

                    case "yachttour":
                        var yachtTour = await _yachtTourRepository.GetAll()
                            .Include(yt => yt.OwnerGuest)
                            .FirstOrDefaultAsync(yt => yt.Id == serviceId && !yt.IsDeleted);

                        if (yachtTour == null)
                            return CreateEmptyServiceStatus(serviceId, serviceType, "Yat turu bulunamadı");

                        serviceAmount = yachtTour.FinalPrice;
                        currency = yachtTour.Currency ?? "TRY";
                        guestName = yachtTour.OwnerGuest?.FullName ?? "Bilinmiyor";
                        serviceDate = yachtTour.TourDate;
                        break;

                    default:
                        return CreateEmptyServiceStatus(serviceId, serviceType, $"Geçersiz servis tipi: {serviceType}");
                }

                // Calculate total paid amount from completed payments (same currency only)
                var paidAmount = await GetPaidAmountForServiceAsync(serviceId, serviceType, currency);
                var remainingAmount = serviceAmount - paidAmount;

                // Calculate paid amounts by currency (currency-safe)
                var paidAmountByCurrency = await GetPaidAmountByCurrencyForServiceAsync(serviceId, serviceType);

                // Build remaining amount by currency (service amount in its currency minus paid amount in that currency)
                var remainingAmountByCurrency = new Dictionary<string, decimal>();
                remainingAmountByCurrency[currency] = serviceAmount - (paidAmountByCurrency.ContainsKey(currency) ? paidAmountByCurrency[currency] : 0);

                // Determine payment status (based on primary currency for backward compatibility)
                var paymentStatus = CalculatePaymentStatus(serviceAmount, paidAmount);

                return new ServicePaymentStatusDto
                {
                    ServiceId = serviceId,
                    ServiceType = serviceType,
                    ServiceAmount = serviceAmount,
                    PaidAmount = paidAmount,
                    PaidAmountByCurrency = paidAmountByCurrency,
                    RemainingAmount = remainingAmount,
                    RemainingAmountByCurrency = remainingAmountByCurrency,
                    Currency = currency,
                    PaymentStatus = paymentStatus,
                    GuestName = guestName,
                    ServiceDate = serviceDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Servis ödeme durumu hesaplanırken hata: ServiceId={serviceId}, ServiceType={serviceType}");
                return CreateEmptyServiceStatus(serviceId, serviceType, "Ödeme durumu hesaplanırken hata oluştu");
            }
        }

        /// <summary>
        /// Calculate payment status for a specific invoice by summing payments for all its services
        /// </summary>
        public async Task<InvoicePaymentStatusDto> GetInvoicePaymentStatusAsync(int invoiceId)
        {
            try
            {
                // Get invoice details
                var invoice = await _invoiceRepository.GetAll()
                    .Include(i => i.InvoiceItems)
                    .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted);

                if (invoice == null)
                {
                    return new InvoicePaymentStatusDto
                    {
                        InvoiceId = invoiceId,
                        InvoiceAmount = 0,
                        PaidAmount = 0,
                        RemainingAmount = 0,
                        Currency = "TRY",
                        PaymentStatus = "Unpaid",
                        PaymentCount = 0,
                        ServiceStatuses = new List<ServicePaymentStatusDto>()
                    };
                }

                var invoiceAmount = invoice.TotalAmount;
                var currency = invoice.Currency ?? "TRY";
                var serviceStatuses = new List<ServicePaymentStatusDto>();

                // Calculate payment status for each service in the invoice
                foreach (var item in invoice.InvoiceItems.Where(ii => !ii.IsDeleted))
                {
                    var serviceStatus = await GetServicePaymentStatusAsync(item.ServiceId, item.ServiceType);
                    serviceStatuses.Add(serviceStatus);
                }

                // Calculate paid amounts by currency for payments directly linked to this invoice
                var paymentsByCurrency = await _paymentRepository.GetAll()
                    .Where(p => p.InvoiceId == invoiceId &&
                               p.Status == PaymentStatus.Completed &&
                               !p.IsDeleted)
                    .GroupBy(p => p.Currency)
                    .Select(g => new
                    {
                        Currency = g.Key,
                        TotalAmount = g.Sum(p => p.Amount)
                    })
                    .ToListAsync();

                var paidAmountByCurrency = paymentsByCurrency.ToDictionary(x => x.Currency, x => x.TotalAmount);

                // Calculate total paid amount from payments directly linked to this invoice (same currency only for backward compatibility)
                var directPayments = paidAmountByCurrency.ContainsKey(currency) ? paidAmountByCurrency[currency] : 0;
                var remainingAmount = invoiceAmount - directPayments;

                // Build remaining amount by currency
                var remainingAmountByCurrency = new Dictionary<string, decimal>();
                remainingAmountByCurrency[currency] = remainingAmount;

                var paymentStatus = CalculatePaymentStatus(invoiceAmount, directPayments);

                return new InvoicePaymentStatusDto
                {
                    InvoiceId = invoiceId,
                    InvoiceAmount = invoiceAmount,
                    PaidAmount = directPayments,
                    PaidAmountByCurrency = paidAmountByCurrency,
                    RemainingAmount = remainingAmount,
                    RemainingAmountByCurrency = remainingAmountByCurrency,
                    Currency = currency,
                    PaymentStatus = paymentStatus,
                    PaymentCount = paymentsByCurrency.Sum(x => x.TotalAmount > 0 ? 1 : 0), // Count currencies with payments
                    ServiceStatuses = serviceStatuses
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fatura ödeme durumu hesaplanırken hata: InvoiceId={invoiceId}");
                return new InvoicePaymentStatusDto
                {
                    InvoiceId = invoiceId,
                    InvoiceAmount = 0,
                    PaidAmount = 0,
                    PaidAmountByCurrency = new Dictionary<string, decimal>(),
                    RemainingAmount = 0,
                    RemainingAmountByCurrency = new Dictionary<string, decimal>(),
                    Currency = "TRY",
                    PaymentStatus = "Unpaid",
                    PaymentCount = 0,
                    ServiceStatuses = new List<ServicePaymentStatusDto>()
                };
            }
        }

        /// <summary>
        /// Calculate total paid amount for a specific service from completed payments (single currency)
        /// </summary>
        private async Task<decimal> GetPaidAmountForServiceAsync(int serviceId, string serviceType, string currency)
        {
            var query = _paymentRepository.GetAll()
                .Where(p => p.Status == PaymentStatus.Completed &&
                           p.Currency == currency &&
                           !p.IsDeleted);

            switch (serviceType.ToLower())
            {
                case "transfer":
                    query = query.Where(p => p.TransferId == serviceId);
                    break;
                case "citytour":
                    query = query.Where(p => p.CityTourId == serviceId);
                    break;
                case "yachttour":
                    query = query.Where(p => p.YachtTourId == serviceId);
                    break;
                default:
                    return 0;
            }

            return await query.SumAsync(p => p.Amount);
        }

        /// <summary>
        /// Calculate paid amounts by currency for a specific service from completed payments
        /// </summary>
        private async Task<Dictionary<string, decimal>> GetPaidAmountByCurrencyForServiceAsync(int serviceId, string serviceType)
        {
            var query = _paymentRepository.GetAll()
                .Where(p => p.Status == PaymentStatus.Completed && !p.IsDeleted);

            switch (serviceType.ToLower())
            {
                case "transfer":
                    query = query.Where(p => p.TransferId == serviceId);
                    break;
                case "citytour":
                    query = query.Where(p => p.CityTourId == serviceId);
                    break;
                case "yachttour":
                    query = query.Where(p => p.YachtTourId == serviceId);
                    break;
                default:
                    return new Dictionary<string, decimal>();
            }

            var paymentsByCurrency = await query
                .GroupBy(p => p.Currency)
                .Select(g => new
                {
                    Currency = g.Key,
                    TotalAmount = g.Sum(p => p.Amount)
                })
                .ToListAsync();

            return paymentsByCurrency.ToDictionary(x => x.Currency, x => x.TotalAmount);
        }

        /// <summary>
        /// Calculate payment status based on service amount and paid amount
        /// </summary>
        private string CalculatePaymentStatus(decimal serviceAmount, decimal paidAmount)
        {
            if (paidAmount == 0)
                return "Unpaid";
            else if (paidAmount >= serviceAmount)
                return "Paid";
            else
                return "PartiallyPaid";
        }

        /// <summary>
        /// Create empty service payment status for error cases
        /// </summary>
        private ServicePaymentStatusDto CreateEmptyServiceStatus(int serviceId, string serviceType, string errorMessage)
        {
            _logger.LogWarning($"{serviceType} {serviceId} için: {errorMessage}");

            return new ServicePaymentStatusDto
            {
                ServiceId = serviceId,
                ServiceType = serviceType,
                ServiceAmount = 0,
                PaidAmount = 0,
                RemainingAmount = 0,
                Currency = "TRY",
                PaymentStatus = "Unpaid",
                GuestName = "Bilinmiyor",
                ServiceDate = DateTime.MinValue
            };
        }
    }
}
