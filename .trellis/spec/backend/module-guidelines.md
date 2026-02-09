# 模块开发规范

> IScaffoldModule 及相关模块的开发规范。

---

## 概述

模块是 Scaffold Generator 的核心扩展机制。每个模块负责向生成计划贡献特定功能的文件和配置。

---

## IScaffoldModule 接口

```csharp
public interface IScaffoldModule
{
    string Name { get; }              // 模块唯一标识
    int Order { get; }                // 执行顺序（越小越先）
    bool IsEnabled(GenerateScaffoldRequest request);
    Task ContributeAsync(ScaffoldPlan plan, GenerateScaffoldRequest request, CancellationToken ct);
}
```

---

## 模块实现规范

### 1. 基本结构

```csharp
public sealed class MyFeatureModule : IScaffoldModule
{
    public string Name => "MyFeature";
    public int Order => 50; // 根据依赖关系选择合适的顺序

    public bool IsEnabled(GenerateScaffoldRequest request)
    {
        // 返回 true 表示此模块应该参与生成
        return request.Backend.EnableMyFeature;
    }

    public Task ContributeAsync(
        ScaffoldPlan plan,
        GenerateScaffoldRequest request,
        CancellationToken ct)
    {
        var model = new { /* 模板变量 */ };

        plan.AddTemplateFile(
            templatePath: "backend/my-feature/Setup.cs.sbn",
            outputPath: $"src/{request.Basic.ProjectName}.Api/MyFeature/Setup.cs",
            model: model
        );

        return Task.CompletedTask;
    }
}
```

### 2. Order 排序规则

| Order 范围 | 类别 | 说明 |
|-----------|------|------|
| 0-9 | 核心 | CoreModule，基础框架文件 |
| 10-29 | 架构 | ArchitectureModule，目录结构 |
| 30-49 | 数据层 | OrmModule, DatabaseModule |
| 50-79 | 功能层 | Cache, Auth, Swagger 等 |
| 80-99 | 集成层 | 第三方服务集成 |
| 100+ | 前端 | FrontendModule |

### 3. 依赖管理

```csharp
// ✅ 推荐：使用 Order 确保依赖顺序
public int Order => 50; // 确保在 CoreModule(0) 之后

// ✅ 推荐：检查前置条件
public bool IsEnabled(GenerateScaffoldRequest request)
{
    // 确保依赖的选项已启用
    return request.Backend.EnableMyFeature
        && request.Backend.Database != DatabaseProvider.None;
}
```

---

## 模块元数据（扩展接口）

对于复杂模块，可实现元数据接口：

```csharp
public interface IScaffoldModuleMetadata
{
    /// <summary>依赖的模块名称</summary>
    string[] DependsOn { get; }

    /// <summary>冲突的模块名称</summary>
    string[] ConflictsWith { get; }

    /// <summary>支持的架构模式</summary>
    ArchitectureStyle[] SupportedArchitectures { get; }

    /// <summary>支持的 ORM</summary>
    OrmProvider[] SupportedOrms { get; }
}
```

**实现示例**：

```csharp
public sealed class EFCoreModule : IScaffoldModule, IScaffoldModuleMetadata
{
    public string Name => "EFCore";
    public int Order => 35;

    public string[] DependsOn => ["Core", "Architecture"];
    public string[] ConflictsWith => ["SqlSugar", "Dapper", "FreeSql"];
    public ArchitectureStyle[] SupportedArchitectures =>
        [ArchitectureStyle.Simple, ArchitectureStyle.CleanArchitecture];
    public OrmProvider[] SupportedOrms => [OrmProvider.EFCore];

    // ...
}
```

---

## 路径冲突检测

### 规则

1. **禁止重复输出路径**：多个模块不得写入同一文件
2. **冲突检测时机**：ScaffoldPlanBuilder.BuildAsync 阶段
3. **冲突处理**：抛出 InvalidCombinationException

### 实现

```csharp
// ScaffoldPlanBuilder 在 Build 阶段检测
var duplicates = plan.Files
    .GroupBy(f => f.OutputPath)
    .Where(g => g.Count() > 1)
    .ToList();

if (duplicates.Any())
{
    var conflicts = duplicates.Select(g =>
        $"{g.Key}: [{string.Join(", ", g.Select(f => f.SourceModule))}]");
    throw new InvalidCombinationException(
        $"路径冲突: {string.Join("; ", conflicts)}");
}
```

---

## 包管理规范

### NuGet 包

```csharp
// ✅ 正确：通过 plan 添加包
plan.AddNugetPackage(new PackageReference("Dapper", "2.1.35"));

// ❌ 错误：在模板中硬编码包
// 模板应从 model.NugetPackages 读取
```

### 包版本冲突

- 同一包不同版本视为冲突
- 冲突时返回 422 错误
- 建议：统一在 `Directory.Build.props` 管理版本

---

## 模板文件规范

### 目录结构

```
templates/
├── backend/
│   ├── core/           # CoreModule 模板
│   ├── architecture/   # ArchitectureModule 模板
│   │   ├── simple/
│   │   ├── clean/
│   │   ├── vertical-slice/
│   │   └── modular-monolith/
│   └── orm/            # OrmModule 模板
│       ├── sqlsugar/
│       ├── efcore/
│       ├── dapper/
│       └── freesql/
└── frontend/
    └── ui/             # UI 库模板
```

### 模板命名

| 约定 | 示例 |
|------|------|
| 输出文件名 + `.sbn` | `Program.cs.sbn` |
| 小写连字符目录 | `vertical-slice/` |

---

## 测试规范

### 单元测试

```csharp
[Fact]
public async Task ContributeAsync_WhenEnabled_AddsExpectedFiles()
{
    // Arrange
    var module = new MyFeatureModule();
    var plan = new ScaffoldPlan();
    var request = CreateRequest(enableMyFeature: true);

    // Act
    await module.ContributeAsync(plan, request, CancellationToken.None);

    // Assert
    plan.Files.Should().ContainSingle(f =>
        f.OutputPath.EndsWith("Setup.cs"));
}

[Fact]
public void IsEnabled_WhenFeatureDisabled_ReturnsFalse()
{
    var module = new MyFeatureModule();
    var request = CreateRequest(enableMyFeature: false);

    module.IsEnabled(request).Should().BeFalse();
}
```

### 集成测试

- 验证生成的项目可编译
- 验证无路径冲突
- 验证包依赖完整

---

## 常见错误

| 错误 | 正确做法 |
|------|----------|
| 模板中硬编码包版本 | 通过 `plan.AddNugetPackage` 添加 |
| 多模块写同一文件 | 使用不同输出路径或合并模板 |
| Order 相同导致顺序不确定 | 确保 Order 唯一 |
| IsEnabled 中有副作用 | 保持纯函数 |
| 忽略 CancellationToken | 传递给所有异步操作 |

---

## 新增模块检查清单

- [ ] 实现 IScaffoldModule 四个成员
- [ ] 选择合适的 Order 值
- [ ] 模板文件放在正确目录
- [ ] 输出路径不与其他模块冲突
- [ ] NuGet 包通过 plan 添加
- [ ] 单元测试覆盖 IsEnabled 和 ContributeAsync
- [ ] 在 Program.cs 注册 DI
