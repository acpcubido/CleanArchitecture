# testproject

This is a **.NET 10** solution based on the **Cubido Clean Architecture Template**.

## Getting Started

### Creating a New Project

This project was generated using the Cubido template. To create a new project from this template:

```bash
# Install the Cubido template
dotnet new install Cubido.Templates

# Create a new project
dotnet new cubido-template -n YourProjectName
```

## How to Run

### On Windows (Visual Studio)

1. Open the `Cubido.Template.slnx` solution file in Visual Studio
2. Press **F5** to build and run the application in debug mode
3. The application will start with both the backend API and the Angular frontend

### On macOS / Linux

1. Navigate to the project root directory
2. Run the application using the .NET CLI:

```bash
# Run the Web API (backend)
dotnet run --project src/Web/Web.csproj

# In a separate terminal, run the Angular frontend (if needed)
cd src/Frontend
npm install
npm start
```

## Clean Architecture Overview

This solution follows **Clean Architecture** principles, which organize code into layers with clear dependencies flowing inward. This approach ensures maintainability, testability, and separation of concerns.

### What is Clean Architecture?

Clean Architecture (popularized by Robert C. Martin) structures applications in concentric layers:

- **Core Business Logic** (innermost) - Independent of external concerns
- **Application Logic** - Use cases and business workflows
- **Infrastructure** - External dependencies (database, APIs, file system)
- **Presentation** - User interface and API endpoints (outermost)

**Key Principle**: Dependencies always point inward. Inner layers never depend on outer layers.

### Project Structure

This solution implements Clean Architecture with the following layers:

#### **Domain Layer** (`src/Domain/`)
The **innermost layer** containing enterprise business rules and domain entities.
- **Entities**: Core business objects (`TodoItem`, `TodoList`)
- **Value Objects**: Immutable objects representing concepts (`Colour`)
- **Enums**: Domain-specific enumerations (`PriorityLevel`)
- **Events**: Domain events (`TodoItemCreatedEvent`, `TodoItemCompletedEvent`)
- **Exceptions**: Domain-specific exceptions
- **No dependencies** on other projects - pure business logic

#### **Application Layer** (`src/Application/`)
Contains **application business rules** and orchestrates domain logic.
- **Commands & Queries** (CQRS pattern): Use cases for the application
- **Behaviours**: Cross-cutting concerns (validation, logging, transaction management)
- **Interfaces**: Abstractions for infrastructure services
- **Mappings**: Object mapping configurations (using Mapperly)
- **DTOs/Models**: Data transfer objects
- **Event Handlers**: Handles domain events
- **Depends only on**: Domain layer

#### **Infrastructure Layer** (`src/Infrastructure/`)
Implements **interfaces from the Application layer** with concrete implementations.
- **Data**: Database context, configurations, migrations (Entity Framework Core)
- **Identity**: Authentication and authorization implementation
- **External Services**: File storage, email, third-party APIs
- **Depends on**: Domain and Application layers

#### **Web/Presentation Layer** (`src/Web/`)
The **API layer** that exposes functionality to clients.
- **Endpoints**: Minimal API endpoints
- **Services**: Presentation-specific services
- **Configuration**: Application startup and configuration
- **wwwroot**: Static files
- **Depends on**: Application and Infrastructure layers

#### **Frontend** (`src/Frontend/`)
Angular-based **user interface**.
- Independent SPA that communicates with the Web API
- Generated TypeScript clients using NSwag

#### **Tests**
- **Domain.UnitTests**: Tests for domain logic
- **Application.UnitTests**: Tests for application use cases
- **Application.FunctionalTests**: End-to-end tests with test database
- **Infrastructure.IntegrationTests**: Tests for infrastructure implementations

### Benefits of This Structure

✅ **Testability**: Business logic is independent and easy to test  
✅ **Maintainability**: Clear separation of concerns  
✅ **Flexibility**: Easy to swap implementations (e.g., change databases)  
✅ **Independence**: UI and database can change without affecting business rules  
✅ **Reusability**: Domain and application logic can be shared across multiple interfaces  

## Technologies Used

-   **ASP.NET Core 10** - Web framework
-   **MediatR** - CQRS and mediator pattern implementation
-   **Mapperly** - Compile-time object mapper
-   **Entity Framework Core** - ORM for data access
-   **Sqids & Sqiddler** - ID obfuscation
-   **Angular** - Frontend framework
-   **NUnit, NSubstitute, Shouldly** - Testing frameworks

## Deployment

Bicep templates for Azure deployment are available in the `deployment/` folder.
