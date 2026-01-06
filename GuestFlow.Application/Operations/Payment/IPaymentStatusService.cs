using GuestFlow.Application.Operations.Payment.Dtos;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Payment
{
    public interface IPaymentStatusService
    {
        /// <summary>
        /// Calculate payment status for a specific service (Transfer/CityTour/YachtTour)
        /// </summary>
        Task<ServicePaymentStatusDto> GetServicePaymentStatusAsync(int serviceId, string serviceType);

        /// <summary>
        /// Calculate payment status for a specific invoice
        /// </summary>
        Task<InvoicePaymentStatusDto> GetInvoicePaymentStatusAsync(int invoiceId);
    }
}
