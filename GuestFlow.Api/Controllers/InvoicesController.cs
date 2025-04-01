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
    [Authorize(Roles = "Staff,Admin")] // Bu controller'a sadece Staff ve Admin rolleri erişebilir.
    [TypeFilter(typeof(LoggingFilter))] // Bu controller'daki tüm işlemler için loglama filtresi uyguluyorum.
    public class InvoicesController : ControllerBase
    {
        // Burada kullanacağım değişkeni tanımlıyorum.
        // _invoiceService: Faturalarla ilgili işlemleri yapmak için kullanıyorum.
        private readonly IInvoiceService _invoiceService;

        // Constructor: Bu sınıf oluşturulurken bağımlılıkları buradan alıyorum.
        public InvoicesController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        // Bu metodumla belirli bir faturayı ID'sine göre getiriyorum.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            // Servisten faturayı ID'sine göre alıyorum.
            var result = await _invoiceService.GetInvoiceById(id);
            // Eğer fatura bulunamazsa, 404 Not Found ile hata mesajı döndürüyorum; bulunursa sonucu JSON formatında döndürüyorum.
            return result == null ? NotFound(new { Message = "Fatura bulunamadı." }) : Ok(result);
        }

        // Bu metodumla tüm faturaları getiriyorum.
        [HttpGet]
        public async Task<IActionResult> GetInvoices()
        {
            // Servisten tüm faturaları alıyorum ve JSON formatında döndürüyorum.
            var result = await _invoiceService.GetInvoices();
            return Ok(result);
        }

        // Bu metodumla belirli bir misafire ait faturaları getiriyorum.
        [HttpGet("by-guest/{guestId}")]
        public async Task<IActionResult> GetInvoicesByGuestId(int guestId)
        {
            // Servisten misafire ait faturaları alıyorum ve JSON formatında döndürüyorum.
            var result = await _invoiceService.GetInvoicesByGuestId(guestId);
            return Ok(result);
        }
    }
}