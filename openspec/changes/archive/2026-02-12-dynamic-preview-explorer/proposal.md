# Dynamic Preview Explorer

## Context

用户在 web-configurator 中选择配置后，预览面板（Explorer）的文件树是前端硬编码的，无法反映：
- 架构风格差异（Simple / CleanArchitecture / VerticalSlice / ModularMonolith）
- ORM 选择差异（SqlSugar / EFCore / Dapper / FreeSql）
- UI 库选择差异（ElementPlus / NaiveUI / ShadcnVue 等）
- 用户选择的 NuGet/npm 包

当前状态：
- 前端 `config.ts` 中 `fileTree` computed 属性是硬编码的静态结构
- 后端 `ScaffoldPlan` 已有完整的动态文件生成逻辑
- 两者不一致，导致"预览"与"生成"结果不同

## Requirements

### R1: 后端提供文件树 API
**场景**：用户打开预览面板时，前端请求后端获取基于当前配置的完整文件树
**验收**：
- [ ] 新增 `POST /api/scaffold/preview/tree` 端点
- [ ] 返回 `FileTreeNode[]` 结构，与前端类型兼容
- [ ] 响应时间 < 200ms

### R2: 前端调用后端获取文件树
**场景**：用户切换任意配置选项时，文件树自动更新
**验收**：
- [ ] 废弃前端硬编码的 `fileTree` computed
- [ ] 添加 `fetchFileTree()` action 调用后端 API
- [ ] 配置变化时防抖调用（300ms）

### R3: 文件树反映架构差异
**场景**：选择 CleanArchitecture 时显示 Domain/Application/Infrastructure 层
**验收**：
- [ ] Simple: Api + Web 两层
- [ ] CleanArchitecture: Api + Domain + Application + Infrastructure
- [ ] VerticalSlice: Api + Features 目录
- [ ] ModularMonolith: Api + Modules 目录

### R4: 文件树反映 ORM 差异
**场景**：选择 FreeSql 时显示 FreeSqlSetup.cs，选择 EFCore 时显示 DbContext
**验收**：
- [ ] SqlSugar: SqlSugarSetup.cs
- [ ] EFCore: AppDbContext.cs + Migrations/
- [ ] Dapper: DapperSetup.cs
- [ ] FreeSql: FreeSqlSetup.cs

### R5: 文件树反映 UI 库差异
**场景**：选择 ShadcnVue 时显示 tailwind.config.js
**验收**：
- [ ] ElementPlus: 无额外配置文件
- [ ] ShadcnVue/TailwindHeadless: tailwind.config.js + postcss.config.js
- [ ] NaiveUI: 无额外配置文件

### R6: 预览包信息
**场景**：选择 NuGet 包后，预览 .csproj 时能看到 PackageReference
**验收**：
- [ ] .csproj 预览包含用户选择的 NuGet 包
- [ ] package.json 预览包含用户选择的 npm 包
- [ ] 包列表与生成结果一致

## Dependencies

- 后端 `ScaffoldPlan` 已有 `Files`、`NugetPackages`、`NpmPackages`
- 后端 `IScaffoldModule` 系统已实现动态文件生成
- 前端 `FileTreeNode` 类型已定义

## Risks

| 风险 | 影响 | 缓解 |
|------|------|------|
| 频繁 API 调用 | 性能下降 | 防抖 + 缓存 |
| 后端计算开销 | 响应变慢 | 仅计算文件列表，不渲染内容 |
| 类型不兼容 | 前端解析失败 | 后端返回符合 FileTreeNode 的结构 |

## Decisions

1. **缓存策略**：前端 computed 自动缓存，无需额外处理
2. **错误处理**：不做特殊处理，API 失败时保持当前状态
