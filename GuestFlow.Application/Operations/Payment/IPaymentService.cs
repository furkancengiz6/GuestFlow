using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Payment.Dtos;
using GuestFlow.Application.Types;

namespace GuestFlow.Application.Operations.Payment
{
    /// <summary>
    /// Ödeme servisi interface'i
    /// </summary>
    public interface IPaymentService
    {
        /// <summary>
        /// Yeni ödeme oluşturur
        /// </summary>
        Task<ServiceMessage<GetPaymentDto>> AddPaymentAsync(AddPaymentDto paymentDto);

        /// <summary>
        /// Ödemeyi günceller
        /// </summary>
        Task<ServiceMessage> UpdatePaymentAsync(UpdatePaymentDto paymentDto);

        /// <summary>
        /// Ödemeyi siler (soft delete)
        /// </summary>
        Task<ServiceMessage> DeletePaymentAsync(int id);

        /// <summary>
        /// Ödeme ID'sine göre ödeme getirir
        /// </summary>
        Task<GetPaymentDto?> GetPaymentByIdAsync(int id);

        /// <summary>
        /// Ödeme detayını getirir
        /// </summary>
        Task<PaymentDetailDto?> GetPaymentDetailAsync(int id);

        /// <summary>
        /// Sayfalanmış ödemeleri getirir
        /// </summary>
        Task<PagedResult<GetPaymentDto>> GetPaymentsPagedAsync(int pageNumber, int pageSize, PaymentFilterParameters? filters = null, SortingParameters? sorting = null);

        /// <summary>
        /// Ödemeyi tamamlar (gateway'den gelen callback için)
        /// </summary>
        Task<ServiceMessage> CompletePaymentAsync(int paymentId, string transactionId, string? gatewayResponse = null);

        /// <summary>
        /// Ödemeyi başarısız olarak işaretler
        /// </summary>
        Task<ServiceMessage> FailPaymentAsync(int paymentId, string? reason = null);

        /// <summary>
        /// Ödemeyi iade eder
        /// </summary>
        Task<ServiceMessage> RefundPaymentAsync(int paymentId, string? refundReason = null);

        /// <summary>
        /// Ödemeyi iptal eder
        /// </summary>
        Task<ServiceMessage> CancelPaymentAsync(int paymentId, string? cancellationReason = null);

        /// <summary>
        /// Misafir ID'sine göre ödemeleri getirir
        /// </summary>
        Task<List<GetPaymentDto>> GetPaymentsByGuestIdAsync(int guestId);

        /// <summary>
        /// Fatura ID'sine göre ödemeleri getirir
        /// </summary>
        Task<List<GetPaymentDto>> GetPaymentsByInvoiceIdAsync(int invoiceId);

        /// <summary>
        /// Duruma göre ödemeleri getirir
        /// </summary>
        Task<List<GetPaymentDto>> GetPaymentsByStatusAsync(string status);

        /// <summary>
        /// Benzersiz ödeme numarası oluşturur
        /// </summary>
        Task<string> GeneratePaymentNumberAsync();
    }
}

