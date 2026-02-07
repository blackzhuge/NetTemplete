# 后端开发规范

> 本项目后端开发的最佳实践。

---

## 技术栈

- **.NET 9** + Minimal API
- **模板引擎**：Scriban
- **验证**：FluentValidation
- **日志**：Serilog
- **测试**：xUnit + Moq + FluentAssertions

---

## 规范索引

| 规范 | 描述 |
|------|------|
| [目录结构](./directory-structure.md) | Clean Architecture 层级、Module Pattern |
| [错误处理](./error-handling.md) | Result Pattern、错误码分类、中间件 |
| [质量规范](./quality-guidelines.md) | 代码标准、禁止模式、DI |
| [日志规范](./logging-guidelines.md) | Serilog、结构化日志 |
| [测试规范](./test-guidelines.md) | xUnit、Mock、集成测试 |
