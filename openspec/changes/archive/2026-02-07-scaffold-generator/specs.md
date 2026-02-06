# Scaffold Generator - Specifications

## Overview
可配置的全栈脚手架生成器，用户通过 Web 配置器选择选项后下载 ZIP 包。

---

## Constraints (Final)

| Category | Constraint | Value |
|----------|------------|-------|
| 配置器技术栈 | Vue 3 + .NET API | 与脚手架一致 |
| Monorepo | .NET + pnpm 混合 | Directory.Build.props + pnpm workspaces |
| 输出形式 | ZIP 下载 | 配置后即时下载 |
| UI 库 | Element Plus | 前端统一 |
| 权限 | 简单 JWT | 无 RBAC |
| 多租户 | 不实现 | - |
| UI 布局 | 分屏布局 | 左配置 + 右预览 |
| 模板维护 | Live Source | 可运行项目转模板 |
| 生成范围 | 全栈 | 前端 + 后端 |

---

## Tech Stack

### Backend (Generator API)
- .NET 8 (Minimal API)
- Scriban (Template Engine)
- FluentValidation (Input Validation)
- System.IO.Compression (ZIP)
- Serilog (Logging)

### Frontend (Configurator)
- Vue 3 + Vite + TypeScript
- Pinia (State)
- Element Plus (UI)
- Axios (HTTP)
- VeeValidate + Zod (Form Validation)

### Generated Scaffold
- Backend: .NET 8 + SqlSugar + Serilog + JWT
- Frontend: Vue 3 + Vite + TypeScript + Pinia + Element Plus

---

## API Contract

### POST /api/v1/scaffolds/generate-zip

**Request Body:**
```json
{
  "basic": {
    "projectName": "string (^[a-zA-Z][a-zA-Z0-9]*$)",
    "namespace": "string"
  },
  "backend": {
    "database": "SQLite | MySQL | SQLServer",
    "cache": "None | MemoryCache | Redis",
    "swagger": "boolean",
    "jwtAuth": "boolean"
  },
  "frontend": {
    "routerMode": "hash | history",
    "mockData": "boolean"
  }
}
```

**Response:** `application/zip`

**Error Codes:**
- 400: Invalid input
- 422: Invalid option combination
- 500: Template error

---

## Configurable Options

| Category | Option | Type | Default | Choices |
|----------|--------|------|---------|---------|
| Basic | projectName | string | MyApp | User input |
| Basic | namespace | string | MyApp | User input |
| Backend | database | enum | SQLite | SQLite/MySQL/SQLServer |
| Backend | cache | enum | None | None/MemoryCache/Redis |
| Backend | swagger | bool | true | true/false |
| Backend | jwtAuth | bool | true | true/false |
| Frontend | routerMode | enum | hash | hash/history |
| Frontend | mockData | bool | false | true/false |

---

## Success Criteria

- [ ] Web 配置器可正常访问和使用
- [ ] 分屏布局：左侧配置表单 + 右侧文件树预览
- [ ] 所有配置项可正确切换
- [ ] 点击生成后下载 ZIP 文件
- [ ] ZIP 解压后前后端项目可直接运行
- [ ] 数据库选项正确影响 SqlSugar 配置
- [ ] 缓存选项正确注入对应实现
- [ ] JWT 选项正确添加认证中间件
- [ ] 前端路由模式正确配置
