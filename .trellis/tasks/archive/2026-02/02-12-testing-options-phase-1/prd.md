# Testing Options Phase 1: 后端 Contracts

## Goal
创建测试框架枚举和扩展 GenerateScaffoldRequest DTO。

## Requirements
- 创建 4 个枚举：BackendUnitTestFramework, BackendIntegrationTestFramework, FrontendUnitTestFramework, FrontendE2EFramework
- 扩展 BackendOptions 和 FrontendOptions 添加测试框架字段
- 编写枚举序列化单元测试

## Acceptance Criteria
- [ ] 4 个枚举文件编译通过
- [ ] GenerateScaffoldRequest 包含新字段，默认值 None
- [ ] JSON 序列化/反序列化测试通过
- [ ] 向后兼容（缺失字段反序列化为 None）

## Technical Notes
参考 openspec/changes/testing-options/ 下的 design.md 和 specs/
