# Scaffold Generator - 需求规格

## Purpose

可配置的全栈脚手架生成器，用户通过 Web 配置器选择选项后下载 ZIP 包。

## Constraints

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

## Requirements

### REQ-001: 生成 ZIP 包

**场景**: 用户提交配置后生成项目 ZIP

#### API: POST /api/v1/scaffolds/generate-zip

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

### REQ-002: 配置选项

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

### REQ-003: 动态预览文件树

**场景**: 配置变化时自动更新文件树预览

#### API: POST /api/scaffold/preview/tree

**Request Body:** 与 REQ-001 相同的 GenerateScaffoldRequest

**Response:**
```json
{
  "tree": [
    {
      "name": "src",
      "path": "src",
      "isDirectory": true,
      "children": [
        { "name": "MyApp.Api", "path": "src/MyApp.Api", "isDirectory": true, "children": [...] },
        { "name": "MyApp.Web", "path": "src/MyApp.Web", "isDirectory": true, "children": [...] }
      ]
    }
  ]
}
```

#### 场景 1.1: 正常获取文件树
**Given** 用户配置了 projectName="MyProject", architecture="Simple", orm="SqlSugar"
**When** 前端调用 POST /api/scaffold/preview/tree
**Then** 返回 200，包含与 ScaffoldPlan.Files 一致的树结构

**约束**:
- 响应时间 < 200ms
- 树结构与生成结果 100% 一致

#### 场景 1.2: 空配置
**Given** 请求体为空或缺少必填字段
**When** 调用 API
**Then** 返回 400

#### 场景 1.3: 无效配置组合
**Given** 请求包含不支持的配置组合
**When** 调用 API
**Then** 返回 422

### REQ-004: 前端自动更新文件树

#### 场景 2.1: 配置变化时自动更新
**Given** 用户在预览面板打开状态下
**When** 切换 architecture 从 Simple 到 CleanArchitecture
**Then** 300ms 后文件树自动更新，显示新架构的目录结构

**约束**:
- 防抖 300ms
- 更新期间显示 loading 状态

#### 场景 2.2: 选中文件在新树中不存在
**Given** 用户选中了 SqlSugarSetup.cs
**When** 切换 ORM 到 EFCore
**Then** 文件树更新后，选中状态清除，代码预览清空

### REQ-005: 文件树反映架构差异

| 架构 | 预期目录 |
|------|----------|
| Simple | src/{ProjectName}.Api, src/{ProjectName}.Web |
| CleanArchitecture | Api, Domain, Application, Infrastructure |
| VerticalSlice | Api/Features |
| ModularMonolith | Api/Modules |

### REQ-006: 文件树反映 ORM 差异

| ORM | 预期文件 |
|-----|----------|
| SqlSugar | SqlSugarSetup.cs |
| EFCore | AppDbContext.cs, Migrations/ |
| Dapper | DapperSetup.cs |
| FreeSql | FreeSqlSetup.cs |

### REQ-007: 文件树反映 UI 库差异

| UI 库 | 预期文件 |
|-------|----------|
| ElementPlus | 无额外配置 |
| ShadcnVue/TailwindHeadless | tailwind.config.js, postcss.config.js |
| NaiveUI | 无额外配置 |

### REQ-008: 预览包信息

#### 场景 6.1: NuGet 包显示
**Given** 用户添加了 NuGet 包 "AutoMapper@12.0.1"
**When** 预览 .csproj 文件
**Then** 内容包含 `<PackageReference Include="AutoMapper" Version="12.0.1" />`

#### 场景 6.2: npm 包显示
**Given** 用户添加了 npm 包 "dayjs@1.11.10"
**When** 预览 package.json 文件
**Then** 内容包含 `"dayjs": "1.11.10"`

## Property-Based Testing

### P1: 树一致性不变式
**属性**: 对于任意有效配置 C，PreviewTree(C) 的文件路径集合 = Generate(C) 的文件路径集合
**伪造策略**: 随机生成配置组合，对比预览树路径与生成结果路径

### P2: 幂等性
**属性**: 相同配置多次调用 PreviewTree，返回完全相同的树结构
**伪造策略**: 同一配置连续调用 3 次，断言响应 JSON 完全相等

### P3: 树结构有效性
**属性**: 树中每个节点的 path 都是其 name 的合法路径拼接
**伪造策略**: 遍历树，验证 child.path = parent.path + "/" + child.name

### P4: 目录包含性
**属性**: isDirectory=true 的节点必须有 children 字段（可为空数组）
**伪造策略**: 遍历树，验证所有目录节点都有 children

### P5: 文件叶子性
**属性**: isDirectory=false 的节点不能有 children 字段
**伪造策略**: 遍历树，验证所有文件节点无 children

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
- [ ] 配置变化时文件树自动更新（300ms 防抖）
- [ ] 文件树正确反映架构/ORM/UI 库差异
- [ ] 预览内容与实际生成内容一致
