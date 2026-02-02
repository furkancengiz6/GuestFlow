using GuestFlow.Api.Models;
using GuestFlow.Application.Operations.AI;
using GuestFlow.Application.Models.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// AI Destekli Akıllı Concierge Chatbot API
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize] // Giriş yapmış tüm kullanıcılar (Misafir/Personel) erişebilir
    [Tags("AI Concierge")]
    public class AIController : BaseController
    {
        private readonly IAIAssistantService _aiAssistantService;
        private readonly ILogger<AIController> _logger;

        public AIController(IAIAssistantService aiAssistantService, ILogger<AIController> logger)
        {
            _aiAssistantService = aiAssistantService;
            _logger = logger;
        }

        /// <summary>
        /// Chatbot asistanına mesaj gönderir
        /// </summary>
        /// <param name="request">Mesaj ve bağlam bilgileri</param>
        /// <returns>AI yanıtı ve önerilen aksiyonlar</returns>
        /// <response code="200">AI başarılı bir şekilde yanıt döner</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPost("chat")]
        [ProducesResponseType(typeof(GuestFlow.Api.Models.ApiResponse<AIChatResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Chat([FromBody] AIChatRequest request)
        {
            _logger.LogInformation("AI Chat request received for message: {Message}", request.Message);
            
            var response = await _aiAssistantService.ProcessMessageAsync(request);
            
            return Success(response, "AI yanıtı başarıyla oluşturuldu.");
        }

        /// <summary>
        /// Mevcut sohbet bağlamını temizler
        /// </summary>
        /// <param name="guestId">Misafir ID</param>
        [HttpDelete("context/{guestId}")]
        public async Task<IActionResult> ClearContext(int guestId)
        {
            await _aiAssistantService.ClearContextAsync(guestId);
            return Success("Sohbet bağlamı temizlendi.");
        }
    }
}
