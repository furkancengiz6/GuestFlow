using GuestFlow.Api.Filters;
using GuestFlow.Application.Operations.Invoice;
using GuestFlow.Application.Operations.Invoice.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")]
    [TypeFilter(typeof(LoggingFilter))]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoicesController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

     
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _invoiceService.GetInvoiceById(id);
            return result == null ? NotFound("Fatura bulunamadı.") : Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetInvoices()
        {
            var result = await _invoiceService.GetInvoices();
            return Ok(result);
        }

      
        [HttpGet("by-guest/{guestId}")]
        public async Task<IActionResult> GetInvoicesByGuestId(int guestId)
        {
            var result = await _invoiceService.GetInvoicesByGuestId(guestId);
            return Ok(result);
        }
    }
}