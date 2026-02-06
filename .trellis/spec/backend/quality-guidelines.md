# Quality Guidelines

> Code quality standards for backend development.

---

## Overview

**Framework**: .NET 9
**API Style**: Minimal API
**Testing**: xUnit
**Validation**: FluentValidation

---

## Required Patterns

### 1. Immutable DTOs with Records

```csharp
// REQUIRED: Use record for DTOs
public sealed record GenerateScaffoldRequest
{
    public required string ProjectName { get; init; }
    public required string Namespace { get; init; }
    public DatabaseProvider Database { get; init; } = DatabaseProvider.SQLite;
    public bool EnableSwagger { get; init; } = true;
}

// REQUIRED: Use record for responses
public sealed record GenerationResult
{
    public bool Success { get; init; }
    public string FileName { get; init; } = string.Empty;
    public byte[] FileContent { get; init; } = [];
    public string? ErrorMessage { get; init; }
}
```

**Key Features**:
- `sealed` modifier (prevent inheritance)
- `record` type (immutability)
- `init` properties (set only during construction)
- `required` for mandatory fields
- `= []` for empty collections (C# 12)

### 2. Constructor Injection

```csharp
public sealed class GenerateScaffoldUseCase
{
    private readonly ITemplateRenderer _templateRenderer;
    private readonly IZipBuilder _zipBuilder;
    private readonly IValidator<GenerateScaffoldRequest> _validator;

    public GenerateScaffoldUseCase(
        ITemplateRenderer templateRenderer,
        IZipBuilder zipBuilder,
        IValidator<GenerateScaffoldRequest> validator)
    {
        _templateRenderer = templateRenderer;
        _zipBuilder = zipBuilder;
        _validator = validator;
    }
}
```

### 3. Async with CancellationToken

```csharp
// REQUIRED: All async methods accept CancellationToken
public interface ITemplateRenderer
{
    Task<string> RenderAsync(string templatePath, object model, CancellationToken ct = default);
}

// Implementation
public async Task<string> RenderAsync(string templatePath, object model, CancellationToken ct = default)
{
    var content = await _fileProvider.ReadTemplateAsync(templatePath, ct);
    // ...
}
```

### 4. Minimal API Endpoints

```csharp
app.MapPost("/api/generate", async (
    GenerateScaffoldRequest request,
    GenerateScaffoldUseCase useCase,
    CancellationToken ct) =>
{
    var result = await useCase.ExecuteAsync(request, ct);

    if (!result.Success)
    {
        return Results.BadRequest(new { error = result.ErrorMessage });
    }

    return Results.File(result.FileContent, "application/zip", result.FileName);
});
```

---

## Naming Conventions

| Type | Convention | Example |
|------|------------|---------|
| Interface | `I` prefix + PascalCase | `ITemplateRenderer` |
| Class | PascalCase | `ScribanTemplateRenderer` |
| Method | PascalCase + `Async` suffix for async | `RenderAsync` |
| Parameter | camelCase | `templatePath`, `ct` |
| Private field | `_` prefix + camelCase | `_templateRenderer` |
| Constant | PascalCase | `MaxRetryCount` |
| Enum | PascalCase (singular) | `DatabaseProvider` |
| Enum value | PascalCase | `SQLite`, `MySQL` |

---

## DI Registration Pattern

**File**: `Api/Program.cs`

```csharp
// Register by interface -> implementation
builder.Services.AddScoped<IValidator<GenerateScaffoldRequest>, GenerateScaffoldValidator>();
builder.Services.AddScoped<IZipBuilder, SystemZipBuilder>();
builder.Services.AddScoped<ITemplateRenderer, ScribanTemplateRenderer>();

// Factory for configuration-dependent services
builder.Services.AddScoped<ITemplateFileProvider>(_ =>
    new FileSystemTemplateProvider(Path.Combine(Directory.GetCurrentDirectory(), "templates")));

// UseCase (concrete class)
builder.Services.AddScoped<GenerateScaffoldUseCase>();
```

### Service Lifetimes

| Lifetime | When to Use |
|----------|-------------|
| `Scoped` | Default for most services, one per request |
| `Singleton` | Stateless utilities, configuration, caches |
| `Transient` | Lightweight, stateless, cheap to create |

---

## Forbidden Patterns

### 1. Mutable DTOs

```csharp
// FORBIDDEN
public class UserDto
{
    public string Name { get; set; }  // Mutable!
}

// REQUIRED
public sealed record UserDto
{
    public required string Name { get; init; }
}
```

### 2. Static Classes for Business Logic

```csharp
// FORBIDDEN
public static class UserService
{
    public static User GetUser(int id) { ... }
}

// REQUIRED: Use DI
public sealed class UserService : IUserService
{
    public User GetUser(int id) { ... }
}
```

### 3. Missing sealed Modifier

```csharp
// FORBIDDEN (allows unintended inheritance)
public class UserRepository { }

// REQUIRED
public sealed class UserRepository { }
```

### 4. Async Without CancellationToken

```csharp
// FORBIDDEN
public async Task<string> ProcessAsync()

// REQUIRED
public async Task<string> ProcessAsync(CancellationToken ct = default)
```

### 5. Service Locator Pattern

```csharp
// FORBIDDEN
var service = serviceProvider.GetService<IUserService>();

// REQUIRED: Constructor injection
public MyClass(IUserService userService)
```

---

## Testing Requirements

### Framework

```xml
<PackageVersion Include="xunit" Version="2.6.4" />
```

### Test Structure (TODO)

```
tests/
├── ScaffoldGenerator.Application.Tests/
│   └── UseCases/
│       └── GenerateScaffoldUseCaseTests.cs
└── ScaffoldGenerator.Infrastructure.Tests/
    └── Rendering/
        └── ScribanTemplateRendererTests.cs
```

### Test Naming

```csharp
[Fact]
public async Task ExecuteAsync_WithValidRequest_ReturnsSuccessResult()

[Fact]
public async Task ExecuteAsync_WithInvalidProjectName_ReturnsValidationError()
```

**Format**: `{Method}_{Scenario}_{ExpectedResult}`

---

## Code Review Checklist

### Before Submitting

- [ ] All DTOs use `sealed record` with `init` properties
- [ ] All async methods accept `CancellationToken`
- [ ] All classes are `sealed` (unless inheritance is needed)
- [ ] No `static` classes for business logic
- [ ] Dependencies injected via constructor
- [ ] Validation via FluentValidation
- [ ] Error handling follows Result Pattern

### Reviewer Checklist

- [ ] Naming follows conventions
- [ ] No forbidden patterns used
- [ ] Clean Architecture layers respected
- [ ] No circular dependencies
- [ ] Proper error handling
- [ ] Logging where appropriate

---

## Build Commands

```bash
# Restore packages
dotnet restore src/apps/api/ScaffoldGenerator.Api/ScaffoldGenerator.Api.csproj

# Build
dotnet build src/apps/api/ScaffoldGenerator.Api/ScaffoldGenerator.Api.csproj

# Run
dotnet run --project src/apps/api/ScaffoldGenerator.Api/ScaffoldGenerator.Api.csproj

# Test (when tests exist)
dotnet test
```

---

## Package Management

Central package management via `Directory.Packages.props`:

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <!-- ORM -->
    <PackageVersion Include="SqlSugarCore" Version="5.1.4.169" />
    <!-- Logging -->
    <PackageVersion Include="Serilog.AspNetCore" Version="9.0.0" />
    <!-- Validation -->
    <PackageVersion Include="FluentValidation" Version="11.9.0" />
    <!-- etc. -->
  </ItemGroup>
</Project>
```

**Rule**: Add new packages to `Directory.Packages.props`, not individual `.csproj` files.
