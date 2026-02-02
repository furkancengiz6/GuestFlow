using GuestFlow.Domain.Entities.Core;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Invoice
{
    public interface IPdfService
    {
        /// <summary>
        /// Fatura için PDF oluşturur ve dosya yolunu döndürür
        /// </summary>
        Task<string> GenerateInvoicePdfAsync(InvoicesEntity invoice, GuestEntity guest, PersonnelEntity? personnel = null);

        /// <summary>
        /// Haftalık operasyonel rapor için PDF oluşturur
        /// </summary>
        Task<string> GenerateWeeklyReportPdfAsync(object reportData);
    }
}


