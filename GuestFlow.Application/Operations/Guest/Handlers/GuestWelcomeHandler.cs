using GuestFlow.Domain.Events;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Guest.Handlers
{
    public class GuestWelcomeHandler : IDomainEventHandler<GuestCreatedEvent>
    {
        private readonly ILogger<GuestWelcomeHandler> _logger;

        public GuestWelcomeHandler(ILogger<GuestWelcomeHandler> logger)
        {
            _logger = logger;
        }

        public Task HandleAsync(GuestCreatedEvent domainEvent)
        {
            _logger.LogInformation("Sending welcome notification to guest: {FullName} ({Email})", 
                domainEvent.Guest.FullName, domainEvent.Guest.Email);

            // Logic to send welcome email or system notification would go here
            // e.g., _emailService.SendWelcomeEmailAsync(domainEvent.Guest.Email);

            return Task.CompletedTask;
        }
    }
}
