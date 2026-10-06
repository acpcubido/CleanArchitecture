using Cubido.Template.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Cubido.Template.Application.TodoItems.EventHandlers;

public class TodoItemCompletedEventHandler(ILogger<TodoItemCompletedEventHandler> logger) : INotificationHandler<TodoItemCompletedEvent>
{
    public ValueTask Handle(TodoItemCompletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Cubido.Template Domain Event: {DomainEvent}", notification.GetType().Name);
        return ValueTask.CompletedTask;
    }
}
