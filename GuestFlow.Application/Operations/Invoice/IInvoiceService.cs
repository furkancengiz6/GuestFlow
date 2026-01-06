using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Invoice.Dtos;
using GuestFlow.Application.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Invoice
{
    public interface IInvoiceService
    {
        Task<GetInvoiceDto> GetInvoiceDtoById(int id);
        Task<List<GetInvoiceDto>> GetInvoices();
        Task<List<GetInvoiceDto>> GetInvoicesByGuestId(int guestId);
        Task<string> GeneratePdfForInvoiceAsync(int invoiceId);
        
        /// <summary>
        /// Fatura detayını getirir (ilgili veriler ile)
        /// </summary>
        Task<InvoiceDetailDto> GetInvoiceDetailAsync(int id);
        
        /// <summary>
        /// Sayfalanmış, filtrelenmiş ve sıralanmış faturaları getirir
        /// </summary>
        Task<PagedResult<GetInvoiceDto>> GetInvoicesPagedAsync(int pageNumber, int pageSize, InvoiceFilterParameters? filters = null, SortingParameters? sorting = null);
        
        /// <summary>
        /// Fatura istatistiklerini getirir
        /// </summary>
        Task<InvoiceStatisticsDto> GetInvoiceStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
        
        /// <summary>
        /// Faturayı e-posta ile gönderir
        /// </summary>
        Task<ServiceMessage> SendInvoiceByEmailAsync(int invoiceId, string? recipientEmail = null);

        /// <summary>
        /// Get services eligible for invoice creation for a guest
        /// </summary>
        Task<List<EligibleServiceDto>> GetEligibleServicesForInvoiceAsync(int guestId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Create invoice manually with selected services
        /// </summary>
        Task<ServiceMessage<GetInvoiceDto>> CreateInvoiceAsync(CreateInvoiceDto createDto);

        /// <summary>
        /// Update invoice (only allowed for Draft invoices that haven't been PDF generated)
        /// INVOICE IMMUTABILITY: Once PDF is generated, invoices become IMMUTABLE
        /// </summary>
        Task<ServiceMessage<GetInvoiceDto>> UpdateInvoiceAsync(int invoiceId, UpdateInvoiceDto updateDto);

        /// <summary>
        /// Cancel invoice (only allowed for Draft invoices)
        /// INVOICE IMMUTABILITY: Generated invoices cannot be cancelled
        /// </summary>
        Task<ServiceMessage> CancelInvoiceAsync(int invoiceId);
    }
}