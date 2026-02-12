## MODIFIED Requirements

### Requirement: GenerateScaffoldRequest structure

`GenerateScaffoldRequest` MUST 在 `BackendOptions` 中包含 `UnitTestFramework`（BackendUnitTestFramework 枚举）和 `IntegrationTestFramework`（BackendIntegrationTestFramework 枚举）字段，默认值均为 None。在 `FrontendOptions` 中包含 `UnitTestFramework`（FrontendUnitTestFramework 枚举）和 `E2EFramework`（FrontendE2EFramework 枚举）字段，默认值均为 None。

#### Scenario: Request with testing options

- **WHEN** API 接收包含测试选项的 JSON 请求
- **THEN** 系统 MUST 正确反序列化枚举字符串值到对应枚举类型

#### Scenario: Request without testing options (backward compatibility)

- **WHEN** API 接收不包含测试选项字段的 JSON 请求
- **THEN** 系统 MUST 使用 None 作为所有测试枚举的默认值

### Requirement: Module registration

Program.cs MUST 注册 4 个新的 IScaffoldModule 实现：BackendUnitTestModule、BackendIntegrationTestModule、FrontendUnitTestModule、FrontendE2EModule。

#### Scenario: All test modules registered

- **WHEN** 应用启动
- **THEN** DI 容器 MUST 包含 4 个测试相关的 IScaffoldModule 注册
