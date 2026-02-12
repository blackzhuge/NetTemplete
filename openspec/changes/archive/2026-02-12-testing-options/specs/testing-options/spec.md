## ADDED Requirements

### Requirement: Backend unit test framework selection

用户 SHALL 能够从 BackendUnitTestFramework 枚举中选择后端单元测试框架（None / xUnit / NUnit / MSTest）。选择非 None 值时，系统 MUST 生成对应的单元测试项目骨架，包含 .csproj 文件和目录结构。固定包含 Moq 和 FluentAssertions 依赖。

#### Scenario: Select xUnit for backend unit tests

- **WHEN** 用户选择 BackendUnitTestFramework = xUnit
- **THEN** 系统生成 `tests/{ProjectName}.Api.UnitTests/{ProjectName}.Api.UnitTests.csproj`，包含 xUnit、Moq、FluentAssertions NuGet 引用

#### Scenario: Select NUnit for backend unit tests

- **WHEN** 用户选择 BackendUnitTestFramework = NUnit
- **THEN** 系统生成 .csproj 包含 NUnit、NUnit3TestAdapter、Moq、FluentAssertions NuGet 引用

#### Scenario: Select MSTest for backend unit tests

- **WHEN** 用户选择 BackendUnitTestFramework = MSTest
- **THEN** 系统生成 .csproj 包含 MSTest.TestFramework、MSTest.TestAdapter、Moq、FluentAssertions NuGet 引用

#### Scenario: Backend unit test disabled

- **WHEN** 用户选择 BackendUnitTestFramework = None
- **THEN** 系统不生成任何后端单元测试相关文件

#### Scenario: Clean Architecture project references

- **WHEN** BackendUnitTestFramework != None 且 ArchitectureStyle = CleanArchitecture
- **THEN** .csproj MUST 包含对 {ProjectName}.Api、{ProjectName}.Application、{ProjectName}.Domain 的 ProjectReference

#### Scenario: Default architecture project references

- **WHEN** BackendUnitTestFramework != None 且 ArchitectureStyle != CleanArchitecture
- **THEN** .csproj MUST 仅包含对 {ProjectName}.Api 的 ProjectReference

### Requirement: Backend integration test framework selection

用户 SHALL 能够从 BackendIntegrationTestFramework 枚举中选择后端集成测试框架（None / xUnit）。选择非 None 值时，系统 MUST 生成集成测试项目骨架，包含 WebApplicationFactory 依赖。

#### Scenario: Select xUnit for backend integration tests

- **WHEN** 用户选择 BackendIntegrationTestFramework = xUnit
- **THEN** 系统生成 `tests/{ProjectName}.Api.IntegrationTests/{ProjectName}.Api.IntegrationTests.csproj`，包含 xUnit、Microsoft.AspNetCore.Mvc.Testing、FluentAssertions NuGet 引用

#### Scenario: Backend integration test disabled

- **WHEN** 用户选择 BackendIntegrationTestFramework = None
- **THEN** 系统不生成任何后端集成测试相关文件

#### Scenario: Integration test without unit test

- **WHEN** BackendIntegrationTestFramework = xUnit 且 BackendUnitTestFramework = None
- **THEN** 系统 MUST 正常生成集成测试项目（两者独立，无依赖关系）

### Requirement: Frontend unit test framework selection

用户 SHALL 能够从 FrontendUnitTestFramework 枚举中选择前端单元测试框架（None / Vitest）。选择非 None 值时，系统 MUST 生成 vitest 配置文件和测试目录结构，并添加 @vue/test-utils npm devDependency。

#### Scenario: Select Vitest for frontend unit tests

- **WHEN** 用户选择 FrontendUnitTestFramework = Vitest
- **THEN** 系统生成 `vitest.config.ts`，添加 vitest、@vue/test-utils 到 devDependencies，创建 `tests/unit/` 目录

#### Scenario: Frontend unit test disabled

- **WHEN** 用户选择 FrontendUnitTestFramework = None
- **THEN** 系统不生成任何前端单元测试相关文件和依赖

### Requirement: Frontend E2E test framework selection

用户 SHALL 能够从 FrontendE2EFramework 枚举中选择前端 E2E 测试框架（None / Playwright / Cypress）。选择非 None 值时，系统 MUST 生成对应的 E2E 配置文件和测试目录结构。

#### Scenario: Select Playwright for frontend E2E

- **WHEN** 用户选择 FrontendE2EFramework = Playwright
- **THEN** 系统生成 `playwright.config.ts`，添加 @playwright/test 到 devDependencies，创建 `tests/e2e/` 目录

#### Scenario: Select Cypress for frontend E2E

- **WHEN** 用户选择 FrontendE2EFramework = Cypress
- **THEN** 系统生成 `cypress.config.ts`，添加 cypress 到 devDependencies，创建 `cypress/` 目录

#### Scenario: Frontend E2E disabled

- **WHEN** 用户选择 FrontendE2EFramework = None
- **THEN** 系统不生成任何前端 E2E 测试相关文件和依赖

### Requirement: Solution file conditional inclusion

当任何后端测试框架选择非 None 时，.sln 文件 MUST 包含对应的测试项目条目。

#### Scenario: SLN includes unit test project

- **WHEN** BackendUnitTestFramework != None
- **THEN** Solution.sln MUST 包含 {ProjectName}.Api.UnitTests 项目条目和构建配置

#### Scenario: SLN includes integration test project

- **WHEN** BackendIntegrationTestFramework != None
- **THEN** Solution.sln MUST 包含 {ProjectName}.Api.IntegrationTests 项目条目和构建配置

#### Scenario: SLN with no tests

- **WHEN** BackendUnitTestFramework = None 且 BackendIntegrationTestFramework = None
- **THEN** Solution.sln MUST 不包含任何测试项目条目

### Requirement: All tests disabled generates zero artifacts

当所有 4 个测试枚举均为 None 时，系统 MUST 不生成任何测试相关文件、目录或依赖。

#### Scenario: All testing options set to None

- **WHEN** 所有 4 个测试枚举 = None
- **THEN** 生成的项目与未添加测试选项前完全一致（零差异）

## PBT Properties

### Property: Idempotency of None selection

**不变式**: 所有测试枚举设为 None 时，生成结果与不存在测试选项字段时完全一致。
**伪造策略**: 对比有/无 TestingOptions 字段的 GenerateScaffoldRequest 生成结果，文件列表和内容 MUST 完全相同。

### Property: Independence of test options

**不变式**: 4 个测试选项互相独立，任意组合均有效。
**伪造策略**: 枚举所有 4x2x2x3 = 48 种组合，每种组合 MUST 成功生成且不抛异常。

### Property: Enum-package mapping consistency

**不变式**: 每个非 None 枚举值 MUST 映射到确定的包集合，不受其他选项影响。
**伪造策略**: 固定一个枚举值，变化其他 3 个枚举，验证该枚举对应的包集合不变。
