# Testing Options Phase 4: 预设 + 验证 + 收尾

## Goal
更新预设方案、验证规则，编写端到端集成测试。

## Requirements
- 更新 BuiltInPresets.cs：Minimal(无测试), Standard(xUnit+Vitest), Enterprise(全部)
- 确认 FluentValidation 枚举验证
- 编写预设单元测试
- 编写完整流程集成测试

## Acceptance Criteria
- [ ] 预设 API 返回正确的测试选项
- [ ] 无效枚举值请求返回 400
- [ ] Enterprise 预设生成完整测试项目
- [ ] Minimal 预设不生成测试项目
- [ ] 所有测试通过

## Technical Notes
参考 openspec/changes/testing-options/ 下的 design.md 和 specs/
