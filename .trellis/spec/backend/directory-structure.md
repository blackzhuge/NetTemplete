# 目录结构

> 后端代码的组织方式。

---

## 概述

本项目使用 **Clean Architecture + Module Pattern**，确保关注点分离和可扩展性。

---

## 目录布局

```
src/apps/api/
├── ScaffoldGenerator.Api/              # API 层（入口点）
│   ├── Program.cs                      # 启动、DI 注册、路由
│   ├── Endpoints/                      # Minimal API 端点扩展
│   │   ├── PackagesEndpoints.cs
│   │   ├── PresetsEndpoints.cs
│   │   └── PreviewEndpoints.cs
│   └── templates/                      # Scriban 模板文件
│
├── ScaffoldGenerator.Application/      # 应用层（业务逻辑）
│   ├── Abstractions/                   # 接口定义
│   │   ├── IScaffoldModule.cs          # 模块接口
│   │   ├── ITemplateRenderer.cs
│   │   └── IZipBuilder.cs
│   ├── Modules/                        # 功能模块（策略模式）
│   │   ├── CoreModule.cs               # Order=0, 基础框架
│   │   ├── DatabaseModule.cs           # Order=10, 数据库配置
│   │   ├── CacheModule.cs              # Order=20, 缓存配置
│   │   ├── JwtModule.cs                # Order=30, JWT 认证
│   │   ├── SwaggerModule.cs            # Order=40, API 文档
│   │   └── FrontendModule.cs           # Order=100, 前端文件
│   ├── UseCases/                       # 业务用例
│   │   └── GenerateScaffoldUseCase.cs
│   ├── Validators/                     # FluentValidation 验证器
│   ├── Presets/                        # 预设管理
│   ├── Preview/                        # 预览服务
│   └── Packages/                       # 包搜索接口
│
├── ScaffoldGenerator.Infrastructure/   # 基础设施层
│   ├── Rendering/                      # 模板渲染实现
│   │   └── ScribanTemplateRenderer.cs
│   ├── FileSystem/                     # 文件系统操作
│   │   └── SystemZipBuilder.cs
│   └── Packages/                       # 包搜索实现
│       ├── NuGetSearchService.cs
│       └── NpmSearchService.cs
│
└── ScaffoldGenerator.Contracts/        # 契约层（DTOs）
    ├── Enums/
    ├── Requests/
    ├── Responses/
    ├── Presets/
    ├── Preview/
    └── Packages/
```

---

## 层职责

| 层 | 职责 | 依赖 |
|----|------|------|
| **Api** | HTTP 端点、中间件、DI | Application、Infrastructure |
| **Application** | 业务逻辑、验证、模块 | 仅依赖 Contracts |
| **Contracts** | DTOs、枚举 | 无依赖 |
| **Infrastructure** | 外部服务实现 | Application（实现接口） |

---

## Module Pattern

### IScaffoldModule 接口

```csharp
public interface IScaffoldModule
{
    string Name { get; }              // 模块标识
    int Order { get; }                // 执行顺序（越小越先）
    bool IsEnabled(GenerateScaffoldRequest request);
    Task ContributeAsync(ScaffoldPlan plan, GenerateScaffoldRequest request, CancellationToken ct);
}
```

### Order 排序规范

| Order | Module | 职责 |
|-------|--------|------|
| 0 | CoreModule | Program.cs、appsettings.json |
| 10 | DatabaseModule | SqlSugar 配置 |
| 20 | CacheModule | Redis/MemoryCache |
| 30 | JwtModule | JWT 认证 |
| 40 | SwaggerModule | Swagger 文档 |
| 100 | FrontendModule | 前端文件 |

---

## 添加新功能

1. **定义 DTOs** 在 `Contracts/`
2. **定义接口** 在 `Application/Abstractions/`
3. **实现 Module** 在 `Application/Modules/`（或 UseCase）
4. **实现基础设施** 在 `Infrastructure/`
5. **注册 DI** 在 `Api/Program.cs`
6. **添加端点** 在 `Api/Endpoints/`

---

## 命名约定

| 类型 | 约定 | 示例 |
|------|------|------|
| 项目 | `{Product}.{Layer}` | `ScaffoldGenerator.Application` |
| 模块 | `{Feature}Module` | `DatabaseModule` |
| UseCase | `{Action}{Entity}UseCase` | `GenerateScaffoldUseCase` |
| 端点扩展 | `{Feature}Endpoints` | `PresetsEndpoints` |

---

## 禁止模式

| 模式 | 原因 |
|------|------|
| 在 Api 层放业务逻辑 | 违反关注点分离 |
| 循环项目引用 | 破坏整洁架构 |
| 静态类处理业务逻辑 | 难以测试 |
