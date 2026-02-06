# 目录结构

> 后端代码的组织方式。

---

## 概述

本项目使用 **Clean Architecture（整洁架构）**，分为四层，确保关注点分离和可测试性。

**技术栈**：.NET 9、Minimal API、SqlSugar ORM、Serilog、FluentValidation

---

## 目录布局

```
src/apps/api/
├── ScaffoldGenerator.Api/              # API 层（入口点）
│   ├── Program.cs                      # 启动、DI 注册、路由
│   └── templates/                      # Scriban 模板文件
│
├── ScaffoldGenerator.Application/      # 应用层（业务逻辑）
│   ├── Abstractions/                   # 接口定义
│   │   ├── ITemplateRenderer.cs
│   │   ├── ITemplateFileProvider.cs
│   │   └── IZipBuilder.cs
│   ├── UseCases/                       # 业务用例
│   │   └── GenerateScaffoldUseCase.cs
│   └── Validators/                     # FluentValidation 验证器
│       └── GenerateScaffoldValidator.cs
│
├── ScaffoldGenerator.Contracts/        # 契约层（DTOs）
│   ├── Enums/                          # 枚举定义
│   │   ├── DatabaseProvider.cs
│   │   ├── CacheProvider.cs
│   │   └── RouterMode.cs
│   ├── Requests/                       # 请求 DTOs
│   │   └── GenerateScaffoldRequest.cs
│   └── Responses/                      # 响应 DTOs
│       └── GenerationResult.cs
│
└── ScaffoldGenerator.Infrastructure/   # 基础设施层（外部依赖）
    ├── Rendering/                      # 模板引擎实现
    │   └── ScribanTemplateRenderer.cs
    └── FileSystem/                     # 文件系统操作
        ├── FileSystemTemplateProvider.cs
        └── SystemZipBuilder.cs
```

---

## 层职责

| 层 | 职责 | 依赖 |
|----|------|------|
| **Api** | HTTP 端点、中间件、DI 设置 | Application、Infrastructure |
| **Application** | 业务逻辑、验证、用例 | 仅依赖 Contracts |
| **Contracts** | DTOs、枚举、共享类型 | 无依赖 |
| **Infrastructure** | 外部服务实现 | Application（实现接口） |

### 依赖流向

```
Api → Application → Contracts
 ↓
Infrastructure → Application（实现接口）
```

---

## 模块组织

### 添加新功能

1. **定义 DTOs** 在 `Contracts/`
   - 请求 DTO 在 `Requests/`
   - 响应 DTO 在 `Responses/`
   - 枚举在 `Enums/`

2. **定义接口** 在 `Application/Abstractions/`

3. **实现 UseCase** 在 `Application/UseCases/`
   - 需要时在 `Validators/` 添加验证器

4. **实现基础设施** 在 `Infrastructure/`
   - 为功能领域创建文件夹

5. **注册 DI** 在 `Api/Program.cs`

6. **添加端点** 在 `Api/Program.cs`

---

## 命名约定

| 类型 | 约定 | 示例 |
|------|------|------|
| 项目 | `{Product}.{Layer}` | `ScaffoldGenerator.Application` |
| 接口 | `I` 前缀 + PascalCase | `ITemplateRenderer` |
| UseCase | `{Action}{Entity}UseCase` | `GenerateScaffoldUseCase` |
| 验证器 | `{Entity}Validator` | `GenerateScaffoldValidator` |
| 请求 DTO | `{Action}{Entity}Request` | `GenerateScaffoldRequest` |
| 响应 DTO | `{Entity}Result` 或 `{Entity}Response` | `GenerationResult` |
| 实现类 | 描述性名称 | `ScribanTemplateRenderer` |

---

## 示例

### 良好组织的功能：Scaffold 生成

| 文件 | 用途 |
|------|------|
| `Contracts/Requests/GenerateScaffoldRequest.cs` | 输入 DTO |
| `Contracts/Responses/GenerationResult.cs` | 输出 DTO |
| `Application/Abstractions/ITemplateRenderer.cs` | 接口 |
| `Application/UseCases/GenerateScaffoldUseCase.cs` | 业务逻辑 |
| `Application/Validators/GenerateScaffoldValidator.cs` | 验证规则 |
| `Infrastructure/Rendering/ScribanTemplateRenderer.cs` | 实现 |

---

## 禁止模式

| 模式 | 原因 |
|------|------|
| 在 Api 层直接访问数据库 | 违反关注点分离 |
| 在 Infrastructure 层放业务逻辑 | Infrastructure 仅用于外部依赖 |
| 循环项目引用 | 破坏整洁架构 |
| 使用静态类处理业务逻辑 | 难以测试，违反 DI 原则 |
