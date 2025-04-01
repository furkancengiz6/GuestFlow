using GuestFlow.Application.Operations.Invoice.Dtos;
using GuestFlow.Application.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Invoice
{
    public interface IInvoiceService
    {
        Task<ServiceMessage> AddInvoice(AddInvoiceDto invoice);
        Task<ServiceMessage> UpdateInvoice(UpdateInvoiceDto invoice);
        Task<ServiceMessage> DeleteInvoice(int id);
        Task<GetInvoiceDto> GetInvoiceById(int id);
        Task<List<GetInvoiceDto>> GetInvoices();
        Task<List<GetInvoiceDto>> GetInvoicesByGuestId(int guestId);
    }
}