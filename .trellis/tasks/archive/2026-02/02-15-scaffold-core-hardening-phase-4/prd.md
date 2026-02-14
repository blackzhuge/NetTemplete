# Task: 执行 OpenSpec Change Phase

## Change
openspec/changes/scaffold-core-hardening

## Phase
4

## Phase 标题
回归验证

## 说明
执行上述 change 的 Phase 4 任务。

按照 ccg-impl 工作流：
1. 读取 openspec/changes/scaffold-core-hardening/tasks.md 中 Phase 4 的任务列表
2. 读取 openspec/changes/scaffold-core-hardening/specs.md 和 design.md 理解规范和设计
3. 多模型协作实现（后端用 Codex，前端用 Gemini）
4. 外部模型只返回 diff patch，Claude 重写为生产代码
5. 完成后更新 tasks.md 标记 [x]
