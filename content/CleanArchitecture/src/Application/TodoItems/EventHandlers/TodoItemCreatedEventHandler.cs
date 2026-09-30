using Cubido.Template.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Cubido.Template.Application.TodoItems.EventHandlers;

public class TodoItemCreatedEventHandler(ILogger<TodoItemCreatedEventHandler> logger) : INotificationHandler<TodoItemCreatedEvent>
{
    public ValueTask Handle(TodoItemCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Cubido.Template Domain Event: {DomainEvent}", notification.GetType().Name);

        return default;
    }
}
