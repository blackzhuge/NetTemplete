# 质量规范

> 后端开发的代码质量标准。

---

## 概述

**框架**：.NET 9
**API 风格**：Minimal API
**测试**：xUnit
**验证**：FluentValidation

---

## 必需模式

### 1. 不可变 DTOs 使用 Records

```csharp
// 必需：DTOs 使用 record
public sealed record GenerateScaffoldRequest
{
    public required string ProjectName { get; init; }
    public required string Namespace { get; init; }
    public DatabaseProvider Database { get; init; } = DatabaseProvider.SQLite;
    public bool EnableSwagger { get; init; } = true;
}

// 必需：响应使用 record
public sealed record GenerationResult
{
    public bool Success { get; init; }
    public string FileName { get; init; } = string.Empty;
    public byte[] FileContent { get; init; } = [];
    public string? ErrorMessage { get; init; }
}
```

### 1.1 嵌套 DTO 结构（复杂请求）

**场景**：当请求包含多个逻辑分组时，使用嵌套 DTO 而非扁平结构。

```csharp
// ✅ 正确：嵌套结构，语义清晰
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

public sealed record BackendOptions
{
    public DatabaseProvider Database { get; init; } = DatabaseProvider.SQLite;
    public bool EnableSwagger { get; init; } = true;
}

// ❌ 错误：扁平结构，字段过多难维护
public sealed record GenerateScaffoldRequest
{
    public required string ProjectName { get; init; }
    public required string Namespace { get; init; }
    public DatabaseProvider Database { get; init; }
    public bool EnableSwagger { get; init; }
    public bool EnableFrontend { get; init; }
    public string FrontendFramework { get; init; }
    // ... 20+ 字段
}
```

**规则**：
- 超过 6 个字段考虑分组
- 分组名用业务语义（Basic/Backend/Frontend）
- 嵌套 DTO 也使用 `sealed record`

**关键特性**：
- `sealed` 修饰符（防止继承）
- `record` 类型（不可变性）
- `init` 属性（仅构造时可设置）
- `required` 用于必填字段
- `= []` 用于空集合（C# 12）

### 2. 构造函数注入

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

### 3. Async 配合 CancellationToken

```csharp
// 必需：所有异步方法接受 CancellationToken
public interface ITemplateRenderer
{
    Task<string> RenderAsync(string templatePath, object model, CancellationToken ct = default);
}

// 实现
public async Task<string> RenderAsync(string templatePath, object model, CancellationToken ct = default)
{
    var content = await _fileProvider.ReadTemplateAsync(templatePath, ct);
    // ...
}
```

### 4. Minimal API 端点

```csharp
app.MapPost("/api/v1/scaffolds/generate-zip", async (
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

### 5. RESTful 路由命名规范

| 规则 | 正确示例 | 错误示例 |
|------|----------|----------|
| 版本化 API | `/api/v1/...` | `/api/...` |
| 复数资源名 | `/scaffolds` | `/scaffold` |
| 动作后缀 | `/generate-zip` | `/generate` |
| kebab-case | `/generate-zip` | `/generateZip` |

```csharp
// ✅ 正确：版本化 + 资源复数 + 动作后缀
app.MapPost("/api/v1/scaffolds/generate-zip", ...)
app.MapGet("/api/v1/users/{id}", ...)
app.MapPost("/api/v1/orders/batch-create", ...)

// ❌ 错误
app.MapPost("/api/generate", ...)  // 无版本、无资源名
app.MapPost("/api/v1/scaffold/generateZip", ...)  // 单数、camelCase
```

### 6. JSON 序列化配置

**枚举必须配置字符串序列化**，否则前端收到数字而非字符串：

```csharp
// Program.cs
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
```

**常见错误**：
```json
// ❌ 无 JsonStringEnumConverter
{ "database": 0, "cache": 1 }

// ✅ 有 JsonStringEnumConverter
{ "database": "SQLite", "cache": "MemoryCache" }
```

---

## 命名约定

| 类型 | 约定 | 示例 |
|------|------|------|
| 接口 | `I` 前缀 + PascalCase | `ITemplateRenderer` |
| 类 | PascalCase | `ScribanTemplateRenderer` |
| 方法 | PascalCase + 异步加 `Async` 后缀 | `RenderAsync` |
| 参数 | camelCase | `templatePath`、`ct` |
| 私有字段 | `_` 前缀 + camelCase | `_templateRenderer` |
| 常量 | PascalCase | `MaxRetryCount` |
| 枚举 | PascalCase（单数） | `DatabaseProvider` |
| 枚举值 | PascalCase | `SQLite`、`MySQL` |

---

## DI 注册模式

**文件**：`Api/Program.cs`

```csharp
// 按 接口 -> 实现 注册
builder.Services.AddScoped<IValidator<GenerateScaffoldRequest>, GenerateScaffoldValidator>();
builder.Services.AddScoped<IZipBuilder, SystemZipBuilder>();
builder.Services.AddScoped<ITemplateRenderer, ScribanTemplateRenderer>();

// 需要配置的服务使用工厂
builder.Services.AddScoped<ITemplateFileProvider>(_ =>
    new FileSystemTemplateProvider(Path.Combine(Directory.GetCurrentDirectory(), "templates")));

// UseCase（具体类）
builder.Services.AddScoped<GenerateScaffoldUseCase>();
```

### 服务生命周期

| 生命周期 | 何时使用 |
|----------|----------|
| `Scoped` | 大多数服务的默认值，每请求一个 |
| `Singleton` | 无状态工具、配置、缓存 |
| `Transient` | 轻量级、无状态、创建成本低 |

---

## 禁止模式

### 1. 可变 DTOs

```csharp
// 禁止
public class UserDto
{
    public string Name { get; set; }  // 可变！
}

// 必需
public sealed record UserDto
{
    public required string Name { get; init; }
}
```

### 2. 静态类处理业务逻辑

```csharp
// 禁止
public static class UserService
{
    public static User GetUser(int id) { ... }
}

// 必需：使用 DI
public sealed class UserService : IUserService
{
    public User GetUser(int id) { ... }
}
```

### 3. 缺少 sealed 修饰符

```csharp
// 禁止（允许意外继承）
public class UserRepository { }

// 必需
public sealed class UserRepository { }
```

### 4. Async 不带 CancellationToken

```csharp
// 禁止
public async Task<string> ProcessAsync()

// 必需
public async Task<string> ProcessAsync(CancellationToken ct = default)
```

### 5. Service Locator 模式

```csharp
// 禁止
var service = serviceProvider.GetService<IUserService>();

// 必需：构造函数注入
public MyClass(IUserService userService)
```

### 6. 硬编码绝对路径

```csharp
// 禁止 - 只在特定机器工作
builder.Services.AddScoped<ITemplateFileProvider>(_ =>
    new FileSystemTemplateProvider("/Users/developer/project/templates"));

// 禁止 - Directory.GetCurrentDirectory() 随执行上下文变化
builder.Services.AddScoped<ITemplateFileProvider>(_ =>
    new FileSystemTemplateProvider(Path.Combine(Directory.GetCurrentDirectory(), "templates")));

// 必需 - 使用相对路径解析
public static string FindProjectPath(string relativePath)
{
    var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (dir != null)
    {
        var targetPath = Path.Combine(dir.FullName, relativePath);
        if (Directory.Exists(targetPath) || File.Exists(targetPath))
            return targetPath;
        dir = dir.Parent;
    }
    throw new InvalidOperationException($"Could not find: {relativePath}");
}
```

**原因**：
- 绝对路径在其他机器上失效
- `Directory.GetCurrentDirectory()` 在测试和运行时返回不同值
- 向上搜索确保跨开发、测试和生产环境的可移植性

**常见陷阱（集成测试）**：

```csharp
// 在测试的 CustomWebApplicationFactory 中
protected override void ConfigureWebHost(IWebHostBuilder builder)
{
    builder.ConfigureServices(services =>
    {
        // 移除运行时注册
        var descriptor = services.SingleOrDefault(
            d => d.ServiceType == typeof(ITemplateFileProvider));
        if (descriptor != null)
            services.Remove(descriptor);

        // 使用测试友好的路径解析重新注册
        var templatesPath = FindTemplatesPath(); // 使用相对搜索
        services.AddScoped<ITemplateFileProvider>(_ =>
            new FileSystemTemplateProvider(templatesPath));
    });
}
```

---

## 测试要求

### 框架

```xml
<PackageVersion Include="xunit" Version="2.6.4" />
```

### 测试结构（待完成）

```
tests/
├── ScaffoldGenerator.Application.Tests/
│   └── UseCases/
│       └── GenerateScaffoldUseCaseTests.cs
└── ScaffoldGenerator.Infrastructure.Tests/
    └── Rendering/
        └── ScribanTemplateRendererTests.cs
```

### 测试命名

```csharp
[Fact]
public async Task ExecuteAsync_WithValidRequest_ReturnsSuccessResult()

[Fact]
public async Task ExecuteAsync_WithInvalidProjectName_ReturnsValidationError()
```

**格式**：`{Method}_{Scenario}_{ExpectedResult}`

---

## 代码审查清单

### 提交前

- [ ] 所有 DTOs 使用 `sealed record` 和 `init` 属性
- [ ] 所有异步方法接受 `CancellationToken`
- [ ] 所有类都是 `sealed`（除非需要继承）
- [ ] 业务逻辑无 `static` 类
- [ ] 依赖通过构造函数注入
- [ ] 验证使用 FluentValidation
- [ ] 错误处理遵循 Result Pattern

### 审查者清单

- [ ] 命名遵循约定
- [ ] 未使用禁止模式
- [ ] 遵守 Clean Architecture 层级
- [ ] 无循环依赖
- [ ] 正确的错误处理
- [ ] 适当的日志记录

---

## 构建命令

```bash
# 恢复包
dotnet restore src/apps/api/ScaffoldGenerator.Api/ScaffoldGenerator.Api.csproj

# 构建
dotnet build src/apps/api/ScaffoldGenerator.Api/ScaffoldGenerator.Api.csproj

# 运行
dotnet run --project src/apps/api/ScaffoldGenerator.Api/ScaffoldGenerator.Api.csproj

# 测试（当测试存在时）
dotnet test
```

---

## 包管理

通过 `Directory.Packages.props` 集中管理：

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <!-- ORM -->
    <PackageVersion Include="SqlSugarCore" Version="5.1.4.169" />
    <!-- 日志 -->
    <PackageVersion Include="Serilog.AspNetCore" Version="9.0.0" />
    <!-- 验证 -->
    <PackageVersion Include="FluentValidation" Version="11.9.0" />
    <!-- 等等 -->
  </ItemGroup>
</Project>
```

**规则**：新包添加到 `Directory.Packages.props`，不要添加到单独的 `.csproj` 文件。
