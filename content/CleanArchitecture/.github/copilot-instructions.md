# GitHub Copilot Instructions

This project is a **Clean Architecture .NET 10 solution** built with the Cubido template. When assisting with code generation or modifications, follow these guidelines:

## General Principles

1. **Respect the Clean Architecture**: Always maintain proper dependency flow (Domain → Application → Infrastructure → Web)
2. **Follow CQRS pattern**: Use MediatR commands for writes and queries for reads
3. **Use existing patterns**: Follow established patterns in the codebase
4. **Write tests**: Include unit or functional tests for new features
5. **Apply SOLID principles**: Keep code maintainable and extensible

## Project Structure

```
Domain/          → Core business logic, no dependencies
Application/     → Use cases, depends only on Domain
Infrastructure/  → Implementations, depends on Domain + Application
Web/            → API endpoints, depends on Application + Infrastructure
Frontend/       → Angular SPA (or React alternative)
```

## Technology Stack

- **ASP.NET Core 10** - Web framework
- **MediatR** - CQRS implementation
- **Mapperly** - Compile-time mapping
- **Entity Framework Core** - Data access
- **Sqids** - ID obfuscation
- **NUnit, NSubstitute, Shouldly** - Testing

## Code Generation Guidelines

### When adding a new feature:

1. Start with the Domain entity (if needed)
2. Create Application command/query with handler
3. Add DTOs and Mapperly mappers
4. Implement Infrastructure if external services needed
5. Create Web endpoint
6. Write tests

### File Organization:

- Commands: `Application/[Feature]/Commands/[CommandName]/`
- Queries: `Application/[Feature]/Queries/[QueryName]/`
- Handlers: Same folder as command/query
- Endpoints: `Web/Endpoints/[Feature].cs`
- Tests: Mirror the structure in test projects

## Coding Standards

### Naming Conventions:
- Commands: `[Verb][Entity]Command` (e.g., `CreateTodoItemCommand`)
- Queries: `Get[Entity][Details]Query` (e.g., `GetTodoItemsQuery`)
- Handlers: `[CommandOrQueryName]Handler`
- DTOs: `[Entity]Dto`
- Mappers: `[Entity]Mapper`

### Record Types:
Use records for commands, queries, and DTOs:
```csharp
public record CreateTodoItemCommand(string Title, int ListId) : IRequest<int>;
```

### Dependency Injection:
Always inject interfaces, never concrete types in Application layer:
```csharp
public class MyHandler : IRequestHandler<MyCommand>
{
    private readonly IApplicationDbContext _context;
    // ✅ Good: Interface
}
```

### Guard Clauses:
Use Ardalis.GuardClauses for validation:
```csharp
Guard.Against.NotFound(id, entity);
Guard.Against.NullOrEmpty(title);
```

## Common Patterns to Follow

### MediatR Command Pattern:
```csharp
// Command
public record CreateItemCommand : IRequest<int>
{
    public string Title { get; init; }
}

// Handler
public class CreateItemCommandHandler : IRequestHandler<CreateItemCommand, int>
{
    private readonly IApplicationDbContext _context;

    public async Task<int> Handle(CreateItemCommand request, CancellationToken cancellationToken)
    {
        var entity = new Item { Title = request.Title };
        _context.Items.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
```

### MediatR Query Pattern:
```csharp
// Query
public record GetItemQuery(int Id) : IRequest<ItemDto>;

// Handler
public class GetItemQueryHandler : IRequestHandler<GetItemQuery, ItemDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper<Item, ItemDto> _mapper;

    public async Task<ItemDto> Handle(GetItemQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.Items.FindAsync(request.Id);
        Guard.Against.NotFound(request.Id, entity);
        return _mapper.Map(entity);
    }
}
```

### Mapperly Mapper:
```csharp
[Mapper]
public partial class ItemMapper
{
    public partial ItemDto Map(Item entity);
}
```

### Minimal API Endpoint:
```csharp
public class Items : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetItems)
            .MapPost(CreateItem);
    }

    public async Task<List<ItemDto>> GetItems(ISender sender)
    {
        return await sender.Send(new GetItemsQuery());
    }

    public async Task<int> CreateItem(ISender sender, CreateItemCommand command)
    {
        return await sender.Send(command);
    }
}
```

### Domain Events:
```csharp
// Raise in entity
public class Item : BaseAuditableEntity
{
    private bool _completed;
    public bool Completed
    {
        get => _completed;
        set
        {
            if (value && !_completed)
            {
                AddDomainEvent(new ItemCompletedEvent(this));
            }
            _completed = value;
        }
    }
}

// Handle in Application
public class ItemCompletedEventHandler : INotificationHandler<ItemCompletedEvent>
{
    public async Task Handle(ItemCompletedEvent notification, CancellationToken cancellationToken)
    {
        // Handle the event
    }
}
```

## What NOT to Do

❌ Don't put business logic in endpoints/controllers
❌ Don't reference Infrastructure from Domain or Application
❌ Don't bypass MediatR to call handlers directly
❌ Don't use AutoMapper or reflection-based mappers (use Mapperly)
❌ Don't create anemic domain models
❌ Don't forget to write tests
❌ Don't violate the dependency rule

## Testing Patterns

### Functional Tests:
```csharp
[TestFixture]
public class CreateItemTests : BaseTestFixture
{
    [Test]
    public async Task ShouldCreateItem()
    {
        var command = new CreateItemCommand { Title = "Test" };
        var id = await SendAsync(command);
        var item = await FindAsync<Item>(id);
        
        item.Should().NotBeNull();
        item!.Title.Should().Be("Test");
    }
}
```

### Unit Tests with Mocks:
```csharp
[Test]
public void ShouldValidateCommand()
{
    var validator = new CreateItemCommandValidator();
    var command = new CreateItemCommand { Title = "" };
    
    var result = validator.Validate(command);
    
    result.IsValid.Should().BeFalse();
    result.Errors.Should().ContainSingle(e => e.PropertyName == "Title");
}
```

## Additional Resources

For detailed guidance, refer to:
- `Agent.md` - Comprehensive Clean Architecture guide
- `.github/instructions/clean-architecture.md` - Architecture-specific rules
- `.github/instructions/typescript.md` - Frontend TypeScript guidelines
- `.github/instructions/angular.md` - Angular-specific patterns
- `.github/instructions/react.md` - React-specific patterns (if using React)

## Questions to Ask Before Generating Code

1. Which layer does this code belong to?
2. Does this follow the CQRS pattern?
3. Are dependencies pointing inward?
4. Is this testable?
5. Does this use the right abstractions?
6. Am I following existing patterns in the codebase?

When in doubt, check `Agent.md` for detailed examples and patterns.
