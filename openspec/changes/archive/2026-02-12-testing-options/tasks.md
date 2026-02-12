# Testing Options - 实施任务

## Phase 1: 后端 Contracts（枚举 + DTO）

### 1.1 创建测试框架枚举

- [x] **文件**: `src/apps/api/ScaffoldGenerator.Contracts/Enums/BackendUnitTestFramework.cs`
- [x] **文件**: `src/apps/api/ScaffoldGenerator.Contracts/Enums/BackendIntegrationTestFramework.cs`
- [x] **文件**: `src/apps/api/ScaffoldGenerator.Contracts/Enums/FrontendUnitTestFramework.cs`
- [x] **文件**: `src/apps/api/ScaffoldGenerator.Contracts/Enums/FrontendE2EFramework.cs`

**实现要点**:
- 每个枚举第一个值为 None（默认值）
- PascalCase 命名，与前端 Zod schema 对齐
- BackendUnitTestFramework: None, xUnit, NUnit, MSTest
- BackendIntegrationTestFramework: None, xUnit
- FrontendUnitTestFramework: None, Vitest
- FrontendE2EFramework: None, Playwright, Cypress

**验收**: 枚举文件编译通过，命名空间为 ScaffoldGenerator.Contracts.Enums

### 1.2 扩展 GenerateScaffoldRequest

- [x] **文件**: `src/apps/api/ScaffoldGenerator.Contracts/Requests/GenerateScaffoldRequest.cs`

**实现要点**:
- BackendOptions 增加 UnitTestFramework 和 IntegrationTestFramework 字段
- FrontendOptions 增加 UnitTestFramework 和 E2EFramework 字段
- 所有字段默认值 None，加 [JsonConverter(typeof(JsonStringEnumConverter))]
- 保持向后兼容（缺失字段反序列化为 None）

**验收**: 编译通过，JSON 反序列化测试通过

### 1.3 单元测试 - 枚举序列化

- [x] **文件**: `src/tests/ScaffoldGenerator.Tests/Contracts/TestingEnumSerializationTests.cs`

**实现要点**:
- 测试 4 个新枚举的 JSON 字符串序列化/反序列化
- 测试缺失字段反序列化为 None（向后兼容）
- 测试无效值反序列化行为

**验收**: xUnit 测试全部通过

## Phase 2: 后端 Modules + Templates

### 2.1 创建 BackendUnitTestModule

- [x] **文件**: `src/apps/api/ScaffoldGenerator.Application/Modules/BackendUnitTestModule.cs`

**实现要点**:
- 实现 IScaffoldModule，Order = 60
- IsEnabled: request.Backend.UnitTestFramework != None
- ContributeAsync: 根据枚举值选择模板路径（xunit/nunit/mstest）
- 添加对应 NuGet 包（测试框架 + Moq + FluentAssertions）
- Clean Architecture 时额外添加 Application/Domain 项目引用到模板 model

**验收**: 模块编译通过，IsEnabled 逻辑正确

### 2.2 创建 BackendIntegrationTestModule

- [x] **文件**: `src/apps/api/ScaffoldGenerator.Application/Modules/BackendIntegrationTestModule.cs`

**实现要点**:
- 实现 IScaffoldModule，Order = 65
- IsEnabled: request.Backend.IntegrationTestFramework != None
- ContributeAsync: 生成集成测试 .csproj，包含 WebApplicationFactory 依赖
- 添加 Microsoft.AspNetCore.Mvc.Testing + FluentAssertions NuGet 包

**验收**: 模块编译通过

### 2.3 创建 FrontendUnitTestModule

- [x] **文件**: `src/apps/api/ScaffoldGenerator.Application/Modules/FrontendUnitTestModule.cs`

**实现要点**:
- 实现 IScaffoldModule，Order = 90
- IsEnabled: request.Frontend.UnitTestFramework != None
- ContributeAsync: 生成 vitest.config.ts，添加 vitest + @vue/test-utils npm devDependencies

**验收**: 模块编译通过

### 2.4 创建 FrontendE2EModule

- [x] **文件**: `src/apps/api/ScaffoldGenerator.Application/Modules/FrontendE2EModule.cs`

**实现要点**:
- 实现 IScaffoldModule，Order = 95
- IsEnabled: request.Frontend.E2EFramework != None
- ContributeAsync: 根据枚举值选择 Playwright/Cypress 模板和 npm 包

**验收**: 模块编译通过

### 2.5 创建 Scriban 模板文件

- [x] **文件**: `templates/backend/tests/unit/xunit/UnitTests.csproj.sbn`
- [x] **文件**: `templates/backend/tests/unit/nunit/UnitTests.csproj.sbn`
- [x] **文件**: `templates/backend/tests/unit/mstest/UnitTests.csproj.sbn`
- [x] **文件**: `templates/backend/tests/integration/xunit/IntegrationTests.csproj.sbn`
- [x] **文件**: `templates/frontend/tests/unit/vitest/vitest.config.ts.sbn`
- [x] **文件**: `templates/frontend/tests/e2e/playwright/playwright.config.ts.sbn`
- [x] **文件**: `templates/frontend/tests/e2e/cypress/cypress.config.ts.sbn`

**实现要点**:
- .csproj 模板包含 ProjectReference（使用 model 变量控制路径）
- 前端模板包含基础配置（无示例测试代码）
- 使用 snake_case 变量名（Scriban 自动转换）

**验收**: 模板语法正确，Scriban 渲染无错误

### 2.6 修改 Solution.sln.sbn 模板

- [x] **文件**: 现有 `templates/backend/Solution.sln.sbn`

**实现要点**:
- 添加 Scriban if 条件块，按 enable_unit_tests / enable_integration_tests 包含测试项目
- 为测试项目分配独立 GUID
- 添加对应的 ProjectConfigurationPlatforms 条目

**验收**: 条件渲染正确，启用/禁用测试时 .sln 格式有效

### 2.7 注册模块到 DI

- [x] **文件**: `src/apps/api/ScaffoldGenerator.Api/Program.cs`

**实现要点**:
- 添加 4 行 AddScoped<IScaffoldModule, XxxModule>() 注册

**验收**: 应用启动无错误

### 2.8 单元测试 - 测试模块

- [x] **文件**: `src/tests/ScaffoldGenerator.Tests/Modules/BackendUnitTestModuleTests.cs`
- [x] **文件**: `src/tests/ScaffoldGenerator.Tests/Modules/BackendIntegrationTestModuleTests.cs`
- [x] **文件**: `src/tests/ScaffoldGenerator.Tests/Modules/FrontendUnitTestModuleTests.cs`
- [x] **文件**: `src/tests/ScaffoldGenerator.Tests/Modules/FrontendE2EModuleTests.cs`

**实现要点**:
- 测试 IsEnabled 对每个枚举值的返回值
- 测试 ContributeAsync 生成的文件列表和 NuGet/npm 包
- 测试 Clean Architecture 时的额外项目引用
- 测试 None 时不生成任何文件

**验收**: xUnit 测试全部通过

### 2.9 集成测试 - 端到端生成

- [x] **文件**: `src/tests/ScaffoldGenerator.Tests/Api/TestingOptionsEndpointTests.cs`

**实现要点**:
- 测试 POST /api/v1/scaffolds/preview-tree 包含测试选项时的文件树
- 测试所有 None 时文件树与无测试选项一致
- 测试各框架组合生成的文件路径正确

**验收**: 集成测试全部通过

## Phase 3: 前端（Schema + Store + UI）

### 3.1 新增 TypeScript 类型定义

- [x] **文件**: `src/apps/web-configurator/src/types/index.ts`

**实现要点**:
- 添加 4 个 union type：BackendUnitTestFramework、BackendIntegrationTestFramework、FrontendUnitTestFramework、FrontendE2EFramework
- 值与 C# 枚举 PascalCase 完全一致

**验收**: TypeScript 编译通过

### 3.2 扩展 Zod Schema

- [x] **文件**: `src/apps/web-configurator/src/schemas/config.ts`

**实现要点**:
- 添加 4 个 z.enum() 字段，默认值 'None'
- backendUnitTestFramework: z.enum(['None', 'xUnit', 'NUnit', 'MSTest'])
- backendIntegrationTestFramework: z.enum(['None', 'xUnit'])
- frontendUnitTestFramework: z.enum(['None', 'Vitest'])
- frontendE2EFramework: z.enum(['None', 'Playwright', 'Cypress'])

**验收**: Schema 验证通过，默认值正确

### 3.3 扩展 Pinia Store

- [x] **文件**: `src/apps/web-configurator/src/stores/config.ts`

**实现要点**:
- ScaffoldConfig 增加 4 个扁平字段，默认值 'None'
- updateConfig 方法支持新字段

**验收**: Store 初始化和更新正常

### 3.4 更新 API 映射

- [x] **文件**: `src/apps/web-configurator/src/api/generator.ts`

**实现要点**:
- toApiRequest() 将扁平字段映射到嵌套 DTO：
  - backend.unitTestFramework / backend.integrationTestFramework
  - frontend.unitTestFramework / frontend.e2eFramework

**验收**: API 请求 DTO 结构正确

### 3.5 BackendOptions 添加测试选择器

- [x] **文件**: `src/apps/web-configurator/src/components/BackendOptions.vue`

**实现要点**:
- 添加"测试配置"分组标题
- 2 个 el-select 下拉框：后端单元测试框架、后端集成测试框架
- 使用 useField() + watch 同步 store

**验收**: UI 渲染正确，选择值同步到 store

### 3.6 FrontendOptions 添加测试选择器

- [x] **文件**: `src/apps/web-configurator/src/components/FrontendOptions.vue`

**实现要点**:
- 添加"测试配置"分组标题
- 2 个 el-select 下拉框：前端单元测试框架、前端 E2E 框架
- 使用 useField() + watch 同步 store

**验收**: UI 渲染正确，选择值同步到 store

### 3.7 前端单元测试

- [x] **文件**: `src/apps/web-configurator/tests/schemas/config.spec.ts`（新建）
- [x] **文件**: `src/apps/web-configurator/tests/api/generator.spec.ts`（更新）
- [x] **文件**: `src/apps/web-configurator/tests/stores/config.spec.ts`（更新）

**实现要点**:
- 测试 Zod schema 对新枚举字段的验证
- 测试 toApiRequest() 映射正确性
- 测试 store 默认值和更新

**验收**: Vitest 测试全部通过

## Phase 4: 预设 + 验证 + 收尾

### 4.1 更新预设方案

- [x] **文件**: `src/apps/api/ScaffoldGenerator.Application/Presets/BuiltInPresets.cs`
- [x] **文件**: `presets/builtin/enterprise.json`
- [x] **文件**: `presets/schema.json`

**实现要点**:
- Minimal: 所有测试枚举 = None
- Standard: BackendUnitTestFramework = xUnit, FrontendUnitTestFramework = Vitest
- Enterprise: 全部启用（xUnit + xUnit + Vitest + Playwright）

**验收**: 预设 API 返回正确的测试选项

### 4.2 更新验证规则

- [x] **确认**: 枚举字段由 `JsonStringEnumConverter` 自动验证，无需额外 FluentValidation 规则

**实现要点**:
- 枚举字段为值类型，FluentValidation 自动验证
- 确认无需额外验证规则（枚举值由 JSON 反序列化保证）

**验收**: 无效枚举值请求返回 400

### 4.3 单元测试 - 预设

- [x] **文件**: `src/tests/ScaffoldGenerator.Tests/Application/PresetServiceTests.cs`（更新，添加 3 个预设测试框架字段验证）

**实现要点**:
- 测试 3 个预设的测试选项字段值
- 测试预设切换后测试选项正确更新

**验收**: xUnit 测试全部通过

### 4.4 集成测试 - 完整流程

- [x] **文件**: `src/tests/ScaffoldGenerator.Tests/Modules/BackendUnitTestModuleTests.cs`（新建）
- [x] **文件**: `src/tests/ScaffoldGenerator.Tests/Modules/FrontendE2EModuleTests.cs`（新建）

**实现要点**:
- 测试 Enterprise 预设生成的完整文件树包含所有测试项目
- 测试 Minimal 预设生成的文件树不包含测试项目
- 测试 .sln 文件内容包含/不包含测试项目条目

**验收**: 集成测试全部通过

## 进度统计

| Phase | 总任务 | 已完成 | 进度 |
|-------|--------|--------|------|
| Phase 1: 后端 Contracts | 3 | 3 | 100% |
| Phase 2: 后端 Modules + Templates | 9 | 9 | 100% |
| Phase 3: 前端 Schema + UI | 7 | 7 | 100% |
| Phase 4: 预设 + 验证 + 收尾 | 4 | 4 | 100% |
| **总计** | **23** | **23** | **100%** |
