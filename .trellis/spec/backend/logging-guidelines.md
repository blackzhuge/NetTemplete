# 日志规范

> 本项目的日志记录方式。

---

## 概述

**框架**：Serilog
**输出**：Console（开发）、File（生产）
**集成**：通过 `UseSerilog()` 集成 ASP.NET Core

### 依赖

```xml
<!-- src/Directory.Packages.props -->
<PackageVersion Include="Serilog.AspNetCore" Version="9.0.0" />
<PackageVersion Include="Serilog.Sinks.Console" Version="5.0.1" />
<PackageVersion Include="Serilog.Sinks.File" Version="5.0.0" />
```

---

## 配置

### 启动配置

**文件**：`Api/Program.cs`

```csharp
// 早期初始化（在构建 host 前）
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

// 集成 ASP.NET Core
builder.Host.UseSerilog();

// 启用请求日志
app.UseSerilogRequestLogging();
```

### 应用生命周期

```csharp
try
{
    Log.Information("Starting ScaffoldGenerator API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
```

---

## 日志级别

| 级别 | 何时使用 | 示例 |
|------|----------|------|
| `Verbose` | 详细调试 | 循环迭代、变量值 |
| `Debug` | 开发诊断 | 方法进入/退出、状态变化 |
| `Information` | 正常操作 | 应用启动、请求完成、功能使用 |
| `Warning` | 意外但已处理 | 重试、降级、弃用使用 |
| `Error` | 需要关注的错误 | 异常捕获、操作失败 |
| `Fatal` | 应用无法继续 | 启动失败、不可恢复错误 |

### 使用示例

```csharp
// Information - 应用生命周期
Log.Information("Starting ScaffoldGenerator API");
Log.Information("Generation completed for project {ProjectName}", request.ProjectName);

// Warning - 已处理的问题
Log.Warning("Template cache miss, reloading from disk");
Log.Warning("Deprecated API endpoint called: {Endpoint}", endpoint);

// Error - 异常
Log.Error(ex, "Unhandled exception");
Log.Error(ex, "Failed to render template {TemplatePath}", templatePath);

// Fatal - 应用无法继续
Log.Fatal(ex, "Application terminated unexpectedly");
```

---

## 结构化日志

### 使用消息模板（不是字符串插值）

```csharp
// 正确 - 结构化日志
Log.Information("User {UserId} created order {OrderId}", userId, orderId);

// 错误 - 字符串插值（丢失结构）
Log.Information($"User {userId} created order {orderId}");
```

### 属性命名

| 类型 | 格式 | 示例 |
|------|------|------|
| 标识符 | PascalCase | `{UserId}`、`{OrderId}` |
| 计数 | 以 `Count` 结尾 | `{ItemCount}` |
| 时长 | 以 `Ms` 或 `Duration` 结尾 | `{ElapsedMs}` |
| 解构对象 | 前缀 `@` | `{@Request}` |

```csharp
Log.Information("Processed {ItemCount} items in {ElapsedMs}ms", items.Count, sw.ElapsedMilliseconds);
Log.Debug("Request details: {@Request}", request);
```

---

## 应该记录什么

### 必须记录

| 事件 | 级别 | 示例 |
|------|------|------|
| 应用启动/停止 | Information | `Starting API`、`Shutting down` |
| 未处理的异常 | Error | `Log.Error(ex, "Unhandled exception")` |
| 认证失败 | Warning | `Login failed for {Username}` |
| 关键业务操作 | Information | `Order {OrderId} created` |
| 外部服务调用（失败） | Error | `Payment gateway returned error` |

### 考虑记录

| 事件 | 级别 |
|------|------|
| 请求/响应（通过中间件） | Information |
| 缓存命中/未命中 | Debug |
| 数据库查询耗时 | Debug |
| 配置加载 | Information |

---

## 不应该记录什么

### 永不记录

| 数据类型 | 原因 |
|----------|------|
| 密码 | 安全 |
| API 密钥 / Token | 安全 |
| 信用卡号 | PCI 合规 |
| 完整身份证号 | PII |
| 会话令牌 | 安全 |

### 敏感数据脱敏

```csharp
// 错误
Log.Information("User logged in with password {Password}", password);

// 正确
Log.Information("User {Username} logged in successfully", username);

// 如必须包含，进行脱敏
Log.Information("Processing card ending in {CardLast4}", card[^4..]);
```

---

## 请求日志

通过 `UseSerilogRequestLogging()` 启用：

```
[14:23:45 INF] HTTP GET /api/health responded 200 in 12.3456 ms
[14:23:46 INF] HTTP POST /api/generate responded 200 in 234.5678 ms
```

### 自定义请求日志

```csharp
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
    };
});
```

---

## 异常日志

### 模式

```csharp
catch (Exception ex)
{
    // 始终将异常对象放在第一个参数
    Log.Error(ex, "Failed to process request for {ProjectName}", request.ProjectName);

    // 返回错误响应
    return new GenerationResult { Success = false, ErrorMessage = "..." };
}
```

### 中间件异常日志

```csharp
catch (Exception ex)
{
    Log.Error(ex, "Unhandled exception");
    context.Response.StatusCode = 500;
    await context.Response.WriteAsJsonAsync(new { error = "服务器内部错误" });
}
```

---

## 常见错误

| 错误 | 正确做法 |
|------|----------|
| 日志中使用字符串插值 | 使用 `{Property}` 的消息模板 |
| 缺少异常对象 | 将 `ex` 作为第一个参数 |
| 记录 PII | 脱敏或排除敏感数据 |
| 循环中过度日志 | 记录摘要，而非每次迭代 |
| 缺少上下文 | 包含相关 ID 和名称 |

---

## 生产配置（待完成）

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateLogger();
```
