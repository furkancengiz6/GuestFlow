using GuestFlow.Application.Operations.Invoice.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Invoice
{
    public interface IInvoiceService
    {
        Task<GetInvoiceDto> GetInvoiceById(int id);
        Task<List<GetInvoiceDto>> GetInvoices();
        Task<List<GetInvoiceDto>> GetInvoicesByGuestId(int guestId);
    }
}