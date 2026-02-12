# Testing Options

## Why

脚手架生成器目前不支持为生成的项目配置测试基础设施。用户需要手动搭建测试项目和配置，增加了项目启动成本。添加可选的测试支持（4 个独立选项，含框架选择）让用户可以按需选择测试类型和框架。

## What Changes

- 新增 `TestingOptions` record 到 `GenerateScaffoldRequest`，包含 4 个枚举字段（None = 关闭）：
  - `BackendUnitTestFramework` — 后端单元测试框架 (None / xUnit / NUnit / MSTest)
  - `BackendIntegrationTestFramework` — 后端集成测试框架 (None / xUnit)
  - `FrontendUnitTestFramework` — 前端单元测试框架 (None / Vitest / Jest)
  - `FrontendE2EFramework` — 前端 E2E 测试框架 (None / Playwright / Cypress)
- 新增 4 个枚举类型：
  - `BackendUnitTestFramework` (None, xUnit, NUnit, MSTest)
  - `BackendIntegrationTestFramework` (None, xUnit)
  - `FrontendUnitTestFramework` (None, Vitest, Jest)
  - `FrontendE2EFramework` (None, Playwright, Cypress)
- 固定搭配（不可选）：后端 Mock 固定 Moq，断言固定 FluentAssertions
- 新增对应的 IScaffoldModule 实现（按枚举值条件启用）
- 新增 Scriban 模板文件（仅空项目骨架，不含示例测试代码）
- 更新前端 Zod schema 和 UI 表单，添加测试选项区域（下拉选择框）
- 更新预设方案：Minimal 全 None → Standard 含 xUnit 单元测试 → Enterprise 全部启用

## Capabilities

### New Capabilities

- `testing-options`: 脚手架生成器的测试基础设施可选项，涵盖后端/前端的单元测试和集成/E2E 测试框架选择

### Modified Capabilities

- `scaffold-generator`: 扩展 GenerateScaffoldRequest 增加 TestingOptions，更新验证规则和预设方案
- `preset-preview`: 预设方案需包含测试选项字段

## Impact

- **后端 Contracts/Enums**: 新增 4 个测试框架枚举
- **后端 Contracts/Requests**: `GenerateScaffoldRequest` 增加 `TestingOptions` record
- **后端 Application/Modules**: 新增 4 个 IScaffoldModule 实现
- **后端 Api**: Program.cs 注册新模块
- **模板**: `templates/` 下新增测试项目 Scriban 模板
- **前端 Schema**: `config.ts` Zod schema 增加测试枚举字段
- **前端 UI**: 配置表单增加测试选项区域（4 个下拉选择框）
- **预设**: `BuiltInPresets.cs` 更新所有预设的测试配置
- **验证**: `GenerateScaffoldValidator` 更新（枚举值验证）
- **解决方案**: `.sln` 模板需条件包含测试项目
