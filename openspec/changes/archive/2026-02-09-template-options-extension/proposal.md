# Template Options Extension

## 概述

扩展 Scaffold Generator 的可选项，增加架构模式、ORM 选择、UI 库选择，并实现预设模版库系统。

## 约束集合

### 硬约束（不可违反）

| 约束 | 原因 |
|------|------|
| 保持 `IScaffoldModule` 接口签名 | 现有 6 个模块依赖此接口 |
| 保持 `GenerateScaffoldRequest` 结构兼容 | API 契约，前端已适配 |
| 模板文件使用 `.sbn` 扩展名 | Scriban 引擎约定 |
| `manifest.json` 控制条件渲染 | 现有机制 |

### 软约束（建议遵循）

| 约束 | 建议 |
|------|------|
| 模块命名 | `{Feature}Module.cs` |
| 模板位置 | `templates/{layer}/{feature}/` |
| 枚举定义 | 放在 `Contracts/Enums/` |
| 预设文件 | JSON Schema 可验证 |

## 功能范围

### 1. 架构模式扩展

| 架构 | 目录结构 | 适用场景 |
|------|----------|----------|
| Simple (当前) | `src/ProjectName.Api/` | 小型项目、快速原型 |
| Clean Architecture | `Api → Application → Domain → Infrastructure` | 中大型项目 |
| Vertical Slice | `src/Features/{Feature}/` | CQRS、独立部署 |
| Modular Monolith | `src/Modules/{Module}/` | 未来拆微服务 |

### 2. ORM 扩展

| ORM | 特点 | 新增模板 |
|-----|------|----------|
| SqlSugar (已有) | 国产、API 友好 | 保持 |
| EF Core | 官方、生态完善 | DbContext.cs.sbn, EFCoreSetup.cs.sbn |
| Dapper | 轻量、SQL 优先 | DapperSetup.cs.sbn |
| FreeSql | 国产、功能全面 | FreeSqlSetup.cs.sbn |

### 3. UI 库扩展

| UI 库 | 特点 | 新增模板 |
|-------|------|----------|
| Element Plus (已有) | 企业级 | 保持 |
| Ant Design Vue | 阿里设计语言 | main.ts.sbn 变体 |
| Naive UI | TS 优先、高性能 | main.ts.sbn 变体 |
| Tailwind + Headless UI | 原子化 CSS | tailwind.config.js.sbn |
| shadcn-vue | Radix 移植 | components.json.sbn |
| MateChat | 华为 AI 对话 | matechat-setup.ts.sbn |

### 4. 预设模版库

```
presets/
├── schema.json              # JSON Schema
├── builtin/
│   ├── enterprise.json      # Clean + EFCore + ElementPlus
│   ├── startup.json         # Simple + SqlSugar + NaiveUI
│   └── ai-ready.json        # Simple + SqlSugar + MateChat
└── custom/                   # 用户自定义
```

## 成功判据

| 判据 | 验证方式 |
|------|----------|
| 4 种架构模式可选 | 配置器 UI 可选择，生成对应目录结构 |
| 4 种 ORM 可选 | 生成对应 Setup 文件，项目可编译 |
| 6 种 UI 库可选 | 生成对应 main.ts，前端可启动 |
| 预设保存/加载 | API 支持 CRUD，配置器可操作 |
| 向后兼容 | 现有配置仍可正常生成 |

## 开放问题

| 问题 | 状态 |
|------|------|
| 架构模式选择 | ✅ 已确认：全部 4 种 |
| ORM 选择 | ✅ 已确认：SqlSugar + EFCore + Dapper + FreeSql |
| UI 库选择 | ✅ 已确认：6 种全部支持 |
| 配置存储 | ✅ 已确认：预设模版库 |
| MateChat 集成 | ✅ 已调研：@matechat/core + vue-devui |

## 技术决策

| 决策项 | 选择 | 理由 |
|--------|------|------|
| 预设存储位置 | 文件系统 `presets/` | 简单、可版本控制 |
| 预设格式 | JSON + Schema | 可验证、IDE 支持 |
| 架构模块实现 | 单一 ArchitectureModule | 避免模块爆炸 |
| UI 库模块实现 | 单一 UiLibraryModule | 统一管理 |
