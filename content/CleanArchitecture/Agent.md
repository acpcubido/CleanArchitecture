# Clean Architecture Guide

## Overview

This document provides a comprehensive guide on how Clean Architecture works in this solution and how to use it effectively when developing features.

## Clean Architecture Principles

### The Dependency Rule

**The overriding rule: Source code dependencies must point only inward, toward higher-level policies.**

```
┌─────────────────────────────────────┐
│         Presentation (Web)          │  ← Framework & Drivers
│   (Controllers, Endpoints, UI)      │
├─────────────────────────────────────┤
│       Infrastructure                │  ← Interface Adapters
│  (Data, Identity, External APIs)    │
├─────────────────────────────────────┤
│         Application                 │  ← Use Cases
│  (Commands, Queries, Handlers)      │
├─────────────────────────────────────┤
│           Domain                    │  ← Entities
│  (Entities, Value Objects, Events)  │
└─────────────────────────────────────┘
```

### Key Benefits

1. **Independent of Frameworks**: Business rules don't depend on the existence of any library or framework
2. **Testable**: Business rules can be tested without UI, database, web server, or any external element
3. **Independent of UI**: UI can change easily without changing the rest of the system
4. **Independent of Database**: Business rules are not bound to the database
5. **Independent of External Services**: Business rules don't know anything about interfaces to the outside world

## Layer-by-Layer Guide

### 1. Domain Layer (`src/Domain/`)

**Purpose**: Contains the enterprise business logic and domain model.

**What belongs here:**
- **Entities**: Business objects with identity (`TodoItem`, `TodoList`)
- **Value Objects**: Immutable objects without identity (`Colour`)
- **Domain Events**: Events that represent something that happened in the domain
- **Enums**: Domain-specific enumerations
- **Exceptions**: Domain-specific exceptions
- **Specifications**: Business rule specifications

**Rules:**
- ✅ No dependencies on other projects
- ✅ Pure C# with no infrastructure concerns
- ✅ Contains business rules and invariants
- ❌ No dependency injection
- ❌ No database concerns
- ❌ No external service calls

**Example Entity:**
```csharp
public class TodoItem : BaseAuditableEntity
{
    public int ListId { get; set; }
    public string? Title { get; set; }
    public string? Note { get; set; }
    public PriorityLevel Priority { get; set; }
    public DateTime? Reminder { get; set; }
    private bool _done;
    
    public bool Done
    {
        get => _done;
        set
        {
            if (value && !_done)
            {
                // Raise domain event when item is completed
                AddDomainEvent(new TodoItemCompletedEvent(this));
            }
            _done = value;
        }
    }
}
```

### 2. Application Layer (`src/Application/`)

**Purpose**: Contains application business rules and orchestrates the domain model.

**What belongs here:**
- **Commands**: Operations that change state (CQRS Write side)
- **Queries**: Operations that read state (CQRS Read side)
- **Handlers**: Process commands and queries using MediatR
- **Interfaces**: Abstractions for infrastructure services
- **DTOs/Models**: Data transfer objects
- **Validators**: FluentValidation rules
- **Behaviours**: Cross-cutting concerns (validation, logging, transactions)
- **Mappings**: Mapperly mappers

**Rules:**
- ✅ Depends only on Domain layer
- ✅ Defines interfaces for infrastructure
- ✅ Uses MediatR for CQRS pattern
- ✅ Contains use case logic
- ❌ No direct infrastructure implementations
- ❌ No framework-specific code

**Example Command:**
```csharp
public record CreateTodoItemCommand : IRequest<int>
{
    public int ListId { get; init; }
    public string? Title { get; init; }
}

public class CreateTodoItemCommandHandler : IRequestHandler<CreateTodoItemCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateTodoItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateTodoItemCommand request, CancellationToken cancellationToken)
    {
        var entity = new TodoItem
        {
            ListId = request.ListId,
            Title = request.Title,
            Done = false
        };

        _context.TodoItems.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
```

**Example Query:**
```csharp
public record GetTodoItemsQuery : IRequest<List<TodoItemDto>>
{
    public int ListId { get; init; }
}

public class GetTodoItemsQueryHandler : IRequestHandler<GetTodoItemsQuery, List<TodoItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper<TodoItem, TodoItemDto> _mapper;

    public GetTodoItemsQueryHandler(
        IApplicationDbContext context,
        IMapper<TodoItem, TodoItemDto> mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<TodoItemDto>> Handle(
        GetTodoItemsQuery request,
        CancellationToken cancellationToken)
    {
        var items = await _context.TodoItems
            .Where(x => x.ListId == request.ListId)
            .ToListAsync(cancellationToken);

        return items.Select(_mapper.Map).ToList();
    }
}
```

### 3. Infrastructure Layer (`src/Infrastructure/`)

**Purpose**: Implements interfaces defined in the Application layer with concrete implementations.

**What belongs here:**
- **DbContext**: Entity Framework Core database context
- **Configurations**: Entity configurations
- **Migrations**: Database migrations
- **Identity**: Authentication and authorization
- **Services**: External service implementations
- **Repositories**: If using repository pattern

**Rules:**
- ✅ Implements Application interfaces
- ✅ Depends on Domain and Application
- ✅ Contains all infrastructure concerns
- ✅ Database, file system, external APIs
- ❌ No business logic
- ❌ No direct use in Domain or Application

**Example Implementation:**
```csharp
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<TodoList> TodoLists => Set<TodoList>();
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
```

### 4. Web/Presentation Layer (`src/Web/`)

**Purpose**: Entry point and API exposure using ASP.NET Core Minimal APIs.

**What belongs here:**
- **Endpoints**: API endpoint definitions
- **Filters**: Request/response filters
- **Middleware**: Custom middleware
- **Configuration**: Startup configuration
- **DI Registration**: Service registration

**Rules:**
- ✅ Depends on Application and Infrastructure
- ✅ Configures dependency injection
- ✅ Defines HTTP contracts
- ❌ No business logic
- ❌ Minimal code - delegate to Application

**Example Endpoint:**
```csharp
public class TodoItems : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetTodoItems)
            .MapPost(CreateTodoItem)
            .MapPut(UpdateTodoItem, "{id}")
            .MapDelete(DeleteTodoItem, "{id}");
    }

    public async Task<List<TodoItemDto>> GetTodoItems(
        ISender sender,
        [AsParameters] GetTodoItemsQuery query)
    {
        return await sender.Send(query);
    }

    public async Task<int> CreateTodoItem(
        ISender sender,
        CreateTodoItemCommand command)
    {
        return await sender.Send(command);
    }
}
```

## Technology Stack Usage

### MediatR (CQRS Pattern)

**Commands** - Change state:
```csharp
// Define command
public record DeleteTodoItemCommand(int Id) : IRequest;

// Handle command
public class DeleteTodoItemCommandHandler : IRequestHandler<DeleteTodoItemCommand>
{
    private readonly IApplicationDbContext _context;

    public async Task Handle(DeleteTodoItemCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.TodoItems.FindAsync(request.Id);
        Guard.Against.NotFound(request.Id, entity);
        
        _context.TodoItems.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
```

**Queries** - Read state:
```csharp
// Define query
public record GetTodoItemQuery(int Id) : IRequest<TodoItemDto>;

// Handle query
public class GetTodoItemQueryHandler : IRequestHandler<GetTodoItemQuery, TodoItemDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper<TodoItem, TodoItemDto> _mapper;

    public async Task<TodoItemDto> Handle(GetTodoItemQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.TodoItems.FindAsync(request.Id);
        Guard.Against.NotFound(request.Id, entity);
        
        return _mapper.Map(entity);
    }
}
```

### Mapperly (Object Mapping)

**Define mapper interface:**
```csharp
[Mapper]
public partial class TodoItemMapper
{
    public partial TodoItemDto Map(TodoItem entity);
    public partial void Map(UpdateTodoItemCommand command, TodoItem entity);
}
```

Mapperly generates the implementation at compile-time for better performance than reflection-based mappers.

### Entity Framework Core

**Always use through IApplicationDbContext interface:**
```csharp
public interface IApplicationDbContext
{
    DbSet<TodoList> TodoLists { get; }
    DbSet<TodoItem> TodoItems { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
```

**Configure entities:**
```csharp
public class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
{
    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        builder.Property(t => t.Title)
            .HasMaxLength(200)
            .IsRequired();
    }
}
```

### Sqids & Sqiddler (ID Obfuscation)

Used to convert integer IDs to short, URL-safe strings:

```csharp
// Configure in DependencyInjection
services.AddSqids(options => 
{
    options.Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    options.MinLength = 8;
});

// Usage in endpoints
public async Task<TodoItemDto> GetTodoItem(
    ISqidEncoder sqids,
    string id)
{
    var numericId = sqids.Decode(id).Single();
    // Use numericId...
}
```

### Testing Stack

**NUnit** - Test framework:
```csharp
[TestFixture]
public class CreateTodoItemCommandTests : BaseTestFixture
{
    [Test]
    public async Task ShouldCreateTodoItem()
    {
        // Arrange
        var command = new CreateTodoItemCommand { Title = "Test Item" };

        // Act
        var itemId = await SendAsync(command);

        // Assert
        var item = await FindAsync<TodoItem>(itemId);
        item.Should().NotBeNull();
        item!.Title.Should().Be("Test Item");
    }
}
```

**NSubstitute** - Mocking:
```csharp
var mockContext = Substitute.For<IApplicationDbContext>();
mockContext.SaveChangesAsync(Arg.Any<CancellationToken>())
    .Returns(1);
```

**Shouldly** - Fluent assertions:
```csharp
result.ShouldNotBeNull();
result.Title.ShouldBe("Expected Title");
result.Items.ShouldNotBeEmpty();
```

## Development Workflow

### Adding a New Feature

1. **Define the domain entity** (if needed) in `Domain/Entities/`
2. **Create command/query** in `Application/[Feature]/Commands/` or `Queries/`
3. **Add handler** in the same folder
4. **Define DTO** in `Application/Common/Models/`
5. **Create mapper** using Mapperly
6. **Add endpoint** in `Web/Endpoints/`
7. **Write tests** in `Application.FunctionalTests/` or `Application.UnitTests/`

### Example: Adding a "Complete All Items" Feature

**Step 1: Create Command**
```csharp
// Application/TodoItems/Commands/CompleteAllTodoItems/CompleteAllTodoItemsCommand.cs
public record CompleteAllTodoItemsCommand(int ListId) : IRequest;
```

**Step 2: Create Handler**
```csharp
// Application/TodoItems/Commands/CompleteAllTodoItems/CompleteAllTodoItemsCommandHandler.cs
public class CompleteAllTodoItemsCommandHandler : IRequestHandler<CompleteAllTodoItemsCommand>
{
    private readonly IApplicationDbContext _context;

    public CompleteAllTodoItemsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(CompleteAllTodoItemsCommand request, CancellationToken cancellationToken)
    {
        var items = await _context.TodoItems
            .Where(x => x.ListId == request.ListId && !x.Done)
            .ToListAsync(cancellationToken);

        foreach (var item in items)
        {
            item.Done = true; // This will trigger domain events
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
```

**Step 3: Add Endpoint**
```csharp
// Web/Endpoints/TodoItems.cs
public async Task<IResult> CompleteAllTodoItems(
    ISender sender,
    int listId)
{
    await sender.Send(new CompleteAllTodoItemsCommand(listId));
    return Results.NoContent();
}
```

**Step 4: Add Test**
```csharp
// Application.FunctionalTests/TodoItems/Commands/CompleteAllTodoItemsTests.cs
[Test]
public async Task ShouldCompleteAllItems()
{
    // Arrange
    var listId = await SendAsync(new CreateTodoListCommand { Title = "My List" });
    await SendAsync(new CreateTodoItemCommand { ListId = listId, Title = "Item 1" });
    await SendAsync(new CreateTodoItemCommand { ListId = listId, Title = "Item 2" });

    // Act
    await SendAsync(new CompleteAllTodoItemsCommand(listId));

    // Assert
    var items = await FindAsync<TodoItem>(x => x.ListId == listId);
    items.Should().AllSatisfy(x => x.Done.Should().BeTrue());
}
```

## Best Practices

### ✅ DO

- Keep domain entities focused on business rules
- Use value objects for concepts without identity
- Raise domain events for important state changes
- Use MediatR for all use cases (commands/queries)
- Define interfaces in Application, implement in Infrastructure
- Use Mapperly for compile-time mapping
- Write tests for all business logic
- Use Guard clauses for validation
- Keep controllers/endpoints thin

### ❌ DON'T

- Put business logic in controllers/endpoints
- Reference Infrastructure from Application
- Use `new` for creating dependencies
- Bypass MediatR to call handlers directly
- Put database concerns in Domain or Application
- Create anemic domain models (just getters/setters)
- Skip tests
- Use runtime reflection-based mappers

## Common Patterns

### Validation with Behaviours

```csharp
public class ValidationBehaviour<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            
            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
                throw new ValidationException(failures);
        }

        return await next();
    }
}
```

### Transaction Management

```csharp
public class TransactionBehaviour<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IApplicationDbContext _context;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = default(TResponse);
        
        try
        {
            await _context.BeginTransactionAsync();
            response = await next();
            await _context.CommitTransactionAsync();
        }
        catch
        {
            await _context.RollbackTransactionAsync();
            throw;
        }

        return response!;
    }
}
```

### Domain Event Handling

```csharp
// Raise event in entity
public class TodoItem : BaseAuditableEntity
{
    public bool Done
    {
        set
        {
            if (value && !_done)
            {
                AddDomainEvent(new TodoItemCompletedEvent(this));
            }
            _done = value;
        }
    }
}

// Handle event
public class TodoItemCompletedEventHandler 
    : INotificationHandler<TodoItemCompletedEvent>
{
    private readonly ILogger<TodoItemCompletedEventHandler> _logger;

    public async Task Handle(
        TodoItemCompletedEvent notification,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Todo item {Id} completed", 
            notification.Item.Id);
    }
}
```

## Architecture Decision Records

### Why CQRS?

- Separates read and write concerns
- Optimizes queries independently
- Simplifies complex business logic
- Better scalability

### Why Mapperly over AutoMapper?

- Compile-time code generation
- No runtime reflection overhead
- Better performance
- Easier debugging

### Why Minimal APIs?

- Less ceremony than controllers
- Better performance
- More explicit routing
- Easier to organize by feature

## Resources

- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [Mapperly Documentation](https://github.com/riok/mapperly)
- [Entity Framework Core Documentation](https://docs.microsoft.com/ef/core/)
