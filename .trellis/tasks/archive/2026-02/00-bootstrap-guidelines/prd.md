# 初始化：填写项目开发规范

## 目的

欢迎使用 Trellis！这是你的第一个任务。

AI 助手使用 `.trellis/spec/` 来理解你项目的编码约定。
**空模板 = AI 编写的代码不符合你的项目风格。**

填写这些规范是一次性的设置工作，但会在每个未来的 AI 会话中受益。

---

## 你的任务

根据你的**现有代码库**填写规范文件。

### 后端规范

| 文件 | 需要记录的内容 |
|------|---------------|
| `.trellis/spec/backend/directory-structure.md` | 不同文件类型放在哪里（routes、services、utils） |
| `.trellis/spec/backend/database-guidelines.md` | ORM、迁移、查询模式、命名约定 |
| `.trellis/spec/backend/error-handling.md` | 错误如何被捕获、记录和返回 |
| `.trellis/spec/backend/logging-guidelines.md` | 日志级别、格式、记录什么 |
| `.trellis/spec/backend/quality-guidelines.md` | 代码审查标准、测试要求 |

### 前端规范

| 文件 | 需要记录的内容 |
|------|---------------|
| `.trellis/spec/frontend/directory-structure.md` | 组件/页面/hook 组织 |
| `.trellis/spec/frontend/component-guidelines.md` | 组件模式、props 约定 |
| `.trellis/spec/frontend/hook-guidelines.md` | 自定义 hook 命名、模式 |
| `.trellis/spec/frontend/state-management.md` | 状态库、模式、什么放在哪里 |
| `.trellis/spec/frontend/type-safety.md` | TypeScript 约定、类型组织 |
| `.trellis/spec/frontend/quality-guidelines.md` | Linting、测试、可访问性 |

### 思维指南（可选）

`.trellis/spec/guides/` 目录包含已填写通用最佳实践的思维指南。
如有需要，你可以根据项目进行自定义。

---

## 如何填写规范

### 原则：记录现实，而非理想

写下你的代码库**实际做的事情**，而不是你希望它做的事情。
AI 需要匹配现有模式，而不是引入新模式。

### 步骤

1. **查看现有代码** - 找到每个模式的 2-3 个示例
2. **记录模式** - 描述你看到的内容
3. **包含文件路径** - 引用真实文件作为示例
4. **列出反模式** - 你的团队避免什么？

---

## 使用 AI 的技巧

让 AI 帮助分析你的代码库：

- "查看我的代码库并记录你看到的模式"
- "分析我的代码结构并总结约定"
- "找到错误处理模式并记录它们"

AI 会读取你的代码并帮助你记录它。

---

## 完成检查清单

- [ ] 为你的项目类型填写了规范
- [ ] 每个规范中至少有 2-3 个真实代码示例
- [ ] 记录了反模式

完成后：

```bash
./.trellis/scripts/task.sh finish
./.trellis/scripts/task.sh archive 00-bootstrap-guidelines
```

---

## 为什么这很重要

完成此任务后：

1. AI 将编写符合你项目风格的代码
2. 相关的 `/trellis:before-*-dev` 命令将注入真实上下文
3. `/trellis:check-*` 命令将根据你的实际标准进行验证
4. 未来的开发者（人类或 AI）将更快地上手

