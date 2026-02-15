using GuestFlow.Domain.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Application.Infrastructure.Events
{
    public class DomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DomainEventDispatcher> _logger;

        public DomainEventDispatcher(IServiceProvider serviceProvider, ILogger<DomainEventDispatcher> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task DispatchEventsAsync(IEnumerable<IDomainEvent> events)
        {
            foreach (var domainEvent in events)
            {
                await DispatchEventAsync(domainEvent);
            }
        }

        private async Task DispatchEventAsync(IDomainEvent domainEvent)
        {
            var eventType = domainEvent.GetType();
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
            var handlers = _serviceProvider.GetServices(handlerType);

            foreach (var handler in handlers)
            {
                if (handler == null) continue;

                try
                {
                    _logger.LogInformation("Handling domain event: {EventName} with {HandlerName}", eventType.Name, handler.GetType().Name);
                    var method = handlerType.GetMethod("HandleAsync");
                    if (method != null)
                    {
                        var task = (Task)method.Invoke(handler, new object[] { domainEvent })!;
                        await task;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling domain event: {EventName}", eventType.Name);
                    // Decide if we should rethrow or continue. Usually continue for other handlers.
                }
            }
        }
    }
}
