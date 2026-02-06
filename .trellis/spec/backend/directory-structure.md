# Directory Structure

> How backend code is organized in this project.

---

## Overview

This project uses **Clean Architecture** with four layers, ensuring separation of concerns and testability.

**Tech Stack**: .NET 9, Minimal API, SqlSugar ORM, Serilog, FluentValidation

---

## Directory Layout

```
src/apps/api/
├── ScaffoldGenerator.Api/              # API Layer (Entry Point)
│   ├── Program.cs                      # Startup, DI registration, routes
│   └── templates/                      # Scriban template files
│
├── ScaffoldGenerator.Application/      # Application Layer (Business Logic)
│   ├── Abstractions/                   # Interface definitions
│   │   ├── ITemplateRenderer.cs
│   │   ├── ITemplateFileProvider.cs
│   │   └── IZipBuilder.cs
│   ├── UseCases/                       # Business use cases
│   │   └── GenerateScaffoldUseCase.cs
│   └── Validators/                     # FluentValidation validators
│       └── GenerateScaffoldValidator.cs
│
├── ScaffoldGenerator.Contracts/        # Contracts Layer (DTOs)
│   ├── Enums/                          # Enum definitions
│   │   ├── DatabaseProvider.cs
│   │   ├── CacheProvider.cs
│   │   └── RouterMode.cs
│   ├── Requests/                       # Request DTOs
│   │   └── GenerateScaffoldRequest.cs
│   └── Responses/                      # Response DTOs
│       └── GenerationResult.cs
│
└── ScaffoldGenerator.Infrastructure/   # Infrastructure Layer (External Dependencies)
    ├── Rendering/                      # Template engine implementation
    │   └── ScribanTemplateRenderer.cs
    └── FileSystem/                     # File system operations
        ├── FileSystemTemplateProvider.cs
        └── SystemZipBuilder.cs
```

---

## Layer Responsibilities

| Layer | Responsibility | Dependencies |
|-------|----------------|--------------|
| **Api** | HTTP endpoints, middleware, DI setup | Application, Infrastructure |
| **Application** | Business logic, validation, use cases | Contracts only |
| **Contracts** | DTOs, enums, shared types | None (no dependencies) |
| **Infrastructure** | External service implementations | Application (for interfaces) |

### Dependency Flow

```
Api → Application → Contracts
 ↓
Infrastructure → Application (implements interfaces)
```

---

## Module Organization

### Adding a New Feature

1. **Define DTOs** in `Contracts/`
   - Request DTO in `Requests/`
   - Response DTO in `Responses/`
   - Enums in `Enums/`

2. **Define Interface** in `Application/Abstractions/`

3. **Implement UseCase** in `Application/UseCases/`
   - Add validator in `Validators/` if needed

4. **Implement Infrastructure** in `Infrastructure/`
   - Create folder for the feature domain

5. **Register in DI** in `Api/Program.cs`

6. **Add Endpoint** in `Api/Program.cs`

---

## Naming Conventions

| Type | Convention | Example |
|------|------------|---------|
| Project | `{Product}.{Layer}` | `ScaffoldGenerator.Application` |
| Interface | `I` prefix + PascalCase | `ITemplateRenderer` |
| UseCase | `{Action}{Entity}UseCase` | `GenerateScaffoldUseCase` |
| Validator | `{Entity}Validator` | `GenerateScaffoldValidator` |
| Request DTO | `{Action}{Entity}Request` | `GenerateScaffoldRequest` |
| Response DTO | `{Entity}Result` or `{Entity}Response` | `GenerationResult` |
| Implementation | Descriptive name | `ScribanTemplateRenderer` |

---

## Examples

### Well-Organized Feature: Scaffold Generation

| File | Purpose |
|------|---------|
| `Contracts/Requests/GenerateScaffoldRequest.cs` | Input DTO |
| `Contracts/Responses/GenerationResult.cs` | Output DTO |
| `Application/Abstractions/ITemplateRenderer.cs` | Interface |
| `Application/UseCases/GenerateScaffoldUseCase.cs` | Business logic |
| `Application/Validators/GenerateScaffoldValidator.cs` | Validation rules |
| `Infrastructure/Rendering/ScribanTemplateRenderer.cs` | Implementation |

---

## Forbidden Patterns

| Pattern | Why |
|---------|-----|
| Direct DB access in Api layer | Violates separation of concerns |
| Business logic in Infrastructure | Infrastructure is for external dependencies only |
| Circular project references | Breaks clean architecture |
| Static classes for business logic | Hard to test, violates DI principle |
