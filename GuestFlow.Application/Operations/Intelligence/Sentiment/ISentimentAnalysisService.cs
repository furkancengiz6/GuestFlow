// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace GuestFlow.Application.Operations.Intelligence.Sentiment
{
    /// <summary>
    /// Sentiment analysis service interface - Duygu analizi servisi
    /// </summary>
    public interface ISentimentAnalysisService
    {
        /// <summary>
        /// Analyze text sentiment (metin duygu analizi)
        /// </summary>
        Task<SentimentAnalysisResult> AnalyzeSentimentAsync(string text, string? language = null);

        /// <summary>
        /// Analyze communication sentiment (Email, SMS, WhatsApp)
        /// </summary>
        Task<SentimentAnalysisResult> AnalyzeCommunicationSentimentAsync(int communicationId, string communicationType);

        /// <summary>
        /// Analyze guest feedback sentiment
        /// </summary>
        Task<SentimentAnalysisResult> AnalyzeFeedbackSentimentAsync(string feedbackText);

        /// <summary>
        /// Batch analyze multiple texts
        /// </summary>
        Task<List<SentimentAnalysisResult>> BatchAnalyzeSentimentAsync(List<string> texts);

        /// <summary>
        /// Get sentiment trends for a guest
        /// </summary>
        Task<Dictionary<string, object>> GetGuestSentimentTrendsAsync(int guestId, DateTime? startDate = null, DateTime? endDate = null);
    }

    /// <summary>
    /// Sentiment analysis result
    /// </summary>
    public class SentimentAnalysisResult
    {
        /// <summary>
        /// Sentiment score (-1.0 to 1.0)
        /// -1.0 = Very Negative
        /// 0.0 = Neutral
        /// 1.0 = Very Positive
        /// </summary>
        public double SentimentScore { get; set; }

        /// <summary>
        /// Sentiment label (Positive, Neutral, Negative)
        /// </summary>
        public string SentimentLabel { get; set; } = "Neutral";

        /// <summary>
        /// Confidence score (0.0 to 1.0)
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// Detected language
        /// </summary>
        public string? Language { get; set; }

        /// <summary>
        /// Key phrases extracted
        /// </summary>
        public List<string> KeyPhrases { get; set; } = new List<string>();

        /// <summary>
        /// Detected emotions (if available)
        /// </summary>
        public Dictionary<string, double> Emotions { get; set; } = new Dictionary<string, double>();
    }
}
