# 错误处理

> 本项目的错误处理方式。

---

## 策略

| 错误类型 | 处理方式 | 示例 |
|----------|----------|------|
| 验证错误 | 返回 `Result` + ErrorCode | 无效输入 |
| 业务规则违反 | 返回 `Result` + ErrorCode | 配置组合冲突 |
| 基础设施故障 | 抛出异常 | 模板渲染失败 |
| 意外错误 | 被中间件捕获 | NullReferenceException |

**原则**：预期失败用返回值，意外失败用异常。

---

## 错误码分类

```csharp
public enum ErrorCode
{
    None = 0,
    ValidationError = 1,    // → 400 Bad Request
    InvalidCombination = 2, // → 422 Unprocessable Entity
    TemplateError = 3       // → 500 Internal Server Error
}
```

| ErrorCode | HTTP | 场景 |
|-----------|------|------|
| ValidationError | 400 | 必填字段缺失、格式错误 |
| InvalidCombination | 422 | 配置组合不合法（如 MySQL + 无连接串） |
| TemplateError | 500 | 模板渲染失败 |

---

## Result Pattern

```csharp
public sealed record GenerationResult
{
    public bool Success { get; init; }
    public ErrorCode ErrorCode { get; init; } = ErrorCode.None;
    public string? ErrorMessage { get; init; }
    public byte[] FileContent { get; init; } = [];
    public string FileName { get; init; } = string.Empty;
}
```

### UseCase 中使用

```csharp
public async Task<GenerationResult> ExecuteAsync(
    GenerateScaffoldRequest request,
    CancellationToken ct = default)
{
    // 1. 验证
    var validation = await _validator.ValidateAsync(request, ct);
    if (!validation.IsValid)
    {
        var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
        return new GenerationResult
        {
            Success = false,
            ErrorCode = ErrorCode.ValidationError,
            ErrorMessage = errors
        };
    }

    // 2. 业务逻辑
    // ...
}
```

---

## API 错误响应

```csharp
app.MapPost("/api/v1/scaffolds/generate-zip", async (...) =>
{
    var result = await useCase.ExecuteAsync(request, ct);

    if (!result.Success)
    {
        var statusCode = result.ErrorCode switch
        {
            ErrorCode.ValidationError => 400,
            ErrorCode.InvalidCombination => 422,
            ErrorCode.TemplateError => 500,
            _ => 400
        };
        return Results.Json(new { error = result.ErrorMessage }, statusCode: statusCode);
    }

    return Results.File(result.FileContent, "application/zip", result.FileName);
});
```

---

## 全局异常中间件

```csharp
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

## 检查清单

- [ ] 验证错误返回 400 + ValidationError
- [ ] 配置冲突返回 422 + InvalidCombination
- [ ] 基础设施异常抛出并被中间件捕获
- [ ] 所有异常用 `Log.Error(ex, "message")` 记录
- [ ] 500 错误永不暴露内部详情
