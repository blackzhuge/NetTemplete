# Testing Options Phase 2: 后端 Modules + Templates

## Goal
实现 4 个 IScaffoldModule 和对应的 Scriban 模板，修改 .sln 模板支持条件包含。

## Requirements
- 创建 BackendUnitTestModule (Order=60), BackendIntegrationTestModule (Order=65)
- 创建 FrontendUnitTestModule (Order=90), FrontendE2EModule (Order=95)
- 创建 7 个 Scriban 模板文件（.csproj + 前端配置）
- 修改 Solution.sln.sbn 添加条件块
- 注册模块到 Program.cs DI
- 编写模块单元测试和集成测试

## Acceptance Criteria
- [ ] 4 个模块编译通过，IsEnabled 逻辑正确
- [ ] 模板渲染无错误
- [ ] .sln 条件包含测试项目正确
- [ ] 模块单元测试和集成测试全部通过

## Technical Notes
参考 openspec/changes/testing-options/ 下的 design.md 和 specs/
