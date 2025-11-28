---
description: Markdown documentation standards for this project
applyTo: '**/*.md'
---

# Markdown Instructions

## Purpose

This file defines the standards and best practices for writing Markdown documentation in this project. All documentation should be clear, consistent, and follow these guidelines.

## General Principles

1. **Clarity First**: Write for developers who may be unfamiliar with the project
2. **Keep Updated**: Documentation should reflect the current state of the code
3. **Examples Matter**: Include code examples wherever possible
4. **Structure**: Use proper heading hierarchy
5. **Links**: Keep links working and relative when possible

## File Structure

### Heading Hierarchy

Always start with H1 (`#`) and follow proper nesting:

```markdown
# Main Title (H1) - Only one per document

## Section (H2)

### Subsection (H3)

#### Detail (H4)
```

❌ **Don't skip levels:**
```markdown
# Title
### Subsection (skipped H2)
```

✅ **Do follow hierarchy:**
```markdown
# Title
## Section
### Subsection
```

### Document Structure

Every markdown file should follow this structure:

```markdown
# Title

Brief description (1-2 sentences)

## Overview

More detailed introduction

## Main Content Sections

### Subsections

## Examples

Practical examples with code blocks

## Common Patterns / Best Practices

## Additional Resources

Links to related documentation
```

## Content Guidelines

### Code Blocks

Always specify the language for syntax highlighting:

✅ **Good:**
````markdown
```csharp
public class Example
{
    public string Name { get; set; }
}
```
````

❌ **Bad:**
````markdown
```
public class Example
{
    public string Name { get; set; }
}
```
````

**Supported languages in this project:**
- `csharp` - C# code
- `typescript` - TypeScript code
- `javascript` - JavaScript code
- `html` - HTML markup
- `css` - CSS styles
- `json` - JSON data
- `bash` - Shell commands
- `sql` - SQL queries
- `xml` - XML/config files

### Inline Code

Use backticks for:
- File names: `Program.cs`, `package.json`
- Class names: `TodoItem`, `ApplicationDbContext`
- Method names: `SaveChangesAsync()`, `Handle()`
- Variables: `userId`, `context`
- Commands: `dotnet run`, `npm install`
- Paths: `src/Domain/Entities/`

### Emphasis

- **Bold** (`**text**`) for important concepts, warnings, or emphasis
- *Italic* (`*text*`) for terms being introduced or light emphasis
- `Code` (`` `text` ``) for technical terms, code elements, file names

### Lists

**Unordered Lists:**
```markdown
- First item
- Second item
  - Nested item
  - Another nested item
- Third item
```

**Ordered Lists:**
```markdown
1. First step
2. Second step
3. Third step
```

**Use ordered lists for:**
- Step-by-step instructions
- Sequential processes
- Prioritized items

**Use unordered lists for:**
- Features
- Benefits
- Non-sequential items

### Checkboxes (✅/❌)

Use for Do's and Don'ts:

```markdown
### Best Practices

✅ **DO:**
- Follow clean architecture principles
- Write tests for business logic
- Use meaningful names

❌ **DON'T:**
- Put business logic in controllers
- Skip validation
- Use magic numbers
```

### Links

**Internal Links** (within the project):
```markdown
[Clean Architecture Guide](./Agent.md)
[TypeScript Standards](./.github/instructions/typescript.md)
```

**External Links:**
```markdown
[MediatR Documentation](https://github.com/jbogard/MediatR)
```

**Link to code:**
```markdown
See [`TodoItem`](../src/Domain/Entities/TodoItem.cs) for implementation.
```

### Tables

Use tables for structured comparison or reference data:

```markdown
| Layer | Responsibility | Dependencies |
|-------|---------------|--------------|
| Domain | Business logic | None |
| Application | Use cases | Domain |
| Infrastructure | Implementation | Domain, Application |
| Web | API endpoints | Application, Infrastructure |
```

**Table Guidelines:**
- Keep tables simple and readable
- Use for 3+ columns of data
- Include header row
- Align columns for readability in source

### Diagrams (ASCII)

Use simple ASCII diagrams for architecture:

```markdown
┌─────────────────────────────────────┐
│         Presentation (Web)          │
├─────────────────────────────────────┤
│       Infrastructure                │
├─────────────────────────────────────┤
│         Application                 │
├─────────────────────────────────────┤
│           Domain                    │
└─────────────────────────────────────┘
```

For complex diagrams, link to external tools:
- Mermaid diagrams (GitHub supports these)
- Draw.io diagrams
- PlantUML

### Admonitions/Callouts

Use blockquotes for important notes:

```markdown
> **Note:** This feature requires .NET 10 or higher.

> **Warning:** Modifying this file may break the build.

> **Tip:** Use Mapperly for better performance than AutoMapper.
```

## Project-Specific Standards

### README.md

Should include:
1. Project title and description
2. How to install/create from template
3. How to run (different platforms)
4. Architecture overview
5. Technologies used
6. Deployment information

### Agent.md

Should include:
1. Comprehensive architecture guide
2. Layer-by-layer breakdown
3. Technology usage examples
4. Development workflow
5. Best practices
6. Common patterns

### API Documentation

When documenting endpoints:

```markdown
### Create Todo Item

**Endpoint:** `POST /api/todoitems`

**Request:**
```json
{
  "title": "Buy groceries",
  "listId": 1,
  "priority": "High"
}
```

**Response:** `201 Created`
```json
{
  "id": "abc123",
  "title": "Buy groceries",
  "done": false
}
```

**Errors:**
- `400 Bad Request` - Invalid input
- `401 Unauthorized` - Not authenticated
- `404 Not Found` - List not found
```

### Feature Documentation

When documenting a feature:

```markdown
## Feature Name

### Overview
Brief description of the feature.

### Use Cases
- Use case 1
- Use case 2

### Implementation
How it's implemented in the codebase.

### Example Usage
\```csharp
// Code example
\```

### Testing
How to test this feature.
```

## Formatting Standards

### Line Length

- Aim for 80-120 characters per line
- Break long sentences into multiple lines
- Exception: URLs and code blocks

### Spacing

- One blank line between sections
- Two blank lines before major sections (H2)
- No trailing whitespace

### File Names

- Use kebab-case: `clean-architecture.md`
- Be descriptive: `typescript-coding-standards.md`
- No spaces in file names

## Maintenance

### Regular Updates

Update documentation when:
- Adding new features
- Changing architecture
- Updating dependencies
- Fixing bugs that affect usage
- Adding new patterns

### Version Information

Include version/date information for major documentation:

```markdown
---
Last Updated: 2025-11-28
Version: 1.0.0
---
```

### Deprecation Notices

Mark deprecated features clearly:

```markdown
> **⚠️ DEPRECATED:** This approach is deprecated as of v2.0. 
> Use [new approach](./new-way.md) instead.
```

## Tools and Validation

### Linting

Consider using:
- `markdownlint` for consistency
- VS Code extension: `DavidAnson.vscode-markdownlint`

### Preview

Always preview markdown:
- In VS Code: `Ctrl+Shift+V` (Windows/Linux) or `Cmd+Shift+V` (Mac)
- In GitHub: Check the rendered preview

## Examples

### Good Documentation Example

```markdown
# TodoItem Entity

The `TodoItem` entity represents a single task in a todo list.

## Properties

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `int` | Unique identifier |
| `Title` | `string` | Task description |
| `Done` | `bool` | Completion status |
| `Priority` | `PriorityLevel` | Task priority |

## Usage

```csharp
var item = new TodoItem
{
    Title = "Buy groceries",
    Priority = PriorityLevel.High
};

item.Done = true; // Raises TodoItemCompletedEvent
```

## Domain Events

When `Done` is set to `true`, the entity raises a `TodoItemCompletedEvent`.

## Validation Rules

- Title is required (max 200 characters)
- Priority must be a valid enum value

## Related

- [TodoList Entity](./TodoList.md)
- [Creating Todo Items](../commands/CreateTodoItem.md)
```

### Common Mistakes to Avoid

❌ **Don't:**
```markdown
## bad heading with no capitalization

check out this code:
public void Method() { }

Click [here](link) for more info

See file Program.cs for implementation
```

✅ **Do:**
```markdown
## Bad Heading with Proper Capitalization

Check out this code:

```csharp
public void Method() { }
```

See the [MediatR documentation](https://github.com/jbogard/MediatR) for more information.

See [`Program.cs`](../src/Web/Program.cs) for implementation.
```

## Review Checklist

Before committing markdown changes, verify:

- [ ] Headings follow proper hierarchy
- [ ] Code blocks have language specified
- [ ] All links work
- [ ] Spelling and grammar checked
- [ ] Examples are accurate and tested
- [ ] Formatting is consistent
- [ ] Technical terms use inline code formatting
- [ ] Tables are properly formatted
- [ ] No trailing whitespace
- [ ] File renders correctly in preview

## Resources

- [Markdown Guide](https://www.markdownguide.org/)
- [GitHub Flavored Markdown](https://github.github.com/gfm/)
- [CommonMark Specification](https://commonmark.org/)
