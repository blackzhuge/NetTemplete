# Provider 模式规范

> 策略接口（Provider Pattern）的设计与实现规范。

---

## 概述

Provider 模式用于实现可插拔的策略选择，允许用户在多个实现中选择一个。本项目中用于 UI 库、ORM 等可选项的管理。

---

## 核心接口设计

### IUiLibraryProvider 示例

```csharp
public interface IUiLibraryProvider
{
    /// <summary>对应的 UI 库枚举值</summary>
    UiLibrary Library { get; }

    /// <summary>获取该 UI 库需要的 npm 包</summary>
    IEnumerable<PackageReference> GetNpmPackages();

    /// <summary>获取 main.ts 模板路径</summary>
    string GetMainTsTemplatePath();

    /// <summary>获取额外需要生成的模板（如 tailwind.config.js）</summary>
    IEnumerable<TemplateMapping> GetAdditionalTemplates();
}

public record TemplateMapping(string TemplatePath, string OutputPath);
```

---

## 实现规范

### 1. Provider 实现

```csharp
public sealed class ElementPlusProvider : IUiLibraryProvider
{
    public UiLibrary Library => UiLibrary.ElementPlus;

    public IEnumerable<PackageReference> GetNpmPackages()
    {
        yield return new PackageReference("element-plus", "^2.5.0");
        yield return new PackageReference("@element-plus/icons-vue", "^2.3.0");
    }

    public string GetMainTsTemplatePath()
        => "frontend/ui/element-plus/main.ts.sbn";

    public IEnumerable<TemplateMapping> GetAdditionalTemplates()
    {
        // Element Plus 不需要额外配置文件
        yield break;
    }
}
```

### 2. 复杂 Provider 示例

```csharp
public sealed class TailwindProvider : IUiLibraryProvider
{
    public UiLibrary Library => UiLibrary.TailwindHeadless;

    public IEnumerable<PackageReference> GetNpmPackages()
    {
        yield return new PackageReference("tailwindcss", "^3.4.0");
        yield return new PackageReference("postcss", "^8.4.0");
        yield return new PackageReference("autoprefixer", "^10.4.0");
        yield return new PackageReference("@headlessui/vue", "^1.7.0");
    }

    public string GetMainTsTemplatePath()
        => "frontend/ui/tailwind/main.ts.sbn";

    public IEnumerable<TemplateMapping> GetAdditionalTemplates()
    {
        yield return new("frontend/ui/tailwind/tailwind.config.js.sbn",
            "tailwind.config.js");
        yield return new("frontend/ui/tailwind/postcss.config.js.sbn",
            "postcss.config.js");
    }
}
```

---

## 注册与选择

### DI 注册

```csharp
// Program.cs
builder.Services.AddSingleton<IUiLibraryProvider, ElementPlusProvider>();
builder.Services.AddSingleton<IUiLibraryProvider, AntDesignProvider>();
builder.Services.AddSingleton<IUiLibraryProvider, NaiveUiProvider>();
builder.Services.AddSingleton<IUiLibraryProvider, TailwindProvider>();
builder.Services.AddSingleton<IUiLibraryProvider, ShadcnVueProvider>();
builder.Services.AddSingleton<IUiLibraryProvider, MateChatProvider>();
```

### 选择机制

```csharp
public sealed class FrontendModule : IScaffoldModule
{
    private readonly IEnumerable<IUiLibraryProvider> _providers;

    public FrontendModule(IEnumerable<IUiLibraryProvider> providers)
    {
        _providers = providers;
    }

    public Task ContributeAsync(ScaffoldPlan plan, GenerateScaffoldRequest request, CancellationToken ct)
    {
        var provider = _providers.FirstOrDefault(p =>
            p.Library == request.Frontend.UiLibrary)
            ?? throw new InvalidOperationException(
                $"不支持的 UI 库: {request.Frontend.UiLibrary}");

        // 使用 provider 获取模板和依赖
        foreach (var pkg in provider.GetNpmPackages())
        {
            plan.AddNpmPackage(pkg);
        }

        plan.AddTemplateFile(
            provider.GetMainTsTemplatePath(),
            "src/main.ts",
            model);

        foreach (var template in provider.GetAdditionalTemplates())
        {
            plan.AddTemplateFile(template.TemplatePath, template.OutputPath, model);
        }

        return Task.CompletedTask;
    }
}
```

---

## 接口设计原则

### 1. 单一职责

每个 Provider 只负责一种选项的配置。

```csharp
// ✅ 正确：职责单一
public interface IUiLibraryProvider { ... }
public interface IOrmProvider { ... }

// ❌ 错误：职责混合
public interface IAllOptionsProvider { ... }
```

### 2. 无副作用

Provider 方法应为纯函数，不修改外部状态。

```csharp
// ✅ 正确：返回数据，不修改状态
public IEnumerable<PackageReference> GetNpmPackages() => [
    new("vue", "^3.4.0")
];

// ❌ 错误：直接修改计划
public void AddPackages(ScaffoldPlan plan) { ... }
```

### 3. 完整性

Provider 应返回该选项所需的所有配置，不遗漏。

```csharp
// ✅ 正确：返回完整依赖
public IEnumerable<PackageReference> GetNpmPackages()
{
    yield return new("tailwindcss", "^3.4.0");
    yield return new("postcss", "^8.4.0");      // 不要遗漏
    yield return new("autoprefixer", "^10.4.0"); // 不要遗漏
}
```

---

## 目录结构

```
Application/
├── Abstractions/
│   ├── IUiLibraryProvider.cs    # 接口定义
│   └── IOrmProvider.cs
└── Providers/
    ├── UiLibrary/               # UI 库 Provider
    │   ├── ElementPlusProvider.cs
    │   ├── AntDesignProvider.cs
    │   ├── NaiveUiProvider.cs
    │   ├── TailwindProvider.cs
    │   ├── ShadcnVueProvider.cs
    │   └── MateChatProvider.cs
    └── Orm/                      # ORM Provider
        ├── SqlSugarProvider.cs
        ├── EFCoreProvider.cs
        ├── DapperProvider.cs
        └── FreeSqlProvider.cs
```

---

## 测试规范

### 单元测试

```csharp
public class ElementPlusProviderTests
{
    [Fact]
    public void GetNpmPackages_ReturnsElementPlusPackages()
    {
        var provider = new ElementPlusProvider();

        var packages = provider.GetNpmPackages().ToList();

        packages.Should().Contain(p => p.Name == "element-plus");
    }

    [Fact]
    public void GetMainTsTemplatePath_ReturnsValidPath()
    {
        var provider = new ElementPlusProvider();

        var path = provider.GetMainTsTemplatePath();

        path.Should().EndWith(".sbn");
        path.Should().Contain("element-plus");
    }
}
```

### 集成测试

```csharp
[Theory]
[InlineData(UiLibrary.ElementPlus)]
[InlineData(UiLibrary.AntDesignVue)]
[InlineData(UiLibrary.NaiveUI)]
public async Task GenerateWithUiLibrary_ProducesValidProject(UiLibrary library)
{
    var request = CreateRequest(uiLibrary: library);

    var result = await _useCase.ExecuteAsync(request);

    result.Success.Should().BeTrue();
    // 验证 npm install 成功
}
```

---

## 新增 Provider 检查清单

- [ ] 创建 Provider 类实现接口
- [ ] 返回完整的包依赖列表
- [ ] 创建对应的模板文件
- [ ] 在 Program.cs 注册 DI
- [ ] 单元测试覆盖所有方法
- [ ] 集成测试验证生成项目可运行
- [ ] 更新相关文档
