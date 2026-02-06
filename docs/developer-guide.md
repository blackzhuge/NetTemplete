# 开发者文档

本文档介绍 Scaffold Generator 的项目架构和开发指南。

## 架构概述

项目采用 Clean Architecture 分层设计：

```
┌─────────────────────────────────────────┐
│              API Layer                  │  ← 入口点、路由、中间件
├─────────────────────────────────────────┤
│           Application Layer             │  ← 用例、模块、验证
├─────────────────────────────────────────┤
│          Infrastructure Layer           │  ← 模板渲染、文件操作
├─────────────────────────────────────────┤
│            Contracts Layer              │  ← DTOs、枚举、接口
└─────────────────────────────────────────┘
```

## 后端项目结构

### ScaffoldGenerator.Api

API 入口层，负责：
- Minimal API 端点定义
- 中间件配置（CORS、异常处理）
- 依赖注入配置
- Serilog 日志配置

**关键文件**：
- `Program.cs` - 应用入口和配置

### ScaffoldGenerator.Application

应用层，包含业务逻辑：
- `UseCases/` - 用例实现
- `Modules/` - 功能模块（Database、Cache、JWT 等）
- `Validators/` - FluentValidation 验证器
- `Abstractions/` - 接口定义

**关键接口**：
- `IScaffoldModule` - 模块接口
- `ITemplateRenderer` - 模板渲染接口
- `IZipBuilder` - ZIP 构建接口

### ScaffoldGenerator.Infrastructure

基础设施层，实现具体技术：
- `Rendering/` - Scriban 模板渲染
- `FileSystem/` - ZIP 文件操作

**性能优化**：
- 模板缓存：使用 `IMemoryCache` 缓存已解析的模板
- 滑动过期 30 分钟，绝对过期 2 小时

### ScaffoldGenerator.Contracts

契约层，定义数据结构：
- `Requests/` - 请求 DTOs
- `Responses/` - 响应 DTOs
- `Enums/` - 枚举定义

## 前端项目结构

```
src/apps/web-configurator/src/
├── api/              # API 调用
├── components/       # Vue 组件
├── composables/      # 组合式函数
├── schemas/          # Zod 验证 schema
├── stores/           # Pinia 状态管理
├── types/            # TypeScript 类型
└── views/            # 页面视图
```

### 技术栈

| 技术 | 用途 |
|------|------|
| Vue 3 | UI 框架 |
| TypeScript | 类型安全 |
| Vite | 构建工具 |
| Element Plus | UI 组件库 |
| Pinia | 状态管理 |
| VeeValidate + Zod | 表单验证 |
| Axios | HTTP 客户端 |

## 开发流程

### 环境准备

```bash
# 安装依赖
pnpm install

# 启动后端
dotnet run --project src/apps/api/ScaffoldGenerator.Api

# 启动前端
pnpm --filter web-configurator dev
```

### 代码规范

#### 后端

- 使用 `sealed record` 定义 DTOs
- 异步方法接受 `CancellationToken`
- 使用构造函数注入
- 遵循 Minimal API 模式

#### 前端

- 使用 `<script setup lang="ts">`
- Store 状态通过 `storeToRefs()` 访问
- 禁止使用 `any` 类型
- 使用 `@/` 路径别名

### 构建验证

```bash
# 后端构建
dotnet build src/apps/api/ScaffoldGenerator.Api

# 前端类型检查
pnpm --filter web-configurator exec vue-tsc --noEmit

# 前端构建
pnpm --filter web-configurator build
```

## API 端点

### 生成脚手架

```
POST /api/v1/scaffolds/generate-zip
Content-Type: application/json

{
  "basic": {
    "projectName": "MyProject",
    "namespace": "MyProject"
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

Response: application/zip (成功) 或 JSON (错误)
```

### 健康检查

```
GET /health

Response: { "status": "healthy" }
```

## 错误处理

### 错误码

| 错误码 | HTTP 状态 | 说明 |
|--------|-----------|------|
| ValidationError | 400 | 输入验证失败 |
| InvalidCombination | 422 | 配置组合无效 |
| TemplateError | 500 | 模板渲染错误 |

### 错误响应格式

```json
{
  "error": "错误描述信息"
}
```

## 扩展开发

### 添加新的配置选项

1. **Contracts**: 在 `GenerateScaffoldRequest` 添加属性
2. **Validator**: 更新验证规则
3. **Module**: 创建或更新模块处理新选项
4. **Frontend**: 更新类型和表单组件
5. **Templates**: 添加或更新模板文件

### 添加新的功能模块

参见 [模板扩展指南](./template-guide.md#模块系统)

## 调试

### 后端日志

日志输出到控制台，使用 Serilog 格式化：

```bash
dotnet run --project src/apps/api/ScaffoldGenerator.Api
```

### 前端调试

使用浏览器开发者工具：
- Network 面板查看 API 请求
- Console 面板查看错误信息
- Vue DevTools 查看组件状态

## 部署

### 后端部署

```bash
dotnet publish -c Release -o ./publish
```

确保 `templates/` 目录随应用一起部署。

### 前端部署

```bash
pnpm --filter web-configurator build
```

构建产物在 `dist/` 目录，可部署到任何静态文件服务器。

### Docker（可选）

```dockerfile
# 后端
FROM mcr.microsoft.com/dotnet/aspnet:9.0
COPY publish/ /app
COPY templates/ /app/templates
WORKDIR /app
ENTRYPOINT ["dotnet", "ScaffoldGenerator.Api.dll"]
```
