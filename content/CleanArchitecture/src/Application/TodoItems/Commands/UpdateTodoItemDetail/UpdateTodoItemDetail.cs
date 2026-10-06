using Cubido.Template.Domain.Entities;
using Cubido.Template.Domain.Enums;

namespace Cubido.Template.Application.TodoItems.Commands.UpdateTodoItemDetail;

public record UpdateTodoItemDetailCommand : ICommand
{
    [JsonSqid<TodoItem>]
    public required int Id { get; set; }

    public int ListId { get; init; }

    public PriorityLevel Priority { get; init; }

    public string? Note { get; init; }
}

public class UpdateTodoItemDetailCommandHandler(IApplicationDbContext context) : ICommandHandler<UpdateTodoItemDetailCommand>
{
    public async ValueTask<Unit> Handle(UpdateTodoItemDetailCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.TodoItems
            .FindAsync([request.Id], cancellationToken);

        NotFoundException.ThrowIfNull(entity, request.Id);

        entity.ListId = request.ListId;
        entity.Priority = request.Priority;
        entity.Note = request.Note;

        await context.SaveChangesAsync(cancellationToken);
        return default;
    }
}
