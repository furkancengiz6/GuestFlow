using GuestFlow.Application.Models;
using GuestFlow.Application.Types;
using GuestFlow.Application.Operations.Common;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;

namespace GuestFlow.Application.Operations.ServiceInfoPdf
{
    /// <summary>
    /// Service Information PDF Service Interface.
    /// 
    /// SERVICE INFORMATION PDF REALITY (LOCKED PRODUCT DECISION):
    /// - A NON-FINANCIAL service info PDF exists
    /// - Generated automatically when service is created
    /// - Contains date, time, pickup, notes, reminders
    /// - Contains NO price, NO payment info
    /// - Is NOT an invoice
    /// - May be regenerated when service details change
    /// </summary>
    public interface IServiceInfoPdfService
    {
        /// <summary>
        /// Generate a Service Information PDF for a Transfer.
        /// Contains operational details only - NO prices, NO payment info.
        /// </summary>
        Task<ServiceMessage<ServiceInfoPdfResult>> GenerateTransferInfoPdfAsync(int transferId);
        
        /// <summary>
        /// Generate a Service Information PDF for a City Tour.
        /// Contains operational details only - NO prices, NO payment info.
        /// </summary>
        Task<ServiceMessage<ServiceInfoPdfResult>> GenerateCityTourInfoPdfAsync(int cityTourId);
        
        /// <summary>
        /// Generate a Service Information PDF for a Yacht Tour.
        /// Contains operational details only - NO prices, NO payment info.
        /// </summary>
        Task<ServiceMessage<ServiceInfoPdfResult>> GenerateYachtTourInfoPdfAsync(int yachtTourId);
        
        /// <summary>
        /// Regenerate Service Information PDF when service details change.
        /// Previous PDFs are kept for historical reference.
        /// </summary>
        Task<ServiceMessage<ServiceInfoPdfResult>> RegenerateServiceInfoPdfAsync(string serviceType, int serviceId);
    }

    /// <summary>
    /// Result of Service Information PDF generation
    /// </summary>
    public class ServiceInfoPdfResult
    {
        /// <summary>
        /// URL to the generated PDF
        /// </summary>
        public string PdfUrl { get; set; } = string.Empty;
        
        /// <summary>
        /// When the PDF was generated
        /// </summary>
        public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Service type (Transfer, CityTour, YachtTour)
        /// </summary>
        public string ServiceType { get; set; } = string.Empty;
        
        /// <summary>
        /// Service ID
        /// </summary>
        public int ServiceId { get; set; }
    }
}

