---
description: Clean Architecture implementation rules for .NET backend
applyTo: 'src/**/*.cs'
---

# Clean Architecture Instructions

## Purpose

This document provides specific rules and patterns for implementing Clean Architecture in this .NET 10 solution using the Cubido template.

## Architecture Overview

```
┌─────────────────────────────────────────────┐
│              Web Layer                      │  ← ASP.NET Core 10
│  - Endpoints (Minimal APIs)                 │  ← Presentation
│  - Filters & Middleware                     │
├─────────────────────────────────────────────┤
│         Infrastructure Layer                │  ← Entity Framework Core
│  - DbContext & Configurations               │  ← Identity
│  - Identity & Authentication                │  ← External Services
│  - External Services                        │
├─────────────────────────────────────────────┤
│         Application Layer                   │  ← MediatR (CQRS)
│  - Commands & Queries                       │  ← Mapperly
│  - Handlers & Validators                    │  ← FluentValidation
│  - DTOs & Interfaces                        │  ← Behaviours
├─────────────────────────────────────────────┤
│            Domain Layer                     │  ← Pure C#
│  - Entities & Value Objects                 │  ← Business Rules
│  - Domain Events                            │  ← No Dependencies
│  - Specifications                           │
└─────────────────────────────────────────────┘
```

## The Dependency Rule

**Critical**: Dependencies must flow inward only.

✅ **Allowed Dependencies:**
- Domain: None
- Application: → Domain
- Infrastructure: → Domain, Application
- Web: → Application, Infrastructure (for DI registration only)

❌ **Forbidden Dependencies:**
- Domain → Application, Infrastructure, or Web
- Application → Infrastructure or Web
- Infrastructure → Web

## Layer-Specific Rules

### Domain Layer (`src/Domain/`)

**Contains:**
- Entities (with identity)
- Value Objects (without identity)
- Domain Events
- Enums
- Exceptions
- Domain-specific interfaces

**Rules:**

✅ **DO:**
```csharp
// ✅ Rich domain models with behavior
public class TodoItem : BaseAuditableEntity
{
    private bool _done;
    
    public string? Title { get; set; }
    public int ListId { get; set; }
    
    public bool Done
    {
        get => _done;
        set
        {
            if (value && !_done)
            {
                AddDomainEvent(new TodoItemCompletedEvent(this));
            }
            _done = value;
        }
    }

    public void MarkAsComplete()
    {
        Done = true;
    }
}

// ✅ Value objects with validation
public class Colour : ValueObject
{
    public string Code { get; private set; } = null!;

    public Colour(string code)
    {
        Code = Guard.Against.NullOrEmpty(code, nameof(code));
        ValidateColourCode(code);
    }

    private static void ValidateColourCode(string code)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(code, "^#(?:[0-9a-fA-F]{3}){1,2}$"))
        {
            throw new UnsupportedColourException(code);
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
    }
}

// ✅ Domain events
public class TodoItemCompletedEvent : BaseEvent
{
    public TodoItemCompletedEvent(TodoItem item)
    {
        Item = item;
    }

    public TodoItem Item { get; }
}
```

❌ **DON'T:**
```csharp
// ❌ No infrastructure concerns
public class TodoItem : BaseEntity
{
    public DbSet<TodoList> Lists { get; set; } // NO!
    
    public async Task SaveToDatabase(DbContext context) // NO!
    {
        await context.SaveChangesAsync();
    }
}

// ❌ No application concerns
public class TodoItem : BaseEntity
{
    public TodoItemDto ToDto() // NO! DTOs belong in Application
    {
        return new TodoItemDto();
    }
}

// ❌ No framework dependencies
[Required] // NO! Data annotations belong in Application
public string Title { get; set; }
```

### Application Layer (`src/Application/`)

**Contains:**
- Commands (write operations)
- Queries (read operations)
- Handlers
- DTOs
- Mapperly Mappers
- FluentValidation Validators
- Interface definitions
- Behaviours

**Rules:**

#### Commands (CQRS Write Side)

```csharp
// ✅ Command definition with IRequest
public record CreateTodoItemCommand : IRequest<int>
{
    public int ListId { get; init; }
    public string? Title { get; init; }
    public PriorityLevel Priority { get; init; } = PriorityLevel.None;
}

// ✅ Command validator
public class CreateTodoItemCommandValidator : AbstractValidator<CreateTodoItemCommand>
{
    public CreateTodoItemCommandValidator()
    {
        RuleFor(v => v.Title)
            .MaximumLength(200)
            .NotEmpty();

        RuleFor(v => v.ListId)
            .NotEmpty()
            .GreaterThan(0);
    }
}

// ✅ Command handler
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
            Priority = request.Priority,
            Done = false
        };

        _context.TodoItems.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
```

#### Queries (CQRS Read Side)

```csharp
// ✅ Query definition
public record GetTodoItemsQuery : IRequest<List<TodoItemDto>>
{
    public int ListId { get; init; }
}

// ✅ Query handler with mapping
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

    public async Task<List<TodoItemDto>> Handle(GetTodoItemsQuery request, CancellationToken cancellationToken)
    {
        var items = await _context.TodoItems
            .Where(x => x.ListId == request.ListId)
            .OrderBy(x => x.Title)
            .ToListAsync(cancellationToken);

        return items.Select(_mapper.Map).ToList();
    }
}
```

#### DTOs and Mapping

```csharp
// ✅ DTO definition
public class TodoItemDto
{
    public string Id { get; init; } = null!;
    public int ListId { get; init; }
    public string? Title { get; init; }
    public bool Done { get; init; }
    public PriorityLevel Priority { get; init; }
}

// ✅ Mapperly mapper
[Mapper]
public partial class TodoItemMapper
{
    // Inject Sqids for ID obfuscation
    private readonly ISqidEncoder _sqids;

    public TodoItemMapper(ISqidEncoder sqids)
    {
        _sqids = sqids;
    }

    // Map entity to DTO
    public partial TodoItemDto Map(TodoItem entity);

    // Custom mapping for ID
    private string MapId(int id) => _sqids.Encode(id);

    // Map command to entity for updates
    public partial void Map(UpdateTodoItemCommand command, TodoItem entity);
}
```

#### Interfaces

```csharp
// ✅ Define interfaces for infrastructure
public interface IApplicationDbContext
{
    DbSet<TodoList> TodoLists { get; }
    DbSet<TodoItem> TodoItems { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

public interface IEmailService
{
    Task SendAsync(string to, string subject, string body);
}

public interface IDateTime
{
    DateTime Now { get; }
}
```

#### Behaviours (Cross-Cutting Concerns)

```csharp
// ✅ Validation behaviour
public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
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

// ✅ Unhandled exception behaviour
public class UnhandledExceptionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<TRequest> _logger;

    public UnhandledExceptionBehaviour(ILogger<TRequest> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;
            _logger.LogError(ex, "Request: Unhandled Exception for Request {Name} {@Request}", requestName, request);
            throw;
        }
    }
}
```

❌ **DON'T:**
```csharp
// ❌ Don't reference Infrastructure implementations
public class MyHandler : IRequestHandler<MyCommand>
{
    private readonly ApplicationDbContext _context; // NO! Use IApplicationDbContext
}

// ❌ Don't put business logic in handlers
public class CreateTodoItemHandler : IRequestHandler<CreateTodoItemCommand, int>
{
    public async Task<int> Handle(CreateTodoItemCommand request, CancellationToken ct)
    {
        // Complex business logic here... // NO! Put in Domain
    }
}

// ❌ Don't bypass MediatR
public class TodoController : ControllerBase
{
    private readonly CreateTodoItemHandler _handler; // NO! Use ISender
}
```

### Infrastructure Layer (`src/Infrastructure/`)

**Contains:**
- DbContext implementation
- Entity configurations
- Identity implementation
- External service implementations
- Migrations

**Rules:**

#### DbContext

```csharp
// ✅ Implement IApplicationDbContext
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly IDateTime _dateTime;
    private readonly IDomainEventService _domainEventService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IDateTime dateTime,
        IDomainEventService domainEventService)
        : base(options)
    {
        _dateTime = dateTime;
        _domainEventService = domainEventService;
    }

    public DbSet<TodoList> TodoLists => Set<TodoList>();
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Set audit fields
        foreach (var entry in ChangeTracker.Entries<BaseAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.Created = _dateTime.Now;
                    entry.Entity.CreatedBy = "system";
                    break;
                case EntityState.Modified:
                    entry.Entity.LastModified = _dateTime.Now;
                    entry.Entity.LastModifiedBy = "system";
                    break;
            }
        }

        var result = await base.SaveChangesAsync(cancellationToken);

        // Dispatch domain events
        await _domainEventService.DispatchEventsAsync(this, cancellationToken);

        return result;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }
}
```

#### Entity Configurations

```csharp
// ✅ Use IEntityTypeConfiguration
public class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
{
    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        builder.Property(t => t.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Priority)
            .HasConversion<int>()
            .IsRequired();

        builder.HasOne<TodoList>()
            .WithMany()
            .HasForeignKey(t => t.ListId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

#### Service Implementations

```csharp
// ✅ Implement application interfaces
public class DateTimeService : IDateTime
{
    public DateTime Now => DateTime.UtcNow;
}

public class EmailService : IEmailService
{
    private readonly IOptions<EmailSettings> _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string body)
    {
        // Implementation using SendGrid, SMTP, etc.
        _logger.LogInformation("Sending email to {To}", to);
        await Task.CompletedTask;
    }
}
```

#### Dependency Injection Registration

```csharp
// ✅ Register services in DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(provider => 
            provider.GetRequiredService<ApplicationDbContext>());

        // Services
        services.AddTransient<IDateTime, DateTimeService>();
        services.AddTransient<IEmailService, EmailService>();
        services.AddTransient<IDomainEventService, DomainEventService>();

        // Identity
        services.AddDefaultIdentity<ApplicationUser>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        return services;
    }
}
```

### Web Layer (`src/Web/`)

**Contains:**
- Minimal API endpoints
- Filters
- Middleware
- Configuration
- DI registration

**Rules:**

#### Minimal API Endpoints

```csharp
// ✅ Endpoint group base
public abstract class EndpointGroupBase
{
    public abstract void Map(WebApplication app);
}

// ✅ Feature endpoints
public class TodoItems : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this)
            .RequireAuthorization()
            .WithOpenApi();

        group.MapGet(GetTodoItems, "lists/{listId}/items")
            .WithName("GetTodoItems")
            .Produces<List<TodoItemDto>>();

        group.MapPost(CreateTodoItem, "items")
            .WithName("CreateTodoItem")
            .Produces<int>(StatusCodes.Status201Created);

        group.MapPut(UpdateTodoItem, "items/{id}")
            .WithName("UpdateTodoItem")
            .Produces(StatusCodes.Status204NoContent);

        group.MapDelete(DeleteTodoItem, "items/{id}")
            .WithName("DeleteTodoItem")
            .Produces(StatusCodes.Status204NoContent);
    }

    // ✅ Thin endpoint methods - delegate to MediatR
    public async Task<List<TodoItemDto>> GetTodoItems(
        ISender sender,
        [AsParameters] GetTodoItemsQuery query)
    {
        return await sender.Send(query);
    }

    public async Task<IResult> CreateTodoItem(
        ISender sender,
        CreateTodoItemCommand command)
    {
        var id = await sender.Send(command);
        return Results.Created($"/api/todoitems/{id}", id);
    }

    public async Task<IResult> UpdateTodoItem(
        ISender sender,
        int id,
        UpdateTodoItemCommand command)
    {
        if (id != command.Id)
        {
            return Results.BadRequest();
        }

        await sender.Send(command);
        return Results.NoContent();
    }

    public async Task<IResult> DeleteTodoItem(
        ISender sender,
        int id)
    {
        await sender.Send(new DeleteTodoItemCommand(id));
        return Results.NoContent();
    }
}
```

#### Program.cs Configuration

```csharp
// ✅ Configure services and pipeline
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddWeb();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseOpenApi();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapEndpoints();

app.Run();
```

❌ **DON'T:**
```csharp
// ❌ Don't put business logic in endpoints
public async Task<IResult> CreateTodoItem(ISender sender, CreateTodoItemCommand command)
{
    // Validation logic... // NO! Use FluentValidation
    // Business logic... // NO! Use Domain/Application
    
    var id = await sender.Send(command);
    return Results.Created($"/api/todoitems/{id}", id);
}

// ❌ Don't call handlers directly
public class TodoItemsController : ControllerBase
{
    private readonly CreateTodoItemHandler _handler; // NO! Use ISender
    
    public async Task<IActionResult> Create(CreateTodoItemCommand command)
    {
        var result = await _handler.Handle(command, CancellationToken.None); // NO!
        return Created("", result);
    }
}
```

## Technology-Specific Patterns

### MediatR

```csharp
// ✅ Command/Query definition
public record MyCommand : IRequest<int>;
public record MyQuery : IRequest<MyResult>;

// ✅ Handler
public class MyCommandHandler : IRequestHandler<MyCommand, int>
{
    public async Task<int> Handle(MyCommand request, CancellationToken cancellationToken)
    {
        // Implementation
        return 1;
    }
}

// ✅ Notification (for domain events)
public class TodoItemCompletedEventHandler : INotificationHandler<TodoItemCompletedEvent>
{
    public async Task Handle(TodoItemCompletedEvent notification, CancellationToken cancellationToken)
    {
        // Handle event
    }
}
```

### Mapperly

```csharp
// ✅ Mapper with custom mapping
[Mapper]
public partial class TodoItemMapper
{
    private readonly ISqidEncoder _sqids;

    public TodoItemMapper(ISqidEncoder sqids)
    {
        _sqids = sqids;
    }

    // Auto-generated mapping
    public partial TodoItemDto Map(TodoItem entity);

    // Custom property mapping
    private string MapId(int id) => _sqids.Encode(id);
    
    // Ignore properties
    [MapperIgnore]
    public string IgnoredProperty { get; set; }

    // Map to existing object
    public partial void Map(UpdateTodoItemCommand command, TodoItem entity);
}
```

### Entity Framework Core

```csharp
// ✅ Optimized queries
public async Task<List<TodoItemDto>> Handle(GetTodoItemsQuery request, CancellationToken ct)
{
    return await _context.TodoItems
        .Where(x => x.ListId == request.ListId)
        .OrderBy(x => x.Title)
        .ProjectTo<TodoItemDto>(_mapper.ConfigurationProvider) // If using AutoMapper
        .ToListAsync(ct);
}

// ✅ Include related entities
var lists = await _context.TodoLists
    .Include(l => l.Items)
    .ToListAsync();

// ✅ No tracking for read-only queries
var items = await _context.TodoItems
    .AsNoTracking()
    .ToListAsync();
```

### Sqids (ID Obfuscation)

```csharp
// ✅ Encode/Decode IDs
public class TodoItemMapper
{
    private readonly ISqidEncoder _sqids;

    private string MapId(int id) => _sqids.Encode(id);
}

// ✅ In endpoints
public async Task<TodoItemDto> GetTodoItem(ISqidEncoder sqids, string id)
{
    var numericId = sqids.Decode(id).Single();
    return await _sender.Send(new GetTodoItemQuery { Id = numericId });
}
```

## Testing Patterns

### Unit Tests

```csharp
[TestFixture]
public class CreateTodoItemCommandValidatorTests
{
    private CreateTodoItemCommandValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new CreateTodoItemCommandValidator();
    }

    [Test]
    public void Should_Have_Error_When_Title_Is_Empty()
    {
        var command = new CreateTodoItemCommand { Title = string.Empty };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(command.Title));
    }

    [Test]
    public void Should_Have_Error_When_Title_Exceeds_MaxLength()
    {
        var command = new CreateTodoItemCommand { Title = new string('a', 201) };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}
```

### Functional Tests

```csharp
[TestFixture]
public class CreateTodoItemTests : BaseTestFixture
{
    [Test]
    public async Task Should_Create_TodoItem()
    {
        // Arrange
        var listId = await SendAsync(new CreateTodoListCommand { Title = "Test List" });
        var command = new CreateTodoItemCommand
        {
            ListId = listId,
            Title = "Test Item",
            Priority = PriorityLevel.High
        };

        // Act
        var itemId = await SendAsync(command);

        // Assert
        var item = await FindAsync<TodoItem>(itemId);
        
        item.Should().NotBeNull();
        item!.Title.Should().Be("Test Item");
        item.Priority.Should().Be(PriorityLevel.High);
        item.Done.Should().BeFalse();
    }

    [Test]
    public async Task Should_Require_Valid_ListId()
    {
        var command = new CreateTodoItemCommand { ListId = 99999, Title = "Test" };

        await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<NotFoundException>();
    }
}
```

## Common Patterns and Solutions

### Pagination

```csharp
public record GetTodoItemsQuery : IRequest<PaginatedList<TodoItemDto>>
{
    public int ListId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public static class PaginatedList
{
    public static async Task<PaginatedList<T>> CreateAsync<T>(
        IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        
        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }
}
```

### Soft Delete

```csharp
public abstract class BaseAuditableEntity : BaseEntity
{
    public DateTime Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}

// Query filter in DbContext
protected override void OnModelCreating(ModelBuilder builder)
{
    builder.Entity<TodoItem>().HasQueryFilter(e => !e.IsDeleted);
}
```

### Specification Pattern

```csharp
public abstract class Specification<T>
{
    public abstract Expression<Func<T, bool>> ToExpression();
    
    public bool IsSatisfiedBy(T entity)
    {
        var predicate = ToExpression().Compile();
        return predicate(entity);
    }
}

public class CompletedTodoItemsSpecification : Specification<TodoItem>
{
    public override Expression<Func<TodoItem, bool>> ToExpression()
    {
        return item => item.Done;
    }
}
```

## Resources

- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [MediatR Wiki](https://github.com/jbogard/MediatR/wiki)
- [Mapperly Documentation](https://github.com/riok/mapperly)
- [FluentValidation Documentation](https://docs.fluentvalidation.net/)
