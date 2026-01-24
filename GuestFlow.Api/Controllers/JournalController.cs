using GuestFlow.Application.Models.Responses.Accounting;
using GuestFlow.Application.Operations.Accounting;
using GuestFlow.Api.Models;
using Microsoft.AspNetCore.Http;
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
        [ProducesResponseType(typeof(ApiResponse<JournalPreviewResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Preview([FromQuery] int invoiceId)
        {
            var result = await _journalService.GenerateJournalPreviewAsync(invoiceId);
            return result.Success
                ? Success(result.Data, result.Message)
                : Error(result.Message, result.StatusCode == 0 ? 400 : result.StatusCode, result.Errors);
        }

        [HttpPost("post")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post([FromBody] JournalPostRequest request)
        {
            var result = await _journalService.PostJournalAsync(request);
            return result.Success
                ? Success(result.Data, result.Message)
                : Error(result.Message, result.StatusCode == 0 ? 400 : result.StatusCode, result.Errors);
        }

        [HttpPost("reverse/{journalEntryId:int}")]
        [ProducesResponseType(typeof(ApiResponse<JournalEntryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Reverse([FromRoute] int journalEntryId, [FromBody] ReverseJournalRequest? request = null)
        {
            var result = await _journalService.ReverseJournalEntryAsync(journalEntryId, request?.Description);
            return result.Success
                ? Success(result.Data, result.Message)
                : Error(result.Message, result.StatusCode == 0 ? 400 : result.StatusCode, result.Errors);
        }

        [HttpGet("by-invoice/{invoiceId:int}")]
        [ProducesResponseType(typeof(ApiResponse<JournalEntryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByInvoice([FromRoute] int invoiceId)
        {
            var result = await _journalService.GetJournalByInvoiceAsync(invoiceId);
            return result.Success
                ? Success(result.Data, result.Message)
                : Error(result.Message, result.StatusCode == 0 ? 400 : result.StatusCode, result.Errors);
        }
    }
}

