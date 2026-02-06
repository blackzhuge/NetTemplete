# Scaffold Generator

快速生成 .NET + Vue 全栈项目脚手架的工具。

## 功能特性

- **可视化配置**: Web 界面配置项目选项
- **全栈生成**: 同时生成后端 (.NET 9) 和前端 (Vue 3) 代码
- **模块化**: 按需选择数据库、缓存、认证等模块
- **即开即用**: 生成的项目可直接运行

## 技术栈

| 层级 | 技术 |
|------|------|
| 后端 | .NET 9, Minimal API, SqlSugar, Serilog |
| 前端 | Vue 3, TypeScript, Element Plus, Vite |
| 模板引擎 | Scriban |

## 快速开始

### 环境要求

- .NET 9 SDK
- Node.js 20+
- pnpm 9+

### 启动开发服务器

```bash
# 启动后端 API
cd src/apps/api/ScaffoldGenerator.Api
dotnet run

# 启动前端配置器（另一个终端）
cd src/apps/web-configurator
pnpm install
pnpm dev
```

### 访问配置器

打开浏览器访问 `http://localhost:5173`

## 文档

- [配置器使用说明](./user-guide.md) - 如何使用 Web 配置器生成项目
- [模板扩展指南](./template-guide.md) - 如何添加或修改模板
- [开发者文档](./developer-guide.md) - 项目架构和开发指南

## 项目结构

```
src/
├── apps/
│   ├── api/                    # 后端 API
│   │   ├── ScaffoldGenerator.Api/           # API 层
│   │   ├── ScaffoldGenerator.Application/   # 应用层
│   │   ├── ScaffoldGenerator.Contracts/     # 契约层
│   │   └── ScaffoldGenerator.Infrastructure/ # 基础设施层
│   ├── web-configurator/       # 前端配置器
│   └── template-frontend/      # 前端模板源
├── packages/
│   └── @scaffold/
│       └── shared-types/       # 共享类型定义
└── templates/                  # 模板文件
    ├── backend/
    └── frontend/
```

## License

MIT
