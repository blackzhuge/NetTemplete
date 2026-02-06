# Error Handling

> How errors are handled in this project.

---

## Overview

This project uses a **Result Pattern** for business logic errors and **Global Exception Middleware** for unexpected exceptions.

**Principle**: Use return values for expected failures, exceptions for unexpected failures.

---

## Error Handling Strategy

| Error Type | Handling Method | Example |
|------------|-----------------|---------|
| Validation errors | Return `Result` with errors | Invalid input |
| Business rule violations | Return `Result` with errors | Insufficient balance |
| Not found | Return `Result` with errors | Entity not found |
| Infrastructure failures | Throw exception | Database down |
| Unexpected errors | Caught by middleware | NullReferenceException |

---

## Result Pattern

### Result Type Definition

**File**: `Contracts/Responses/GenerationResult.cs`

```csharp
public sealed record GenerationResult
{
    public bool Success { get; init; }
    public string FileName { get; init; } = string.Empty;
    public byte[] FileContent { get; init; } = [];
    public string? ErrorMessage { get; init; }
}
```

### Usage in UseCase

**File**: `Application/UseCases/GenerateScaffoldUseCase.cs`

```csharp
public async Task<GenerationResult> ExecuteAsync(
    GenerateScaffoldRequest request,
    CancellationToken ct = default)
{
    // 1. Validation
    var validationResult = await _validator.ValidateAsync(request, ct);
    if (!validationResult.IsValid)
    {
        var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
        return new GenerationResult
        {
            Success = false,
            ErrorMessage = errors
        };
    }

    // 2. Business Logic
    try
    {
        // ... processing ...
        return new GenerationResult
        {
            Success = true,
            FileName = fileName,
            FileContent = zipContent
        };
    }
    catch (Exception ex)
    {
        return new GenerationResult
        {
            Success = false,
            ErrorMessage = $"生成失败: {ex.Message}"
        };
    }
}
```

---

## Input Validation

### FluentValidation Pattern

**File**: `Application/Validators/GenerateScaffoldValidator.cs`

```csharp
public sealed class GenerateScaffoldValidator : AbstractValidator<GenerateScaffoldRequest>
{
    public GenerateScaffoldValidator()
    {
        RuleFor(x => x.ProjectName)
            .NotEmpty().WithMessage("项目名称不能为空")
            .Length(2, 50).WithMessage("项目名称长度应在 2-50 个字符之间")
            .Matches(@"^[a-zA-Z][a-zA-Z0-9_]*$")
            .WithMessage("项目名称只能包含字母、数字和下划线，且必须以字母开头");

        RuleFor(x => x.Namespace)
            .NotEmpty().WithMessage("命名空间不能为空")
            .Matches(@"^[a-zA-Z_][a-zA-Z0-9_]*(\.[a-zA-Z_][a-zA-Z0-9_]*)*$")
            .WithMessage("命名空间格式无效");
    }
}
```

### Validation in UseCase

```csharp
var validationResult = await _validator.ValidateAsync(request, ct);
if (!validationResult.IsValid)
{
    var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
    return new GenerationResult
    {
        Success = false,
        ErrorMessage = errors
    };
}
```

---

## Global Exception Middleware

### Pattern: Inline Middleware

**File**: `Api/Program.cs` (Lines 46-58)

```csharp
// Exception handling middleware
app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Unhandled exception");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { error = "服务器内部错误" });
    }
});
```

---

## API Error Responses

### Success Response

```json
{
  "success": true,
  "data": { ... }
}
```

Or file download (binary response with headers).

### Validation Error (400)

```json
{
  "error": "项目名称不能为空; 命名空间格式无效"
}
```

### Business Error (400)

```json
{
  "error": "生成失败: 模板文件不存在"
}
```

### Server Error (500)

```json
{
  "error": "服务器内部错误"
}
```

---

## Infrastructure Exception Pattern

When external dependencies fail, throw descriptive exceptions:

**File**: `Infrastructure/Rendering/ScribanTemplateRenderer.cs`

```csharp
public async Task<string> RenderAsync(string templatePath, object model, CancellationToken ct = default)
{
    var templateContent = await _fileProvider.ReadTemplateAsync(templatePath, ct);
    var template = Template.Parse(templateContent);

    if (template.HasErrors)
    {
        var errors = string.Join("; ", template.Messages.Select(m => m.Message));
        throw new InvalidOperationException($"模板解析错误: {errors}");
    }

    // ... render ...
}
```

---

## Common Mistakes

| Mistake | Correct Approach |
|---------|------------------|
| Using exceptions for flow control | Use Result Pattern for expected failures |
| Swallowing exceptions silently | Always log before returning error |
| Exposing internal error details | Return generic message for 500 errors |
| Missing validation | Validate at entry point (API/UseCase) |
| Inconsistent error format | Use standard error response structure |

---

## Checklist

- [ ] Validation errors return 400 with message
- [ ] Business errors return Result with `Success = false`
- [ ] Infrastructure exceptions are thrown and caught by middleware
- [ ] All exceptions are logged with `Log.Error(ex, "message")`
- [ ] 500 errors never expose internal details
