# Package Manager Integration

## Context

用户希望在脚手架配置器中增加 NuGet 和 npm 包管理功能：
- 后端配置支持选择 NuGet 包
- 前端配置支持选择 npm 包
- 支持包搜索、版本选择
- 支持自定义包源地址
- 预置官方默认源

## Status

research-complete

## Created

2026-02-07

---

## 调研发现

### 现有架构

#### 后端

| 组件 | 路径 | 说明 |
|------|------|------|
| BackendOptions | `src/apps/api/ScaffoldGenerator.Contracts/Requests/GenerateScaffoldRequest.cs` | 需扩展 NugetPackages 字段 |
| FrontendOptions | 同上 | 需扩展 NpmPackages 字段 |
| ScaffoldPlan | `src/apps/api/ScaffoldGenerator.Application/Abstractions/ScaffoldPlan.cs` | 已有 AddNugetPackage/AddNpmPackage 方法，但仅存储包名 |
| .csproj 模板 | `src/apps/api/ScaffoldGenerator.Api/templates/backend/Api.csproj.sbn` | 硬编码版本，需改为动态渲染 |
| package.json 模板 | `src/apps/api/ScaffoldGenerator.Api/templates/frontend/package.json.sbn` | 需支持动态依赖 |
| API 端点 | `src/apps/api/ScaffoldGenerator.Api/Endpoints/` | Minimal API 模式 |

#### 前端

| 组件 | 路径 | 说明 |
|------|------|------|
| Store | `src/apps/web-configurator/src/stores/config.ts` | Pinia store，需扩展包列表状态 |
| Types | `src/apps/web-configurator/src/types/index.ts` | 需添加 PackageInfo 类型 |
| Schema | `src/apps/web-configurator/src/schemas/config.ts` | Zod 验证，需扩展 |
| API | `src/apps/web-configurator/src/api/generator.ts` | Axios 实例 |

### 外部 API

#### NuGet V3 API

```
入口: https://api.nuget.org/v3/index.json
搜索: SearchQueryService (动态获取)
示例: GET {searchUrl}?q=Serilog&take=20
```

#### npm Registry API

```
搜索: https://registry.npmjs.org/-/v1/search?text={query}
包信息: https://registry.npmjs.org/{package}
```

---

## 约束集合

### 硬约束 (Hard Constraints)

1. **架构约束**
   - 后端服务接口定义在 Application 层 (IPackageSearchService)
   - 实现在 Infrastructure 层 (NuGetSearchService, NpmSearchService)
   - API 使用 Minimal API 模式

2. **数据结构约束**
   - 包信息必须包含: Name, Version, Description
   - 版本号遵循 SemVer 格式
   - 包源地址必须是有效 URL

3. **UI 约束**
   - 使用 Element Plus 组件 (el-select, el-autocomplete)
   - 表单验证使用 vee-validate + Zod
   - 搜索防抖 300ms

4. **模板约束**
   - Scriban 模板语法
   - 动态包列表使用 for 循环渲染

### 软约束 (Soft Constraints)

1. 搜索结果分页（每页 20 条）
2. 版本选择默认最新稳定版
3. 支持包源连通性测试
4. 缓存搜索结果（5 分钟）

### 预置默认值

```json
{
  "nugetSources": [
    { "name": "nuget.org", "url": "https://api.nuget.org/v3/index.json", "isDefault": true }
  ],
  "npmSources": [
    { "name": "npmjs.org", "url": "https://registry.npmjs.org/", "isDefault": true },
    { "name": "淘宝镜像", "url": "https://registry.npmmirror.com/", "isDefault": false }
  ]
}
```

---

## 技术方案

### 后端新增文件

```
src/apps/api/
├── ScaffoldGenerator.Contracts/
│   ├── Packages/
│   │   ├── PackageInfo.cs           # 包信息 DTO
│   │   ├── PackageSearchRequest.cs  # 搜索请求
│   │   └── PackageSearchResponse.cs # 搜索响应
│   └── Requests/
│       └── GenerateScaffoldRequest.cs  # 扩展 Packages 字段
├── ScaffoldGenerator.Application/
│   └── Packages/
│       └── IPackageSearchService.cs # 搜索服务接口
├── ScaffoldGenerator.Infrastructure/
│   └── Packages/
│       ├── NuGetSearchService.cs    # NuGet API 实现
│       └── NpmSearchService.cs      # npm API 实现
└── ScaffoldGenerator.Api/
    └── Endpoints/
        └── PackagesEndpoints.cs     # 包搜索 API
```

### 前端新增文件

```
src/apps/web-configurator/src/
├── components/
│   └── PackageSelector.vue          # 包选择器组件
├── api/
│   └── packages.ts                  # 包搜索 API
└── types/
    └── packages.ts                  # 包相关类型
```

### API 端点设计

```
GET /api/v1/packages/nuget/search?q={query}&source={sourceUrl}
GET /api/v1/packages/npm/search?q={query}&source={sourceUrl}
GET /api/v1/packages/nuget/{packageId}/versions
GET /api/v1/packages/npm/{packageName}/versions
```

---

## 成功判据

1. [ ] 用户可以在后端配置中搜索并选择 NuGet 包
2. [ ] 用户可以在前端配置中搜索并选择 npm 包
3. [ ] 选择的包版本正确写入生成的 .csproj 和 package.json
4. [ ] 支持切换包源地址
5. [ ] 默认预置官方源地址
6. [ ] 搜索响应时间 < 2 秒

---

## 开放问题

需要用户确认：

1. **包源管理 UI**：是在设置页面单独管理，还是在选择包时内联配置？
2. **包依赖冲突**：是否需要在选择时检测依赖冲突？
3. **常用包推荐**：是否需要预置常用包快捷选择列表？
4. **包分类**：是否需要按用途分类（ORM、日志、认证等）？

---

## Sources

- [NuGet V3 API Documentation](https://learn.microsoft.com/en-us/nuget/api/overview)
- [npm Registry API](https://github.com/npm/registry/blob/master/docs/REGISTRY-API.md)
- [edoardoscibona.com - npm API](https://edoardoscibona.com)
- [npmjs.org - Search API](https://www.npmjs.org)
