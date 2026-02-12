## MODIFIED Requirements

### Requirement: Preset testing defaults

所有内置预设 MUST 包含测试选项字段，按复杂度递增分配。

#### Scenario: Minimal preset

- **WHEN** 用户选择 Minimal 预设
- **THEN** 所有 4 个测试枚举 MUST 为 None

#### Scenario: Standard preset

- **WHEN** 用户选择 Standard 预设
- **THEN** BackendUnitTestFramework = xUnit，FrontendUnitTestFramework = Vitest，其余为 None

#### Scenario: Enterprise preset

- **WHEN** 用户选择 Enterprise 预设
- **THEN** BackendUnitTestFramework = xUnit，BackendIntegrationTestFramework = xUnit，FrontendUnitTestFramework = Vitest，FrontendE2EFramework = Playwright
