# Scaffold Generator - Technical Design

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                        Monorepo Root                            │
├─────────────────────────────────────────────────────────────────┤
│  apps/                                                          │
│  ├── api/                    # .NET Minimal API                 │
│  ├── web-configurator/       # Vue 3 配置器                      │
│  └── template-frontend/      # 前端模板源码 (Live Source)        │
│                                                                 │
│  packages/                                                      │
│  ├── @scaffold/shared-types/ # TypeScript 类型定义              │
│  └── @scaffold/template-utils/ # 模板转换脚本                   │
│                                                                 │
│  templates/                                                     │
│  ├── backend/                # Scriban 后端模板                 │
│  └── frontend/               # 生成的前端模板                   │
└─────────────────────────────────────────────────────────────────┘
```

---

## Backend Design

### Project Structure

```
apps/api/
├── ScaffoldGenerator.Api/
│   ├── Program.cs              # Minimal API Host
│   ├── Endpoints/
│   │   └── ScaffoldEndpoints.cs
│   └── Middleware/
│       └── ExceptionMiddleware.cs
│
├── ScaffoldGenerator.Contracts/
│   ├── Requests/
│   │   └── GenerateScaffoldRequest.cs
│   └── Responses/
│       └── GenerationResult.cs
│
├── ScaffoldGenerator.Application/
│   ├── UseCases/
│   │   └── GenerateScaffoldUseCase.cs
│   ├── Interfaces/
│   │   ├── ITemplateRenderer.cs
│   │   └── IZipBuilder.cs
│   └── Validators/
│       └── GenerateScaffoldValidator.cs
│
├── ScaffoldGenerator.Infrastructure/
│   ├── Templates/
│   │   └── ScribanTemplateRenderer.cs
│   ├── Zip/
│   │   └── SystemZipBuilder.cs
│   └── FileSystem/
│       └── TemplateFileProvider.cs
│
└── ScaffoldGenerator.Modules/
    ├── IScaffoldModule.cs
    ├── JwtModule.cs
    ├── CacheModule.cs
    └── DatabaseModule.cs
```

### Key Interfaces

```csharp
// 模块贡献接口
public interface IScaffoldModule
{
    string Key { get; }
    bool IsEnabled(GenerateScaffoldRequest request);
    void Contribute(ScaffoldPlanBuilder builder, GenerateScaffoldRequest request);
}

// 模板渲染接口
public interface ITemplateRenderer
{
    Task<string> RenderAsync(string templatePath, object model, CancellationToken ct);
}

// ZIP 构建接口
public interface IZipBuilder
{
    Task<Stream> BuildAsync(IEnumerable<GeneratedFile> files, CancellationToken ct);
}
```

### Generation Flow

```
Request → Validate → Build Plan → Render Templates → Create ZIP → Response

1. ValidateRequest (FluentValidation)
2. LoadModules (根据配置筛选启用的模块)
3. BuildPlan (收集模板文件 + 变量)
4. RenderTemplates (Scriban 渲染)
5. CreateZip (System.IO.Compression)
6. ReturnStream (application/zip)
```

---

## Frontend Design

### Configurator Structure

```
apps/web-configurator/src/
├── api/
│   └── generator.ts           # API 调用封装
├── components/
│   ├── common/
│   │   └── FileTreeView.vue   # 文件树组件
│   └── configurator/
│       ├── ConfigForm.vue     # 配置表单容器
│       ├── BasicOptions.vue   # 基础配置
│       ├── BackendOptions.vue # 后端配置
│       └── FrontendOptions.vue# 前端配置
├── composables/
│   ├── useConfig.ts           # 配置状态管理
│   └── useFileTree.ts         # 文件树生成逻辑
├── stores/
│   └── configStore.ts         # Pinia Store
├── types/
│   └── config.ts              # 类型定义
└── views/
    └── HomePage.vue           # 主页面 (分屏布局)
```

### UI Layout

```
┌────────────────────────────────────────────────────────────┐
│  Scaffold Generator                              [Generate] │
├───────────────────────────┬────────────────────────────────┤
│                           │                                │
│  📁 Basic                 │  📂 my-app/                    │
│  ├─ Project Name: [____]  │  ├── backend/                  │
│  └─ Namespace: [____]     │  │   ├── src/                  │
│                           │  │   │   ├── Api/              │
│  📁 Backend               │  │   │   ├── Application/      │
│  ├─ Database: [SQLite ▼]  │  │   │   └── Infrastructure/   │
│  ├─ Cache: [None ▼]       │  │   └── MyApp.sln             │
│  ├─ ☑ Swagger             │  └── frontend/                 │
│  └─ ☑ JWT Auth            │      ├── src/                  │
│                           │      │   ├── views/            │
│  📁 Frontend              │      │   └── main.ts           │
│  ├─ Router: [Hash ▼]      │      └── package.json          │
│  └─ ☐ Mock Data           │                                │
│                           │                                │
└───────────────────────────┴────────────────────────────────┘
```

### State Flow

```
ConfigStore (Pinia)
     │
     ├──> ConfigForm (v-model 双向绑定)
     │
     └──> FileTreeView (computed 响应式预览)
```

---

## Template System

### Manifest Format (backend/manifest.json)

```json
{
  "version": "1.0",
  "files": [
    {
      "source": "src/Api/Program.cs.sbn",
      "target": "backend/src/{{basic.namespace}}.Api/Program.cs"
    },
    {
      "source": "modules/jwt/JwtExtensions.cs.sbn",
      "target": "backend/src/{{basic.namespace}}.Api/Auth/JwtExtensions.cs",
      "when": { "path": "backend.jwtAuth", "equals": true }
    }
  ]
}
```

### Template Variables

```json
{
  "basic": {
    "projectName": "MyApp",
    "namespace": "MyApp"
  },
  "backend": {
    "database": "SQLite",
    "cache": "None",
    "swagger": true,
    "jwtAuth": true
  },
  "frontend": {
    "routerMode": "hash",
    "mockData": false
  }
}
```

---

## Security Considerations

1. **Path Traversal Prevention**: 验证生成路径不包含 `..`
2. **Zip Slip Protection**: 检查解压路径在目标目录内
3. **Input Sanitization**: projectName 只允许字母数字
4. **Template Isolation**: 模板目录只读挂载

---

## Error Handling

| Error Type | HTTP Code | Response |
|------------|-----------|----------|
| Validation Error | 400 | `{ "errors": [...] }` |
| Invalid Combination | 422 | `{ "message": "..." }` |
| Template Not Found | 500 | `{ "message": "..." }` |
| IO Error | 503 | `{ "message": "..." }` |
