using GuestFlow.Domain.Events;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Intelligence.Sentiment.Handlers
{
    public class SentimentAnalysisHandler : IDomainEventHandler<GuestReviewAddedEvent>
    {
        private readonly ISentimentAnalysisService _sentimentService;
        private readonly ILogger<SentimentAnalysisHandler> _logger;

        public SentimentAnalysisHandler(ISentimentAnalysisService sentimentService, ILogger<SentimentAnalysisHandler> logger)
        {
            _sentimentService = sentimentService;
            _logger = logger;
        }

        public async Task HandleAsync(GuestReviewAddedEvent domainEvent)
        {
            _logger.LogInformation("Automatic Sentiment Analysis started for Review ID: {ReviewId}", domainEvent.Review.Id);

            if (string.IsNullOrWhiteSpace(domainEvent.Review.Comment))
            {
                _logger.LogWarning("Review comment is empty for Review ID: {ReviewId}. Skipping sentiment analysis.", domainEvent.Review.Id);
                return;
            }

            var result = await _sentimentService.AnalyzeSentimentAsync(domainEvent.Review.Comment);

            _logger.LogInformation("Sentiment Analysis completed for Review ID: {ReviewId}. Result: {Label} ({Score})", 
                domainEvent.Review.Id, result.SentimentLabel, result.SentimentScore);
            
            // In a real scenario, we might update the review entity or create a behavioral entry.
        }
    }
}
