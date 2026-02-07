# 质量规范

> 后端开发的代码质量标准。

---

## 必需模式

### 1. 不可变 DTOs 使用 Records

```csharp
public sealed record GenerateScaffoldRequest
{
    public required BasicOptions Basic { get; init; }
    public BackendOptions Backend { get; init; } = new();
    public FrontendOptions Frontend { get; init; } = new();
}

public sealed record BasicOptions
{
    public required string ProjectName { get; init; }
    public required string Namespace { get; init; }
}
```

**关键特性**：`sealed` + `record` + `init` + `required`

---

### 2. 构造函数注入

```csharp
public sealed class GenerateScaffoldUseCase(
    ScaffoldPlanBuilder planBuilder,
    IZipBuilder zipBuilder,
    IValidator<GenerateScaffoldRequest> validator)
{
    public async Task<GenerationResult> ExecuteAsync(
        GenerateScaffoldRequest request,
        CancellationToken ct = default)
    {
        // ...
    }
}
```

---

### 3. Async 配合 CancellationToken

```csharp
public interface ITemplateRenderer
{
    Task<string> RenderAsync(string templatePath, object model, CancellationToken ct = default);
}
```

---

### 4. Minimal API 端点

```csharp
// Program.cs
app.MapPresetsEndpoints();
app.MapPreviewEndpoints();
app.MapPackagesEndpoints();

// Endpoints/PresetsEndpoints.cs
public static class PresetsEndpoints
{
    public static void MapPresetsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/presets");
        group.MapGet("/", async (IPresetService service) =>
            Results.Ok(await service.GetAllAsync()));
    }
}
```

---

### 5. RESTful 路由规范

| 规则 | 正确 | 错误 |
|------|------|------|
| 版本化 | `/api/v1/...` | `/api/...` |
| 复数资源 | `/scaffolds` | `/scaffold` |
| kebab-case | `/generate-zip` | `/generateZip` |

---

### 6. JSON 枚举字符串化

```csharp
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
```

---

## 命名约定

| 类型 | 约定 | 示例 |
|------|------|------|
| 接口 | `I` 前缀 | `ITemplateRenderer` |
| 方法 | 异步加 `Async` | `RenderAsync` |
| 私有字段 | `_` 前缀 | `_templateRenderer` |
| 枚举 | PascalCase 单数 | `DatabaseProvider` |

---

## DI 注册

```csharp
// 按 接口 -> 实现 注册
builder.Services.AddScoped<IValidator<GenerateScaffoldRequest>, GenerateScaffoldValidator>();
builder.Services.AddScoped<IZipBuilder, SystemZipBuilder>();

// 模块批量注册
builder.Services.AddScoped<IScaffoldModule, CoreModule>();
builder.Services.AddScoped<IScaffoldModule, DatabaseModule>();
// ...

// UseCase（具体类）
builder.Services.AddScoped<GenerateScaffoldUseCase>();
```

| 生命周期 | 使用场景 |
|----------|----------|
| `Scoped` | 大多数服务（默认） |
| `Singleton` | 无状态工具、缓存 |
| `Transient` | 轻量级无状态 |

---

## 禁止模式

| 模式 | 正确做法 |
|------|----------|
| 可变 DTO（`set`） | 使用 `init` |
| 静态类业务逻辑 | 使用 DI |
| 缺少 `sealed` | 所有类加 `sealed` |
| Async 无 `CancellationToken` | 始终传递 `ct` |
| Service Locator | 构造函数注入 |
| 硬编码绝对路径 | 相对路径搜索 |

---

## 代码审查清单

- [ ] DTOs 使用 `sealed record` + `init`
- [ ] 异步方法接受 `CancellationToken`
- [ ] 所有类都是 `sealed`
- [ ] 依赖通过构造函数注入
- [ ] 验证使用 FluentValidation
- [ ] 错误处理遵循 Result Pattern
