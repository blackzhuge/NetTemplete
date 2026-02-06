# 模板扩展指南

本文档介绍如何添加或修改 Scaffold Generator 的模板文件。

## 模板系统概述

Scaffold Generator 使用 [Scriban](https://github.com/scriban/scriban) 作为模板引擎。

### 模板位置

```
templates/
├── backend/
│   ├── manifest.json       # 后端模板清单
│   ├── Program.cs.sbn      # 入口文件模板
│   ├── appsettings.json.sbn
│   └── ...
└── frontend/
    ├── manifest.json       # 前端模板清单
    ├── main.ts.sbn
    └── ...
```

## 模板语法

### 基本语法

```scriban
// 变量输出
{{ project_name }}

// 条件判断
{{ if enable_swagger }}
app.UseSwagger();
{{ end }}

// 循环
{{ for item in items }}
  {{ item.name }}
{{ end }}
```

### 可用变量

模板中可使用的变量来自请求配置：

| 变量 | 类型 | 说明 |
|------|------|------|
| `project_name` | string | 项目名称 |
| `namespace` | string | 命名空间 |
| `database` | string | 数据库类型 (SQLite/MySQL/SQLServer) |
| `cache` | string | 缓存类型 (None/MemoryCache/Redis) |
| `enable_swagger` | bool | 是否启用 Swagger |
| `enable_jwt_auth` | bool | 是否启用 JWT 认证 |
| `router_mode` | string | 路由模式 (hash/history) |
| `enable_mock_data` | bool | 是否启用 Mock 数据 |

## 添加新模板

### 1. 创建模板文件

在 `templates/backend/` 或 `templates/frontend/` 目录下创建 `.sbn` 文件：

```scriban
// templates/backend/MyService.cs.sbn
namespace {{ namespace }}.Services;

public class MyService
{
    public string GetMessage()
    {
        return "Hello from {{ project_name }}!";
    }
}
```

### 2. 更新清单文件

编辑对应的 `manifest.json`，添加新模板：

```json
{
  "files": [
    {
      "template": "MyService.cs.sbn",
      "output": "src/{{ project_name }}.Api/Services/MyService.cs"
    }
  ]
}
```

### 3. 条件渲染

如果模板只在特定条件下生成：

```json
{
  "files": [
    {
      "template": "RedisConfig.cs.sbn",
      "output": "src/{{ project_name }}.Api/Config/RedisConfig.cs",
      "condition": "cache == 'Redis'"
    }
  ]
}
```

## 模块系统

### 模块接口

每个功能模块实现 `IScaffoldModule` 接口：

```csharp
public interface IScaffoldModule
{
    string Name { get; }
    bool ShouldApply(GenerateScaffoldRequest request);
    Task<IEnumerable<ScaffoldFile>> GetFilesAsync(
        GenerateScaffoldRequest request,
        CancellationToken ct);
}
```

### 创建新模块

1. 在 `ScaffoldGenerator.Application/Modules/` 创建模块类
2. 实现 `IScaffoldModule` 接口
3. 在 `Program.cs` 注册模块

示例：

```csharp
public sealed class MyModule : IScaffoldModule
{
    public string Name => "MyModule";

    public bool ShouldApply(GenerateScaffoldRequest request)
    {
        // 判断是否应用此模块
        return request.Backend.EnableMyFeature;
    }

    public async Task<IEnumerable<ScaffoldFile>> GetFilesAsync(
        GenerateScaffoldRequest request,
        CancellationToken ct)
    {
        // 返回模块生成的文件列表
        return new[]
        {
            new ScaffoldFile("path/to/file.cs", "content")
        };
    }
}
```

## 最佳实践

### 1. 保持模板简洁

- 避免复杂的逻辑判断
- 将复杂逻辑放在模块代码中

### 2. 使用一致的命名

- 模板变量使用 snake_case
- 输出路径使用变量替换

### 3. 测试模板

修改模板后，使用配置器测试生成结果：

1. 启动开发服务器
2. 在配置器中选择相关选项
3. 下载并检查生成的文件

### 4. 版本控制

- 模板文件应纳入版本控制
- 重大变更时更新文档

## 调试技巧

### 查看渲染错误

模板渲染错误会返回 500 状态码，查看 API 日志获取详细信息：

```bash
dotnet run --project src/apps/api/ScaffoldGenerator.Api
```

### 本地测试模板

```csharp
var renderer = new ScribanTemplateRenderer(...);
var result = await renderer.RenderAsync("template.sbn", model);
Console.WriteLine(result);
```
