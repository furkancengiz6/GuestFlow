using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using GuestFlow.Application.Models.AI;

namespace GuestFlow.Application.Operations.AI
{
    /// <summary>
    /// OpenAI API entegrasyonu için temel adaptör (Geliştirme aşamasında mock olarak davranır)
    /// </summary>
    public class OpenAIAssistantAdapter : IAIAssistantService
    {
        private readonly ILogger<OpenAIAssistantAdapter> _logger;
        private readonly ContextRetriever _contextRetriever;
        private readonly IPIIMaskingService _piiMaskingService;
        private readonly IAICommandHandler _commandHandler;

        public OpenAIAssistantAdapter(
            ILogger<OpenAIAssistantAdapter> logger, 
            ContextRetriever contextRetriever,
            IPIIMaskingService piiMaskingService,
            IAICommandHandler commandHandler)
        {
            _logger = logger;
            _contextRetriever = contextRetriever;
            _piiMaskingService = piiMaskingService;
            _commandHandler = commandHandler;
        }

        public async Task<AIChatResponse> ProcessMessageAsync(AIChatRequest request)
        {
            _logger.LogInformation("Processing AI message for Guest: {GuestId}", request.GuestId);

            // Mask input message for privacy
            string maskedInput = _piiMaskingService.MaskPII(request.Message);

            // RAG: Get context from DB
            string context = string.Empty;
            if (request.GuestId.HasValue)
            {
                context = await _contextRetriever.GetGuestContextAsync(request.GuestId.Value);
                context = _piiMaskingService.MaskPII(context); // Mask context for safety
            }

            // Mocking OpenAI call for now
            // Future step: Implement actual OpenAI/Azure OpenAI API call here using HttpClient or SDK
            
            string responseMessage = "Merhaba! Size nasıl yardımcı olabilirim?";
            var suggestedActions = new List<AIAction>();

            var messageLower = request.Message.ToLower();

            // VIP Check
            bool isVip = context.Contains("VIP Durumu: Evet");

            if (messageLower.Contains("transfer") || messageLower.Contains("ulaşım"))
            {
                if (context.Contains("Transfer"))
                {
                    responseMessage = $"Mevcut transfer rezervasyonlarınızı görüyorum. {(isVip ? "VIP statünüz gereği en konforlu araçlarımızı sizin için hazırlatabilirim. " : "")}Detayları incelemek mi istersiniz yoksa yeni bir plan mı yapalım?";
                    suggestedActions.Add(new AIAction { ActionType = "ShowTransfers", Description = "Transferlerimi Göster" });
                }
                else
                {
                    responseMessage = "Şu an planlanmış bir transferiniz bulunmuyor. Havaalanı veya şehir içi ulaşım için bir rezervasyon oluşturmamı ister misiniz?";
                    suggestedActions.Add(new AIAction { ActionType = "CreateTransfer", Description = "Yeni Transfer Planla" });
                }
            }
            else if (messageLower.Contains("yemek") || messageLower.Contains("restoran") || messageLower.Contains("açım"))
            {
                if (context.Contains("Diyet Tercihleri") || context.Contains("Alerjiler"))
                {
                    responseMessage = "Beslenme tercihlerinizi ve varsa alerjilerinizi dikkate alarak size en uygun restoranlarımızı önerebilirim. Gurme lezzetlerimizi keşfetmek ister misiniz?";
                }
                else
                {
                    responseMessage = $"Otelimizde bulunan {(isVip ? "exclusive " : "")}restoranlarımızda size harika bir akşam yemeği organize edebilirim. Deniz mahsulleri mi yoksa yerel mutfak mı daha ilgi çekici?";
                }
                suggestedActions.Add(new AIAction { ActionType = "ShowRestaurants", Description = "Restoranları Listele" });
            }
            else if (messageLower.Contains("tur") || messageLower.Contains("gezi") || messageLower.Contains("aktivite"))
            {
                if (context.Contains("Yat Turu") || context.Contains("Şehir Turu"))
                {
                    responseMessage = "Geçmişteki turlarınızda sizi tekrar ağırlamaktan mutluluk duyarız. Daha önce deneyimlemediğiniz farklı bir rota veya özel bir etkinlik planlayalım mı?";
                }
                else if (context.Contains("İlgi Alanları"))
                {
                    responseMessage = $"İlgi alanlarınıza bakılırsa, sizin için hazırladığım kişiselleştirilmiş aktivite listesini incelemek isteyebilirsiniz. {(isVip ? "VIP üyelere özel kapalı turlarımız ilginizi çekebilir." : "")}";
                }
                else
                {
                    responseMessage = "Bölgemizi keşfetmek için harika tur seçeneklerimiz var. Şehir turları veya lüks yat gezileri hakkında bilgi almak ister misiniz?";
                }
                suggestedActions.Add(new AIAction { ActionType = "ShowTours", Description = "Turları Keşfet" });
            }
            else if (messageLower.Contains("çıkış") || messageLower.Contains("check-out"))
            {
                if (request.GuestId.HasValue)
                {
                    var result = await _commandHandler.ExecuteCommandAsync("CheckOut", new { guestId = request.GuestId.Value });
                    responseMessage = result.Message;
                }
                else
                {
                    responseMessage = "Hangi misafir için çıkış işlemi yapmak istiyorsunuz?";
                }
            }
            else
            {
                responseMessage = $"Merhaba { (context.Contains("Misafir Adı: ") ? context.Split('\n').First(l => l.StartsWith("Misafir Adı: ")).Split(':')[1].Trim() : "") }! Size nasıl yardımcı olabilirim? {(isVip ? "Size özel hizmetlerimizi hatırlatmamı ister misiniz?" : "")}";
            }

            return new AIChatResponse
            {
                Response = responseMessage,
                ConfidenceScore = 0.98f,
                SuggestedActions = suggestedActions
            };
        }

        public Task ClearContextAsync(int guestId)
        {
            _logger.LogInformation("Clearing AI context for Guest: {GuestId}", guestId);
            return Task.CompletedTask;
        }
    }
}
