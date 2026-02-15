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
            
            if (request.Metadata != null && request.Metadata.TryGetValue("Type", out var type) && type == "SentimentAnalysis")
            {
                responseMessage = @"{
                  ""score"": 0.85,
                  ""label"": ""Positive"",
                  ""confidence"": 0.95,
                  ""keyPhrases"": [""quick check-in"", ""helpful staff"", ""amazing view""],
                  ""emotions"": {""Happiness"": 0.9, ""Satisfaction"": 0.85}
                }";
                
                return new AIChatResponse
                {
                    Response = responseMessage,
                    ConfidenceScore = 0.99f,
                    SuggestedActions = new List<AIAction>()
                };
            }

            if (request.Metadata != null && request.Metadata.TryGetValue("Type", out var riskType) && riskType == "RiskPrediction")
            {
                responseMessage = @"{
                  ""overallRiskScore"": 0.35,
                  ""risks"": [
                    {
                      ""riskType"": ""Challenging Behavior"",
                      ""riskScore"": 0.4,
                      ""severity"": ""Low"",
                      ""description"": ""Guest has mentioned complex requirements in previous chat."",
                      ""factors"": { ""context"": ""chat_patterns"" }
                    }
                  ]
                }";

                return new AIChatResponse
                {
                    Response = responseMessage,
                    ConfidenceScore = 0.95f,
                    SuggestedActions = new List<AIAction>()
                };
            }

            if (request.Metadata != null && request.Metadata.TryGetValue("Type", out var oppType) && oppType == "OpportunityDetection")
            {
                responseMessage = @"{
                  ""opportunities"": [
                    {
                      ""opportunityType"": ""Upsell"",
                      ""description"": ""Guest expressed interest in lüks services during recent interactions."",
                      ""opportunityScore"": 0.9,
                      ""recommendedAction"": ""Offer premium yacht tour package"",
                      ""context"": { ""source"": ""sentiment_analysis"" }
                    },
                    {
                      ""opportunityType"": ""Loyalty"",
                      ""description"": ""High repeat booking potential based on historical satisfaction."",
                      ""opportunityScore"": 0.8,
                      ""recommendedAction"": ""Invite to exclusive rewards program"",
                      ""context"": { ""score"": 0.85 }
                    }
                  ]
                }";

                return new AIChatResponse
                {
                    Response = responseMessage,
                    ConfidenceScore = 0.96f,
                    SuggestedActions = new List<AIAction>()
                };
            }

            if (request.Metadata != null && request.Metadata.TryGetValue("Type", out var satType) && satType == "SatisfactionPrediction")
            {
                responseMessage = @"{
                  ""predictedSatisfaction"": 9.2,
                  ""confidence"": 0.98,
                  ""riskLevel"": ""Low"",
                  ""factors"": { ""loyalty"": ""Frequent guest"", ""recent_sentiment"": 0.9 }
                }";

                return new AIChatResponse
                {
                    Response = responseMessage,
                    ConfidenceScore = 0.97f,
                    SuggestedActions = new List<AIAction>()
                };
            }

            if (request.Metadata != null && request.Metadata.TryGetValue("Type", out var staffType) && staffType == "StaffMatching")
            {
                responseMessage = @"{
                  ""matches"": [
                    {
                      ""staffId"": 1,
                      ""staffName"": ""Furkan Cengiz"",
                      ""compatibilityScore"": 0.98,
                      ""relationshipStrength"": 0.95,
                      ""interactionCount"": 12,
                      ""averageSatisfaction"": 9.8,
                      ""matchReason"": ""Long-term positive history and shared language preferences.""
                    },
                    {
                      ""staffId"": 2,
                      ""staffName"": ""Ayşe Yılmaz"",
                      ""compatibilityScore"": 0.85,
                      ""relationshipStrength"": 0.7,
                      ""interactionCount"": 3,
                      ""averageSatisfaction"": 9.0,
                      ""matchReason"": ""High compatibility with guest's recent service requests.""
                    }
                  ]
                }";

                return new AIChatResponse
                {
                    Response = responseMessage,
                    ConfidenceScore = 0.99f,
                    SuggestedActions = new List<AIAction>()
                };
            }

            if (request.Metadata != null && request.Metadata.TryGetValue("Type", out var serviceType) && serviceType == "ServiceRecommendation")
            {
                responseMessage = @"{
                  ""recommendations"": [
                    {
                      ""serviceType"": ""Upsell"",
                      ""serviceName"": ""Presidential Suite Upgrade"",
                      ""recommendationScore"": 0.95,
                      ""recommendationReason"": ""Guest historically prefers high-floor rooms and has upcoming anniversary."",
                      ""recommendedDate"": """ + DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ssZ") + @""",
                      ""context"": { ""event"": ""anniversary"", ""priority"": ""high"" }
                    },
                    {
                      ""serviceType"": ""CrossSell"",
                      ""serviceName"": ""Private Yacht Dinner"",
                      ""recommendationScore"": 0.88,
                      ""recommendationReason"": ""Matches guest's preference for evening seaside activities."",
                      ""recommendedDate"": """ + DateTime.UtcNow.AddDays(2).ToString("yyyy-MM-ddTHH:mm:ssZ") + @""",
                      ""context"": { ""lifestyle"": ""luxury"" }
                    }
                  ]
                }";

                return new AIChatResponse
                {
                    Response = responseMessage,
                    ConfidenceScore = 0.98f,
                    SuggestedActions = new List<AIAction>()
                };
            }

            if (request.Metadata != null && request.Metadata.TryGetValue("Type", out var occType) && occType == "OccupancyForecast")
            {
                responseMessage = @"{
                  ""forecasts"": [
                    {
                      ""date"": """ + DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd") + @""",
                      ""forecastedOccupancyRate"": 0.82,
                      ""forecastedBookedRooms"": 82,
                      ""confidenceIntervalLower"": 0.78,
                      ""confidenceIntervalUpper"": 0.86
                    },
                    {
                      ""date"": """ + DateTime.UtcNow.AddDays(2).ToString("yyyy-MM-dd") + @""",
                      ""forecastedOccupancyRate"": 0.85,
                      ""forecastedBookedRooms"": 85,
                      ""confidenceIntervalLower"": 0.80,
                      ""confidenceIntervalUpper"": 0.90
                    }
                  ]
                }";

                return new AIChatResponse
                {
                    Response = responseMessage,
                    ConfidenceScore = 0.96f,
                    SuggestedActions = new List<AIAction>()
                };
            }

            if (request.Metadata != null && request.Metadata.TryGetValue("Type", out var revType) && revType == "RevenueForecast")
            {
                responseMessage = @"{
                  ""forecasts"": [
                    {
                      ""date"": """ + DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd") + @""",
                      ""forecastedRevenue"": 145000.00,
                      ""forecastedRevPAR"": 1450.00,
                      ""currency"": ""TRY""
                    },
                    {
                      ""date"": """ + DateTime.UtcNow.AddDays(2).ToString("yyyy-MM-dd") + @""",
                      ""forecastedRevenue"": 152000.00,
                      ""forecastedRevPAR"": 1520.00,
                      ""currency"": ""TRY""
                    }
                  ]
                }";

                return new AIChatResponse
                {
                    Response = responseMessage,
                    ConfidenceScore = 0.97f,
                    SuggestedActions = new List<AIAction>()
                };
            }

            if (request.Metadata != null && request.Metadata.TryGetValue("Type", out var noteType) && noteType == "NoteAnalysis")
            {
                responseMessage = @"{
                  ""insights"": [
                    {
                      ""type"": ""Preference"",
                      ""category"": ""Housekeeping"",
                      ""value"": ""Guest prefers extra towels and room temperature set to 22C."",
                      ""sentiment"": 0.6,
                      ""satisfaction"": 9.0
                    },
                    {
                      ""type"": ""Service"",
                      ""category"": ""Dining"",
                      ""value"": ""Guest mentioned a slight delay in room service breakfast."",
                      ""sentiment"": -0.2,
                      ""satisfaction"": 6.0
                    }
                  ],
                  ""overallSatisfaction"": 7.5
                }";

                return new AIChatResponse
                {
                    Response = responseMessage,
                    ConfidenceScore = 0.95f,
                    SuggestedActions = new List<AIAction>()
                };
            }

            if (request.Metadata != null && request.Metadata.TryGetValue("Type", out var proactiveType) && proactiveType == "ProactiveRefinement")
            {
                responseMessage = @"{
                  ""refined"": [
                    {
                      ""title"": ""Personalized Gourmet Experience Suggestion"",
                      ""description"": ""Given the guest's preference for local cuisine and their upcoming anniversary, we recommend arranging a private rooftop dinner featuring a curated local tasting menu."",
                      ""recommendedAction"": ""Share the curated menu with the guest and offer a preferred reservation time.""
                    },
                    {
                      ""title"": ""Priority Wellness Session Request"",
                      ""description"": ""The guest frequently visits the spa in the mornings. We should proactively offer a 9:00 AM slot for the newly opened salt room therapy."",
                      ""recommendedAction"": ""Send a personalized notification about the available morning slot.""
                    }
                  ]
                }";

                return new AIChatResponse
                {
                    Response = responseMessage,
                    ConfidenceScore = 0.92f,
                    SuggestedActions = new List<AIAction>()
                };
            }

            if (request.Metadata != null && request.Metadata.TryGetValue("Type", out var alertType) && alertType == "AlertRefinement")
            {
                responseMessage = @"{
                  ""refined"": [
                    {
                      ""title"": ""High Risk of Service Dissatisfaction"",
                      ""description"": ""Sentiment analysis indicates frustration regarding recent room service delays. This aligns with historical sensitivity to time-based service."",
                      ""recommendedIntervention"": ""Assign a dedicated concierge to personally oversee their next order and include a complimentary amenity as a gesture of goodwill.""
                    }
                  ]
                }";

                return new AIChatResponse
                {
                    Response = responseMessage,
                    ConfidenceScore = 0.94f,
                    SuggestedActions = new List<AIAction>()
                };
            }

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
            }
            else if (messageLower.Contains("çıkış") || messageLower.Contains("check-out") || messageLower.Contains("ayrılmak"))
            {
                if (request.GuestId.HasValue)
                {
                    // Trigger the real command handler
                    var result = await _commandHandler.ExecuteCommandAsync("CheckOut", new { guestId = request.GuestId.Value });
                    responseMessage = result.Message;
                    
                    if (result.Success)
                    {
                        suggestedActions.Add(new AIAction { ActionType = "Navigate", Description = "Faturamı Görüntüle", Parameters = new Dictionary<string, object> { { "screen", "Invoices" } } });
                    }
                }
                else
                {
                    responseMessage = "Hangi misafir için çıkış işlemi yapmak istiyorsunuz? Lütfen oda numaranızı veya adınızı belirtin.";
                }
            }
            else if (messageLower.Contains("geç çıkış") || messageLower.Contains("late checkout") || messageLower.Contains("geç ayrılmak"))
            {
                if (request.GuestId.HasValue)
                {
                    var result = await _commandHandler.ExecuteCommandAsync("LateCheckout", new { guestId = request.GuestId.Value, time = "14:00" });
                    responseMessage = result.Message;
                }
                else
                {
                    responseMessage = "Geç çıkış talebiniz için lütfen oda numaranızı belirtin.";
                }
            }
            else if (messageLower.Contains("havaalanı transfer") || messageLower.Contains("vip transfer") || (messageLower.Contains("transfer") && messageLower.Contains("ayarla")))
            {
                if (request.GuestId.HasValue)
                {
                    // Simulate intent parsing
                    string dest = messageLower.Contains("havaalanı") ? "Airport" : "City Center";
                    var result = await _commandHandler.ExecuteCommandAsync("BookTransfer", new { guestId = request.GuestId.Value, destination = dest });
                    responseMessage = result.Message;
                }
                else
                {
                    responseMessage = "Transfer rezervasyonu için lütfen misafir bilgilerinizi paylaşın.";
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
