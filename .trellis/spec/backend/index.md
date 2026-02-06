# 后端开发规范

> 本项目后端开发的最佳实践。

---

## 概述

本目录包含后端开发规范。每个文件记录了项目的具体约定。

---

## 规范索引

| 规范 | 描述 | 状态 |
|------|------|------|
| [目录结构](./directory-structure.md) | Clean Architecture 层级、命名约定 | 完成 |
| [数据库规范](./database-guidelines.md) | SqlSugar ORM、Repository 模式、查询 | 完成 |
| [错误处理](./error-handling.md) | Result Pattern、FluentValidation、中间件 | 完成 |
| [质量规范](./quality-guidelines.md) | 代码标准、禁止模式、DI | 完成 |
| [日志规范](./logging-guidelines.md) | Serilog、结构化日志、日志级别 | 完成 |

---

## 如何填写这些规范

对于每个规范文件：

1. 记录项目的**实际约定**（而非理想状态）
2. 包含来自代码库的**代码示例**
3. 列出**禁止模式**及其原因
4. 添加团队曾经犯过的**常见错误**

目标是帮助 AI 助手和新团队成员理解你的项目是如何运作的。

**Language**: 所有文档使用 **中文**