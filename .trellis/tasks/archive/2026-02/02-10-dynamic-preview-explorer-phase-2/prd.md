# Task: 执行 OpenSpec Change Phase

## Change
openspec/changes/dynamic-preview-explorer

## Phase
2

## Phase 标题
后端树构建逻辑

## 说明
执行上述 change 的 Phase 2 任务。

按照 ccg-impl 工作流：
1. 读取 openspec/changes/dynamic-preview-explorer/tasks.md 中 Phase 2 的任务列表
2. 读取 openspec/changes/dynamic-preview-explorer/specs.md 和 design.md 理解规范和设计
3. 多模型协作实现（后端用 Codex，前端用 Gemini）
4. 外部模型只返回 diff patch，Claude 重写为生产代码
5. 完成后更新 tasks.md 标记 [x]
