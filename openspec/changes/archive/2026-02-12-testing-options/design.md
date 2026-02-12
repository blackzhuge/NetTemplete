## Context

脚手架生成器当前支持后端（ORM、数据库、缓存、JWT、Swagger）和前端（UI 库、路由模式）的配置选项，但不支持测试基础设施配置。用户生成项目后需手动搭建测试项目。

现有架构：
- `GenerateScaffoldRequest` 包含嵌套 record：`BasicOptions`、`BackendOptions`、`FrontendOptions`
- `IScaffoldModule` 模式：`IsEnabled(request)` + `ContributeAsync(plan, request)`
- 枚举定义在 `Contracts/Enums/`，前后端通过 PascalCase 字符串对齐
- `Solution.sln.sbn` 模板当前为静态（仅包含 Api 项目）
- 前端 Zod schema 使用 `z.enum()` 与 C# 枚举值一一对应

## Goals / Non-Goals

**Goals:**
- 支持 4 个独立的测试框架选择（后端单元/集成、前端单元/E2E）
- 测试选项嵌套在 BackendOptions / FrontendOptions 中
- 生成空项目骨架（配置文件 + 目录结构），不含示例测试代码
- 预设方案按复杂度递增分配测试选项
- .sln 模板条件包含测试项目

**Non-Goals:**
- 不支持 Jest（Vite ESM 兼容性复杂，后续迭代）
- 不支持 Mock/断言框架选择（固定 Moq + FluentAssertions）
- 不支持 Directory.Packages.props 中央版本管理
- 不生成示例测试代码
- 不重构 ScaffoldPlan 为 SolutionProject 模型

## Decisions

### D1: 测试选项嵌套在 BackendOptions / FrontendOptions

**选择**: 在 `BackendOptions` 中添加 `UnitTestFramework` 和 `IntegrationTestFramework`；在 `FrontendOptions` 中添加 `UnitTestFramework` 和 `E2EFramework`。

**理由**: 匹配现有 request 三层嵌套结构（basic/backend/frontend），保持领域归属清晰。Codex 分析也推荐此方案。

**替代方案**: 顶层 `TestingOptions` record — 被否决，因为打破现有嵌套模式。

### D2: 枚举设计（None = 关闭）

**选择**: 4 个独立枚举，`None` 值表示关闭。

| 枚举 | 值 |
|------|-----|
| `BackendUnitTestFramework` | None, xUnit, NUnit, MSTest |
| `BackendIntegrationTestFramework` | None, xUnit |
| `FrontendUnitTestFramework` | None, Vitest |
| `FrontendE2EFramework` | None, Playwright, Cypress |

**理由**: 枚举值同时承载"是否启用"和"选择哪个框架"，避免 bool + enum 双字段。匹配 `CacheProvider.None` 的现有模式。

### D3: .sln 条件模板

**选择**: 在 `Solution.sln.sbn` 中使用 Scriban `if` 条件块，按枚举值 != None 条件包含测试项目。

**理由**: 最小改动，匹配当前模板风格。SolutionProject 模型虽更优雅但超出本次范围。

### D4: 项目引用按架构动态调整

**选择**:
- 默认架构：测试项目引用 `{Project}.Api`
- Clean Architecture：额外引用 `{Project}.Application` 和 `{Project}.Domain`

**理由**: Clean Architecture 的业务逻辑在 Application/Domain 层，单元测试需要直接引用这些项目。

### D5: 模块执行顺序

| Module | Order |
|--------|-------|
| BackendUnitTestModule | 60 |
| BackendIntegrationTestModule | 65 |
| FrontendUnitTestModule | 90 |
| FrontendE2EModule | 95 |
| FrontendModule (existing) | 100 |

**理由**: 后端测试在 Swagger(40) 之后、前端(100) 之前。前端测试模块在 FrontendModule 之前，确保 npm devDependencies 在 package.json 渲染前注册。

### D6: 模板组织

```
templates/
├── backend/tests/
│   ├── unit/
│   │   ├── xunit/UnitTests.csproj.sbn
│   │   ├── nunit/UnitTests.csproj.sbn
│   │   └── mstest/UnitTests.csproj.sbn
│   └── integration/
│       └── xunit/IntegrationTests.csproj.sbn
└── frontend/tests/
    ├── unit/
    │   └── vitest/vitest.config.ts.sbn
    └── e2e/
        ├── playwright/playwright.config.ts.sbn
        └── cypress/cypress.config.ts.sbn
```

### D7: 前端 UI 设计

测试选项以 `el-select` 下拉框形式分别嵌入 BackendOptions 和 FrontendOptions 组件中，使用"测试配置"分组标题。匹配现有 inline selector 模式。

### D8: 预设分配

| 预设 | 后端单元 | 后端集成 | 前端单元 | 前端 E2E |
|------|---------|---------|---------|---------|
| Minimal | None | None | None | None |
| Standard | xUnit | None | Vitest | None |
| Enterprise | xUnit | xUnit | Vitest | Playwright |

## Risks / Trade-offs

| 风险 | 缓解措施 |
|------|---------|
| .sln 条件模板随项目类型增多变复杂 | 本次仅加测试项目，后续可重构为 SolutionProject 模型 |
| 测试项目 ProjectReference 路径依赖输出目录约定 | 模板中使用相对路径，与 CoreModule 输出路径保持一致 |
| NuGet/npm 包版本硬编码在模板中 | 集中定义在 Module 常量中，模板通过 model 变量引用 |
| 集成测试依赖 `public partial class Program` | 在 Program.cs 模板中确保此声明存在 |
| FrontendUnitTestFramework 仅 Vitest 一个选项 | 枚举预留扩展性，后续可加 Jest |
