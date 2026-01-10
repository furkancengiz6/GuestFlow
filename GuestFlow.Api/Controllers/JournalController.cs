using GuestFlow.Application.Models.Responses.Accounting;
using GuestFlow.Application.Operations.Accounting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GuestFlow.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize(Roles = "Staff,Admin")]
    public class JournalController : BaseController
    {
        private readonly IJournalService _journalService;

        public JournalController(IJournalService journalService)
        {
            _journalService = journalService;
        }

        [HttpGet("preview")]
        public async Task<IActionResult> Preview([FromQuery] int invoiceId)
        {
            var result = await _journalService.GenerateJournalPreviewAsync(invoiceId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("post")]
        public async Task<IActionResult> Post([FromBody] JournalPostRequest request)
        {
            var result = await _journalService.PostJournalAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}

