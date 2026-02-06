# Scaffold Generator

## Context
创建一个可配置的全栈开发脚手架生成器，用户通过 Web 界面配置选项后下载 ZIP 包。

## Constraints (已确认)
- **配置器技术栈**: Vue 3 + .NET API
- **Monorepo 工具**: .NET (Directory.Build.props) + pnpm workspaces
- **输出形式**: ZIP 下载
- **前端 UI 库**: Element Plus
- **权限模型**: 简单 JWT (无 RBAC)
- **多租户**: 不实现

## Core Tech Stack
### Backend
- .NET 8/9 (ASP.NET Core Minimal API)
- SqlSugar (ORM)
- Serilog (Logging)

### Frontend
- Vue 3 (Composition API)
- Vite
- TypeScript
- Pinia
- Element Plus

## Configurable Options
| Category | Option | Default | Choices |
|----------|--------|---------|---------|
| Basic | Project Name | MyApp | User input |
| Basic | Namespace | MyApp | User input |
| Backend | Database | SQLite | SQLite/MySQL/SQLServer |
| Backend | Cache | None | None/MemoryCache/Redis |
| Backend | Swagger | true | true/false |
| Backend | JWT Auth | true | true/false |
| Frontend | Router Mode | Hash | Hash/History |
| Frontend | Mock Data | false | true/false |

## Success Criteria
- [ ] Web 配置器可正常运行
- [ ] 用户可选择配置项并预览
- [ ] 生成的 ZIP 包解压后可直接运行
- [ ] 生成的项目结构符合 Monorepo 规范
- [ ] 所有可选功能模块可正确启用/禁用
