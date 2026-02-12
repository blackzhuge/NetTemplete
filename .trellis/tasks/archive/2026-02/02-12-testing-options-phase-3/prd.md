# Testing Options Phase 3: 前端 Schema + Store + UI

## Goal
前端添加测试选项的类型定义、Zod schema、Store 扩展、API 映射和 UI 组件。

## Requirements
- 新增 4 个 TypeScript union type
- 扩展 Zod schema 添加 4 个 z.enum() 字段
- 扩展 Pinia store 添加扁平字段
- 更新 toApiRequest() 映射
- BackendOptions.vue 和 FrontendOptions.vue 添加测试选择器
- 编写前端单元测试

## Acceptance Criteria
- [ ] TypeScript 编译通过
- [ ] Zod schema 验证正确
- [ ] UI 下拉框渲染正确，值同步到 store
- [ ] API 请求 DTO 结构正确
- [ ] Vitest 测试全部通过

## Technical Notes
参考 openspec/changes/testing-options/ 下的 design.md 和 specs/
