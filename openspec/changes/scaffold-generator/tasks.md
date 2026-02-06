# Scaffold Generator - Implementation Tasks

## Phase 1: 项目初始化 (Day 1-2)

### 1.1 Monorepo 骨架
- [ ] 创建根目录结构
- [ ] 配置 `pnpm-workspace.yaml`
- [ ] 配置 `Directory.Build.props`
- [ ] 配置 `Directory.Packages.props`
- [ ] 创建 `.gitignore`

### 1.2 后端 Solution
- [ ] 创建 `scaffold-generator.sln`
- [ ] 创建 `ScaffoldGenerator.Api` 项目
- [ ] 创建 `ScaffoldGenerator.Contracts` 项目
- [ ] 创建 `ScaffoldGenerator.Application` 项目
- [ ] 创建 `ScaffoldGenerator.Infrastructure` 项目
- [ ] 配置项目引用关系

### 1.3 前端工作区
- [ ] 初始化 `apps/web-configurator` (Vue 3 + Vite)
- [ ] 初始化 `apps/template-frontend` (Vue 3 模板源)
- [ ] 初始化 `packages/@scaffold/shared-types`
- [ ] 配置 TypeScript 共享

---

## Phase 2: 后端核心 (Day 3-5)

### 2.1 契约层
- [ ] 定义 `GenerateScaffoldRequest` DTO
- [ ] 定义 `GenerationResult` 响应
- [ ] 定义枚举类型 (DatabaseProvider, CacheProvider, RouterMode)

### 2.2 应用层
- [ ] 实现 `ITemplateRenderer` 接口
- [ ] 实现 `IZipBuilder` 接口
- [ ] 实现 `GenerateScaffoldValidator` (FluentValidation)
- [ ] 实现 `GenerateScaffoldUseCase`

### 2.3 基础设施层
- [ ] 实现 `ScribanTemplateRenderer`
- [ ] 实现 `SystemZipBuilder`
- [ ] 实现 `TemplateFileProvider`

### 2.4 API 层
- [ ] 配置 Minimal API 端点
- [ ] 配置异常处理中间件
- [ ] 配置 Serilog
- [ ] 配置 CORS

---

## Phase 3: 模块系统 (Day 6-7)

### 3.1 模块接口
- [ ] 定义 `IScaffoldModule` 接口
- [ ] 实现 `ScaffoldPlanBuilder`
- [ ] 实现模块注册机制

### 3.2 功能模块
- [ ] 实现 `DatabaseModule` (SQLite/MySQL/SQLServer)
- [ ] 实现 `CacheModule` (None/Memory/Redis)
- [ ] 实现 `JwtModule`
- [ ] 实现 `SwaggerModule`

---

## Phase 4: 模板文件 (Day 8-10)

### 4.1 后端模板
- [ ] 创建 `templates/backend/manifest.json`
- [ ] 创建 `Program.cs.sbn` 模板
- [ ] 创建 `appsettings.json.sbn` 模板
- [ ] 创建 SqlSugar 配置模板
- [ ] 创建 JWT 模块模板 (条件化)
- [ ] 创建缓存模块模板 (条件化)

### 4.2 前端模板
- [ ] 完善 `apps/template-frontend` 项目
- [ ] 创建 `packages/@scaffold/template-utils` 转换脚本
- [ ] 生成 `templates/frontend/` 模板文件
- [ ] 创建 `templates/frontend/manifest.json`

---

## Phase 5: 配置器前端 (Day 11-14)

### 5.1 基础架构
- [ ] 配置 Element Plus
- [ ] 配置 Pinia Store
- [ ] 配置 Axios
- [ ] 配置 VeeValidate + Zod

### 5.2 组件开发
- [ ] 实现 `ConfigForm.vue` 容器
- [ ] 实现 `BasicOptions.vue`
- [ ] 实现 `BackendOptions.vue`
- [ ] 实现 `FrontendOptions.vue`
- [ ] 实现 `FileTreeView.vue`

### 5.3 业务逻辑
- [ ] 实现 `useConfig.ts` composable
- [ ] 实现 `useFileTree.ts` (客户端预览)
- [ ] 实现 `generator.ts` API 调用
- [ ] 实现 ZIP 下载逻辑

### 5.4 页面集成
- [ ] 实现 `HomePage.vue` 分屏布局
- [ ] 集成表单验证
- [ ] 集成加载状态
- [ ] 集成错误提示

---

## Phase 6: 集成测试 (Day 15-16)

### 6.1 后端测试
- [ ] 模板渲染单元测试
- [ ] ZIP 生成单元测试
- [ ] API 集成测试

### 6.2 端到端测试
- [ ] 配置器 UI 测试
- [ ] 生成 + 下载流程测试
- [ ] 生成项目可运行性验证

---

## Phase 7: 完善 (Day 17-18)

### 7.1 文档
- [ ] 配置器使用说明
- [ ] 模板扩展指南
- [ ] 开发者文档

### 7.2 优化
- [ ] 性能优化 (缓存模板)
- [ ] 错误信息优化
- [ ] UI 细节打磨

---

## Milestones

| Milestone | 交付物 | 预计完成 |
|-----------|--------|----------|
| M1 | 项目骨架可编译 | Day 2 |
| M2 | 后端 API 可返回空 ZIP | Day 5 |
| M3 | 模块化生成可工作 | Day 10 |
| M4 | 配置器 UI 可交互 | Day 14 |
| M5 | 端到端流程完整 | Day 16 |
| M6 | 生产就绪 | Day 18 |

---

## Dependencies

```
Phase 1 ──┬── Phase 2 ──┬── Phase 3
          │             │
          └── Phase 5 ──┴── Phase 4
                              │
                              ▼
                          Phase 6 ── Phase 7
```

Phase 2 和 Phase 5 可并行开发。
