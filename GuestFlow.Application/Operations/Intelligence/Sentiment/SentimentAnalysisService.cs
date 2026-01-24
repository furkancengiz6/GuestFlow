// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Domain.UnitOfWork;
using GuestFlow.Application.Operations.Intelligence.Behavioral;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace GuestFlow.Application.Operations.Intelligence.Sentiment
{
    /// <summary>
    /// Sentiment analysis service implementation
    /// Note: This is a basic implementation. For production, integrate with Azure Text Analytics or AWS Comprehend
    /// </summary>
    public class SentimentAnalysisService : ISentimentAnalysisService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBehavioralTrackingService _behavioralTrackingService;
        private readonly ILogger<SentimentAnalysisService> _logger;

        // Basic sentiment keywords (for simple analysis)
        private static readonly Dictionary<string, double> PositiveKeywords = new()
        {
            { "excellent", 0.8 }, { "great", 0.7 }, { "good", 0.6 }, { "wonderful", 0.9 },
            { "amazing", 0.8 }, { "perfect", 0.9 }, { "fantastic", 0.8 }, { "love", 0.9 },
            { "happy", 0.7 }, { "satisfied", 0.7 }, { "pleased", 0.7 }, { "thank", 0.6 },
            { "mükemmel", 0.9 }, { "harika", 0.8 }, { "güzel", 0.6 }, { "beğendim", 0.7 },
            { "teşekkür", 0.6 }, { "çok iyi", 0.7 }, { "süper", 0.8 }
        };

        private static readonly Dictionary<string, double> NegativeKeywords = new()
        {
            { "bad", -0.7 }, { "terrible", -0.9 }, { "awful", -0.8 }, { "horrible", -0.9 },
            { "disappointed", -0.7 }, { "unhappy", -0.7 }, { "angry", -0.8 }, { "frustrated", -0.7 },
            { "poor", -0.6 }, { "worst", -0.9 }, { "hate", -0.8 }, { "complaint", -0.6 },
            { "kötü", -0.7 }, { "berbat", -0.9 }, { "hayal kırıklığı", -0.7 }, { "şikayet", -0.6 },
            { "memnun değilim", -0.7 }, { "beğenmedim", -0.7 }, { "kızgın", -0.8 }
        };

        public SentimentAnalysisService(
            IUnitOfWork unitOfWork,
            IBehavioralTrackingService behavioralTrackingService,
            ILogger<SentimentAnalysisService> logger)
        {
            _unitOfWork = unitOfWork;
            _behavioralTrackingService = behavioralTrackingService;
            _logger = logger;
        }

        public async Task<SentimentAnalysisResult> AnalyzeSentimentAsync(string text, string? language = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    return new SentimentAnalysisResult
                    {
                        SentimentScore = 0.0,
                        SentimentLabel = "Neutral",
                        Confidence = 0.0
                    };
                }

                // Detect language if not provided
                if (string.IsNullOrEmpty(language))
                {
                    language = DetectLanguage(text);
                }

                // Simple keyword-based sentiment analysis
                var sentimentScore = CalculateSentimentScore(text);
                var sentimentLabel = GetSentimentLabel(sentimentScore);
                var confidence = CalculateConfidence(text, sentimentScore);
                var keyPhrases = ExtractKeyPhrases(text);

                var result = new SentimentAnalysisResult
                {
                    SentimentScore = sentimentScore,
                    SentimentLabel = sentimentLabel,
                    Confidence = confidence,
                    Language = language,
                    KeyPhrases = keyPhrases,
                    Emotions = ExtractEmotions(text)
                };

                _logger.LogInformation("Sentiment analyzed: Score={Score}, Label={Label}, Confidence={Confidence}",
                    sentimentScore, sentimentLabel, confidence);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to analyze sentiment");
                return new SentimentAnalysisResult
                {
                    SentimentScore = 0.0,
                    SentimentLabel = "Neutral",
                    Confidence = 0.0
                };
            }
        }

        public async Task<SentimentAnalysisResult> AnalyzeCommunicationSentimentAsync(int communicationId, string communicationType)
        {
            try
            {
                string? text = null;

                switch (communicationType.ToUpperInvariant())
                {
                    case "EMAIL":
                        var email = await _unitOfWork.EmailHistories.GetByIdAsync(communicationId);
                        text = email?.Subject ?? "";
                        break;

                    case "SMS":
                        var sms = await _unitOfWork.SmsHistories.GetByIdAsync(communicationId);
                        text = sms?.Message;
                        break;

                    case "WHATSAPP":
                        var whatsapp = await _unitOfWork.WhatsAppHistories.GetByIdAsync(communicationId);
                        text = whatsapp?.Message;
                        break;
                }

                if (string.IsNullOrWhiteSpace(text))
                {
                    return new SentimentAnalysisResult
                    {
                        SentimentScore = 0.0,
                        SentimentLabel = "Neutral",
                        Confidence = 0.0
                    };
                }

                var result = await AnalyzeSentimentAsync(text);

                // Track sentiment as behavioral data
                if (communicationType.ToUpperInvariant() == "EMAIL")
                {
                    var email = await _unitOfWork.EmailHistories.GetByIdAsync(communicationId);
                    if (email != null && email.RelatedEntityType == "Guest" && email.RelatedEntityId.HasValue)
                    {
                        await _behavioralTrackingService.TrackGuestBehaviorAsync(
                            email.RelatedEntityId.Value,
                            "Communication",
                            "Email",
                            text,
                            result.SentimentScore,
                            null
                        );
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to analyze communication sentiment: CommunicationId={CommunicationId}, Type={Type}",
                    communicationId, communicationType);
                return new SentimentAnalysisResult
                {
                    SentimentScore = 0.0,
                    SentimentLabel = "Neutral",
                    Confidence = 0.0
                };
            }
        }

        public async Task<SentimentAnalysisResult> AnalyzeFeedbackSentimentAsync(string feedbackText)
        {
            return await AnalyzeSentimentAsync(feedbackText);
        }

        public async Task<List<SentimentAnalysisResult>> BatchAnalyzeSentimentAsync(List<string> texts)
        {
            var results = new List<SentimentAnalysisResult>();

            foreach (var text in texts)
            {
                var result = await AnalyzeSentimentAsync(text);
                results.Add(result);
            }

            return results;
        }

        public async Task<Dictionary<string, object>> GetGuestSentimentTrendsAsync(int guestId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _unitOfWork.GuestBehaviors
                    .GetAll(b => b.GuestId == guestId && 
                                b.SentimentScore.HasValue && 
                                !b.IsDeleted);

                if (startDate.HasValue)
                    query = query.Where(b => b.BehaviorDate >= startDate.Value);
                if (endDate.HasValue)
                    query = query.Where(b => b.BehaviorDate <= endDate.Value);

                var behaviors = await query
                    .OrderBy(b => b.BehaviorDate)
                    .ToListAsync();

                if (!behaviors.Any())
                {
                    return new Dictionary<string, object>
                    {
                        ["AverageSentiment"] = 0.0,
                        ["Trend"] = "Stable",
                        ["DataPoints"] = 0
                    };
                }

                var sentiments = behaviors.Select(b => b.SentimentScore!.Value).ToList();
                var averageSentiment = sentiments.Average();
                var trend = CalculateTrend(sentiments);

                var trends = new Dictionary<string, object>
                {
                    ["AverageSentiment"] = averageSentiment,
                    ["MinSentiment"] = sentiments.Min(),
                    ["MaxSentiment"] = sentiments.Max(),
                    ["Trend"] = trend,
                    ["DataPoints"] = sentiments.Count,
                    ["PositiveCount"] = sentiments.Count(s => s > 0.3),
                    ["NeutralCount"] = sentiments.Count(s => s >= -0.3 && s <= 0.3),
                    ["NegativeCount"] = sentiments.Count(s => s < -0.3),
                    ["TimeSeries"] = behaviors.Select(b => new
                    {
                        Date = b.BehaviorDate,
                        Sentiment = b.SentimentScore!.Value
                    }).ToList()
                };

                return trends;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get guest sentiment trends: GuestId={GuestId}", guestId);
                return new Dictionary<string, object>();
            }
        }

        private double CalculateSentimentScore(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0.0;

            var lowerText = text.ToLowerInvariant();
            var score = 0.0;
            var wordCount = 0;

            // Check positive keywords
            foreach (var keyword in PositiveKeywords)
            {
                var matches = Regex.Matches(lowerText, @"\b" + Regex.Escape(keyword.Key) + @"\b", RegexOptions.IgnoreCase);
                if (matches.Count > 0)
                {
                    score += keyword.Value * matches.Count;
                    wordCount += matches.Count;
                }
            }

            // Check negative keywords
            foreach (var keyword in NegativeKeywords)
            {
                var matches = Regex.Matches(lowerText, @"\b" + Regex.Escape(keyword.Key) + @"\b", RegexOptions.IgnoreCase);
                if (matches.Count > 0)
                {
                    score += keyword.Value * matches.Count;
                    wordCount += matches.Count;
                }
            }

            // Normalize score
            if (wordCount > 0)
            {
                score = score / wordCount;
            }

            // Clamp to -1.0 to 1.0
            return Math.Max(-1.0, Math.Min(1.0, score));
        }

        private string GetSentimentLabel(double sentimentScore)
        {
            return sentimentScore switch
            {
                > 0.3 => "Positive",
                < -0.3 => "Negative",
                _ => "Neutral"
            };
        }

        private double CalculateConfidence(string text, double sentimentScore)
        {
            // Simple confidence calculation based on keyword matches
            var lowerText = text.ToLowerInvariant();
            var keywordMatches = 0;

            foreach (var keyword in PositiveKeywords.Keys.Concat(NegativeKeywords.Keys))
            {
                if (lowerText.Contains(keyword))
                {
                    keywordMatches++;
                }
            }

            // Confidence increases with more keyword matches
            var confidence = Math.Min(1.0, keywordMatches / 5.0);
            
            // If sentiment is very strong (close to -1 or 1), increase confidence
            if (Math.Abs(sentimentScore) > 0.7)
            {
                confidence = Math.Min(1.0, confidence + 0.2);
            }

            return confidence;
        }

        private List<string> ExtractKeyPhrases(string text)
        {
            // Simple key phrase extraction (for production, use NLP libraries)
            var phrases = new List<string>();
            
            // Extract common phrases
            var commonPhrases = new[] { "very good", "very bad", "not satisfied", "highly recommend", 
                "çok iyi", "çok kötü", "memnun değilim", "kesinlikle öneririm" };

            var lowerText = text.ToLowerInvariant();
            foreach (var phrase in commonPhrases)
            {
                if (lowerText.Contains(phrase))
                {
                    phrases.Add(phrase);
                }
            }

            return phrases;
        }

        private Dictionary<string, double> ExtractEmotions(string text)
        {
            // Simple emotion detection (for production, use advanced NLP)
            var emotions = new Dictionary<string, double>();
            var lowerText = text.ToLowerInvariant();

            // Happiness indicators
            if (lowerText.Contains("happy") || lowerText.Contains("mutlu") || lowerText.Contains("sevinç"))
                emotions["Happiness"] = 0.7;

            // Sadness indicators
            if (lowerText.Contains("sad") || lowerText.Contains("üzgün") || lowerText.Contains("hayal kırıklığı"))
                emotions["Sadness"] = 0.7;

            // Anger indicators
            if (lowerText.Contains("angry") || lowerText.Contains("kızgın") || lowerText.Contains("sinir"))
                emotions["Anger"] = 0.7;

            // Satisfaction indicators
            if (lowerText.Contains("satisfied") || lowerText.Contains("memnun") || lowerText.Contains("beğendim"))
                emotions["Satisfaction"] = 0.8;

            return emotions;
        }

        private string DetectLanguage(string text)
        {
            // Simple language detection (for production, use proper language detection library)
            var turkishChars = new[] { "ç", "ğ", "ı", "ö", "ş", "ü", "Ç", "Ğ", "İ", "Ö", "Ş", "Ü" };
            var hasTurkishChars = turkishChars.Any(c => text.Contains(c));

            return hasTurkishChars ? "tr-TR" : "en-US";
        }

        private string CalculateTrend(List<double> sentiments)
        {
            if (sentiments.Count < 2)
                return "Stable";

            // Simple trend calculation: compare first half vs second half
            var midPoint = sentiments.Count / 2;
            var firstHalf = sentiments.Take(midPoint).Average();
            var secondHalf = sentiments.Skip(midPoint).Average();

            var difference = secondHalf - firstHalf;

            return difference switch
            {
                > 0.2 => "Improving",
                < -0.2 => "Declining",
                _ => "Stable"
            };
        }
    }
}
