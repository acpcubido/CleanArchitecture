# Cubido Clean Architecture Template

## Architecture Overview

This solution follows our company ACP cubido **Clean Architecture** principles with clear separation of concerns across four main layers:

```
┌─────────────────────────────────────────┐
│  Web (API Layer)                        │  ← ASP.NET Core Web API,  Minimal APIs
├─────────────────────────────────────────┤
│  Application (Use Cases)                │  ← Mediator, Commands/Queries
├─────────────────────────────────────────┤
│  Infrastructure (External Concerns)     │  ← EF Core, Identity, Azure, external API clients
├─────────────────────────────────────────┤
│  Domain (Business Logic)                │  ← Entities, Value Objects, Events
└─────────────────────────────────────────┘
```

---

## Tech Stack

| Layer | Technology | Key Packages |
|---|---|---|
| **Framework** | latest .NET | ASP.NET Core |
| **API** | Minimal APIs + NSwag | Scalar UI for documentation |
| **Mediator** | Mediator.SourceGenerator | Request/response pattern |
| **Mapping** | Mapperly | Source generator mapping |
| **Database** | Entity Framework Core | SQL Server |
| **Identity** | ASP.NET Core Identity | Entity Framework Core stores |
| **Validation** | FluentValidation | Dependency injection extensions |
| **IDs** | Sqids + Sqiddler | Obfuscated integer IDs |
| **Testing** | NUnit + NSubstitute + Shouldly | Unit, Integration, Functional |
| **Frontend** | latest Angular (optional) | Standalone components, NgRx Signals |
| **Cloud** | Azure | Key Vault |

---

## Project Structure

### Source Projects (`src/`)

#### **Domain** (`src/Domain/`)
- **Purpose**: Entities, value objects, domain events
- **Dependencies**: None (no external dependencies)
- **Key Folders**:
  - `Entities/` — Domain entities (e.g., TodoItem, TodoList)
  - `ValueObjects/` — Immutable value objects
  - `Events/` — Domain events
  - `Enums/` — Domain enumerations
  - `Exceptions/` — Domain-specific exceptions
  - `Constants/` — Domain constants
  - `Common/` — Base classes and interfaces

**Rules**:
- No dependencies on other layers
- Only `Mediator.Abstractions` allowed as external package
- Pure data definition (no infrastructure concerns)

#### **Application** (`src/Application/`)
- **Purpose**: Use cases, CQRS commands/queries, application logic
- **Dependencies**: Domain only
- **Key Folders**:
  - `{FeatureName}/Commands/` — Command handlers (writes)
  - `{FeatureName}/Queries/` — Query handlers (reads)
  - `Common/` — Interfaces, behaviors, validators
- **Key Packages**:
  - `Mediator.SourceGenerator` — CQRS pattern
  - `FluentValidation` — Request validation
  - `Mapperly` — DTO mapping
  - `Sqids` + `Sqiddler.Core` — ID obfuscation

**Rules**:
- All business operations via Mediator requests
- DTOs for input/output (never expose domain entities directly)
- Validation via FluentValidation
- Mapping via Mapperly source generators

#### **Infrastructure** (`src/Infrastructure/`)
- **Purpose**: External concerns (database, identity, files, third-party APIs)
- **Dependencies**: Application, Domain
- **Key Folders**:
  - `Data/` — EF Core DbContext, configurations, migrations
  - `Identity/` — ASP.NET Core Identity implementation
- **Key Packages**:
  - `Microsoft.EntityFrameworkCore.SqlServer`
  - `Microsoft.AspNetCore.Identity.EntityFrameworkCore`

**Rules**:
- Implements Application interfaces
- EF Core entity configurations in separate files
- Use interceptors for cross-cutting concerns (auditing, domain events)

#### **Web** (`src/Web/`)
- **Purpose**: API layer, dependency injection, middleware
- **Dependencies**: Application, Infrastructure
- **Key Folders**:
  - `Endpoints/` — Minimal API endpoint definitions
  - `Infrastructure/` — Custom middleware, filters, exception handlers
  - `Services/` — Web-specific services
  - `wwwroot/api/` — Auto-generated OpenAPI specs
- **Key Packages**:
  - `NSwag.MSBuild` — OpenAPI generation
  - `Scalar.AspNetCore` — API documentation UI
  - `Sqiddler.OpenApi` — Sqids OpenAPI schema support

**Rules**:
- Endpoints organized by feature in `Endpoints/`
- OpenAPI auto-generation configured in `.csproj`
- NSwag generates TypeScript client for Angular frontend
- Use Minimal APIs pattern (not controllers)

#### **Frontend** (`src/Frontend/` - optional)
See `src/Frontend/.github/copilot-instructions.md` for Angular-specific guidelines.

---

### Test Projects (`tests/`)

| Project | Type | Purpose |
|---|---|---|
| **Domain.UnitTests** | Unit | Domain logic, value objects |
| **Application.UnitTests** | Unit | Command/query handlers (mocked dependencies) |
| **Infrastructure.IntegrationTests** | Integration | Database operations with real EF Core |
| **Application.FunctionalTests** | Functional | End-to-end API tests with WebApplicationFactory |

**Testing Stack**:
- **Framework**: NUnit
- **Mocking**: NSubstitute
- **Assertions**: Shouldly
- **Functional Tests**: WebApplicationFactory with SQL Server database (localdb or Docker container)

**Conventions**:
- One test class per handler/entity
- Arrange-Act-Assert pattern
- Descriptive test method names (e.g., `Should_CreateTodoItem_When_ValidRequest`)

---

## Key Patterns & Conventions

### 1. CQRS with Mediator

All operations go through Mediator:

```csharp
// Command (write)
public record CreateTodoItemCommand : IRequest<Guid>
{
    public string Title { get; init; } = null!;
}

// Query (read)
public record GetTodoItemQuery(Guid Id) : IRequest<TodoItemDto>;
```

**Rules**:
- Queries return DTOs
- One handler per request
- Validation via FluentValidation

### 2. ID Obfuscation with Sqids

Use `Sqids` to hide internal integer IDs:

- **Domain**: Entities use `int` IDs internally
- **API**: Expose `string` Sqids to clients
- **Sqiddler**: Automatic conversion via JSON value converter `[JsonSqid<TEntity>]`

### 3. Mapping with Mapperly

Use source-generated mappers (no reflection):

```csharp
[Mapper]
public static partial class TodoItemMapper
{
    // Map domain entity to DTO
    public static partial TodoItemSummaryDto ToSummaryDto(this TodoItem entity);

    // mapping with EF Core projection support
    public static partial IQueryable<TodoItemSummaryDto> ProjectToSummaryDto(this IQueryable<TodoItem> query);
}
```

### 4. Entity Configuration

EF Core configurations in separate files:

```csharp
public class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
{
    public void Configure(EntityTypeBuilder<TodoItem> builder) { }
}
```

### 5. Validation

FluentValidation for all requests:

```csharp
public class CreateTodoItemCommandValidator : AbstractValidator<CreateTodoItemCommand>
{
    public CreateTodoItemCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
    }
}
```

---

## Development Guidelines

### Adding a New Feature

1. **Domain**: Create entity in `Domain/Entities/`
2. **Infrastructure**: Add EF configuration in `Infrastructure/Data/Configurations/`
3. **Add Migration**: Run `dotnet ef migrations add {FeatureName} -p src/Infrastructure/ -s src/Web/` to create a new migration
4. **Application**: Add commands/queries in `Application/{FeatureName}/`
5. **Web**: Add endpoints in `Web/Endpoints/{FeatureName}Endpoints.cs`
6. **Tests**: Add tests in respective test projects

### Dependency Flow

```
Web → Application → Domain
  ↓
Infrastructure → Application
```

**Rules**:
- Domain has no dependencies
- Application depends only on Domain
- Infrastructure implements Application interfaces
- Web depends on Application and Infrastructure (composition root)

### API Documentation

- OpenAPI spec auto-generated on build → `wwwroot/api/swagger.json`
- Access Scalar UI at `/scalar/v1` (development)
- TypeScript client auto-generated for Angular frontend (if included)

---

## Code Style & Conventions

- **Naming**: PascalCase for public members, camelCase for private (see `.editorconfig`)
- **File Organization**: One class per file, named after the class
- **Nullability**: Nullable reference types enabled
- **Immutability**: Prefer `record` types for DTOs and value objects
- **Guards**: Use `Ardalis.GuardClauses` for argument validation

---

## Important Notes

- **Never edit generated code**: API clients are auto-generated by NSwag
- **Migration workflow**: Use EF Core migrations for database changes
- **Feature folders**: Organize by feature, not by technical layer
- **Minimal APIs**: All endpoints use Minimal API pattern (no controllers)
- **Source generators**: Mediator and Mapperly use source generation (no reflection)
