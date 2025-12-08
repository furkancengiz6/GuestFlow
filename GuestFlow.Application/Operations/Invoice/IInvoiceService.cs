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
        Task<GetInvoiceDto> GetInvoiceById(int id);
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
    }
}