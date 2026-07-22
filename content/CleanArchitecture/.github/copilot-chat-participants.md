# GitHub Copilot Chat Instructions

This file configures chat-specific instructions and MCP (Model Context Protocol) servers for GitHub Copilot to provide enhanced assistance with documentation and resources.

## Chat Participants

### @workspace - Project Context

When asking @workspace questions, Copilot has access to:
- Clean Architecture implementation in this project
- Backend .NET 10 code (Domain, Application, Infrastructure, Web)
- Frontend code (Angular/React)
- Test projects
- Configuration files
- All documentation (Agent.md, README.md, instruction files)

**Example questions:**
- `@workspace How do I add a new feature following Clean Architecture?`
- `@workspace Where should I put validation logic?`
- `@workspace Show me examples of MediatR commands in this project`
- `@workspace How is the database configured?`

### @terminal - Command Help

Ask @terminal for help with:
- Running the application
- Build commands
- Database migrations
- Running tests
- npm/dotnet CLI commands

**Example questions:**
- `@terminal How do I run the backend?`
- `@terminal How do I create a new migration?`
- `@terminal How do I run the tests?`

## MCP Servers (Model Context Protocol)

MCP servers provide external knowledge and documentation to enhance Copilot's responses.

### Microsoft Learn MCP Server

Access official Microsoft and .NET documentation directly in Copilot.

**Configuration:**

Add to your VS Code `settings.json`:

```json
{
  "github.copilot.chat.mcp.servers": {
    "microsoft-docs": {
      "command": "npx",
      "args": [
        "-y",
        "@modelcontextprotocol/server-microsoft-docs"
      ]
    }
  }
}
```

**What it provides:**
- ASP.NET Core documentation
- Entity Framework Core guides
- C# language reference
- .NET API documentation
- Azure services documentation
- Best practices and tutorials

**Example usage:**
```
@mcp microsoft-docs How do I configure Entity Framework Core with SQL Server?
@mcp microsoft-docs What are the new features in .NET 10?
@mcp microsoft-docs How do I implement authentication in ASP.NET Core?
@mcp microsoft-docs Show me MediatR best practices from Microsoft docs
```

### Angular MCP Server (Community)

Access Angular documentation for frontend development.

**Configuration:**

```json
{
  "github.copilot.chat.mcp.servers": {
    "angular-docs": {
      "command": "npx",
      "args": [
        "-y",
        "mcp-server-angular-docs"
      ]
    }
  }
}
```

**What it provides:**
- Angular components and directives
- Reactive forms documentation
- RxJS patterns
- Angular CLI commands
- Router configuration
- Signals and modern Angular features

**Example usage:**
```
@mcp angular-docs How do I use Angular signals?
@mcp angular-docs What's the new control flow syntax?
@mcp angular-docs How do I implement lazy loading?
@mcp angular-docs Show me reactive forms best practices
```

### Combined MCP Server Configuration

Full configuration for VS Code `settings.json`:

```json
{
  "github.copilot.chat.mcp.servers": {
    "microsoft-docs": {
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/server-microsoft-docs"]
    },
    "angular-docs": {
      "command": "npx",
      "args": ["-y", "mcp-server-angular-docs"]
    }
  }
}
```

## Project-Specific Chat Patterns

### Architecture Questions

Ask Copilot about Clean Architecture patterns in this project:

```
Explain the dependency flow in this Clean Architecture solution

Which layer should I modify to add a new database table?

How do I implement a new CQRS command?

Show me how domain events work in this project
```

### Code Generation

Ask Copilot to generate code following project patterns:

```
Generate a new MediatR command for creating a Product entity

Create a new Minimal API endpoint for managing Orders

Write a FluentValidation validator for this command

Generate a Mapperly mapper for User entity to UserDto
```

### Technology-Specific Questions

**For MediatR:**
```
How do I create a notification handler for domain events?

Show me how to use pipeline behaviors

How do I handle transactions in MediatR?
```

**For Entity Framework Core:**
```
@mcp microsoft-docs How do I configure many-to-many relationships in EF Core?

Show me how to implement soft delete in this project

How do I optimize this query for performance?
```

**For Angular (if using Angular frontend):**
```
@mcp angular-docs How do I use signals in components?

Generate a reactive form component

How do I integrate with the NSwag generated client?
```

**For Mapperly:**
```
How do I configure custom property mappings?

How do I map collections with Mapperly?

Show me how to handle nested object mapping
```

### Testing Questions

```
Generate a functional test for this command handler

Write unit tests for this validator

How do I mock the DbContext in tests?

Show me how to set up test data
```

## Best Practices for Chat

### Be Specific

❌ **Vague:**
```
How do I add a feature?
```

✅ **Specific:**
```
How do I add a new CQRS command in the Application layer to delete a TodoItem, 
following the patterns in this project?
```

### Reference Project Context

❌ **Generic:**
```
How do I use MediatR?
```

✅ **Project-specific:**
```
@workspace Show me how MediatR commands are structured in this project, 
using TodoItem as an example
```

### Combine MCP with Project Context

```
@workspace and @mcp microsoft-docs How should I implement pagination 
for the GetTodoItems query using EF Core best practices?

@workspace How do I integrate @mcp angular-docs Angular signals 
with the NSwag generated client?
```

## Common Chat Workflows

### 1. Adding a New Feature

```
Step 1: @workspace What's the pattern for adding a new entity in the Domain layer?

Step 2: Generate a new Product entity with Name, Price, and Category properties

Step 3: @workspace Show me how to create CQRS commands and queries for this entity

Step 4: Generate CreateProductCommand with handler

Step 5: @workspace How do I add endpoints for this feature?

Step 6: Generate tests for the CreateProductCommand
```

### 2. Understanding Existing Code

```
@workspace Explain how TodoItemCompletedEvent is raised and handled

@workspace What validation rules are applied to CreateTodoItemCommand?

@workspace Show me the data flow from API endpoint to database for creating a TodoItem
```

### 3. Debugging and Troubleshooting

```
@workspace Why am I getting a validation error in this command?

@mcp microsoft-docs How do I debug Entity Framework Core queries?

@workspace Show me similar error handling patterns in this project
```

### 4. Learning Project Patterns

```
@workspace What are all the MediatR pipeline behaviors in this project?

@workspace How is authentication configured?

@workspace Show me examples of Mapperly mappers with custom mappings

@workspace How are domain events dispatched?
```

## MCP Server Installation

### Prerequisites

- Node.js installed
- npx available (comes with npm)

### Testing MCP Servers

Test if MCP servers are working:

```bash
# Test Microsoft Docs MCP
npx -y @modelcontextprotocol/server-microsoft-docs --help

# Test Angular Docs MCP (when available)
npx -y mcp-server-angular-docs --help
```

### Troubleshooting MCP

If MCP servers don't work:

1. **Check VS Code version**: Ensure you have the latest version
2. **Restart VS Code**: After adding MCP configuration
3. **Check internet connection**: MCP servers need internet access
4. **View output**: Check "GitHub Copilot Chat" output panel for errors

## Alternative Documentation Sources

If MCP servers are not available, you can still reference documentation:

### Microsoft Learn
- Direct URL: https://learn.microsoft.com
- Ask: "According to Microsoft Learn documentation..."

### Angular Documentation
- Direct URL: https://angular.dev
- Ask: "Based on Angular documentation..."

### Project Documentation
- **Agent.md**: Comprehensive architecture guide
- **README.md**: Getting started and overview
- **.github/instructions/**: Specific coding standards

**Example:**
```
Based on Agent.md, how should I implement this feature?

According to .github/instructions/clean-architecture.md, 
where does this code belong?
```

## Tips for Effective Copilot Chat

1. **Use @workspace** for project-specific questions
2. **Use @mcp** for official documentation lookups
3. **Be specific** about what you want to achieve
4. **Reference files** by name when asking about specific code
5. **Ask for examples** from the project when learning patterns
6. **Request step-by-step** guidance for complex tasks
7. **Verify generated code** against project standards
8. **Ask for tests** when generating new features

## Examples of Great Prompts

### Architecture
```
@workspace Explain the dependency flow between layers in this Clean Architecture 
solution, using TodoItem as an example from creation to database storage.
```

### Code Generation
```
Generate a complete CQRS implementation for managing a Customer entity, 
including: Domain entity, CreateCustomerCommand, GetCustomersQuery, 
UpdateCustomerCommand, DeleteCustomerCommand, DTOs, Mapperly mappers, 
validators, and Minimal API endpoints. Follow the patterns used for TodoItem 
in this project.
```

### Learning
```
@workspace and @mcp microsoft-docs Show me how to implement optimistic 
concurrency in Entity Framework Core for the TodoItem entity, following 
the patterns already established in this project.
```

### Debugging
```
@workspace I'm getting a "Cannot access a disposed object" error in my 
command handler. Show me how other handlers in this project manage 
DbContext lifecycle.
```

### Testing
```
Generate comprehensive functional tests for UpdateTodoItemCommand including 
success cases, validation failures, and not found scenarios, following the 
patterns in Application.FunctionalTests.
```

## Resources

- [GitHub Copilot Documentation](https://docs.github.com/copilot)
- [MCP Documentation](https://modelcontextprotocol.io/)
- [Microsoft Learn](https://learn.microsoft.com)
- [Angular Documentation](https://angular.dev)
- Project Agent.md for architecture details
