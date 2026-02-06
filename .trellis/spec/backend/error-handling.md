# 错误处理

> 本项目的错误处理方式。

---

## 概述

本项目使用 **Result Pattern（结果模式）** 处理业务逻辑错误，使用**全局异常中间件**处理意外异常。

**原则**：预期失败使用返回值，意外失败使用异常。

---

## 错误处理策略

| 错误类型 | 处理方式 | 示例 |
|----------|----------|------|
| 验证错误 | 返回带错误的 `Result` | 无效输入 |
| 业务规则违反 | 返回带错误的 `Result` | 余额不足 |
| 未找到 | 返回带错误的 `Result` | 实体不存在 |
| 基础设施故障 | 抛出异常 | 数据库宕机 |
| 意外错误 | 被中间件捕获 | NullReferenceException |

---

## Result Pattern

### Result 类型定义

**文件**：`Contracts/Responses/GenerationResult.cs`

```csharp
public sealed record GenerationResult
{
    public bool Success { get; init; }
    public string FileName { get; init; } = string.Empty;
    public byte[] FileContent { get; init; } = [];
    public string? ErrorMessage { get; init; }
}
```

### UseCase 中的用法

**文件**：`Application/UseCases/GenerateScaffoldUseCase.cs`

```csharp
public async Task<GenerationResult> ExecuteAsync(
    GenerateScaffoldRequest request,
    CancellationToken ct = default)
{
    // 1. 验证
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

    // 2. 业务逻辑
    try
    {
        // ... 处理 ...
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

## 输入验证

### FluentValidation 模式

**文件**：`Application/Validators/GenerateScaffoldValidator.cs`

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

### UseCase 中的验证

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

## 全局异常中间件

### 模式：内联中间件

**文件**：`Api/Program.cs`（第 46-58 行）

```csharp
// 异常处理中间件
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

## API 错误响应

### 成功响应

```json
{
  "success": true,
  "data": { ... }
}
```

或文件下载（带响应头的二进制响应）。

### 验证错误（400）

```json
{
  "error": "项目名称不能为空; 命名空间格式无效"
}
```

### 业务错误（400）

```json
{
  "error": "生成失败: 模板文件不存在"
}
```

### 服务器错误（500）

```json
{
  "error": "服务器内部错误"
}
```

---

## 基础设施异常模式

当外部依赖失败时，抛出描述性异常：

**文件**：`Infrastructure/Rendering/ScribanTemplateRenderer.cs`

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

    // ... 渲染 ...
}
```

---

## 常见错误

| 错误 | 正确做法 |
|------|----------|
| 使用异常控制流程 | 预期失败使用 Result Pattern |
| 静默吞掉异常 | 返回错误前始终记录日志 |
| 暴露内部错误详情 | 500 错误返回通用消息 |
| 缺少验证 | 在入口点（API/UseCase）验证 |
| 错误格式不一致 | 使用标准错误响应结构 |

---

## 检查清单

- [ ] 验证错误返回 400 和消息
- [ ] 业务错误返回 `Success = false` 的 Result
- [ ] 基础设施异常抛出并被中间件捕获
- [ ] 所有异常用 `Log.Error(ex, "message")` 记录
- [ ] 500 错误永不暴露内部详情
