# Logging Guidelines

> How logging is done in this project.

---

## Overview

**Framework**: Serilog
**Sinks**: Console (development), File (production)
**Integration**: ASP.NET Core via `UseSerilog()`

### Dependencies

```xml
<!-- src/Directory.Packages.props -->
<PackageVersion Include="Serilog.AspNetCore" Version="9.0.0" />
<PackageVersion Include="Serilog.Sinks.Console" Version="5.0.1" />
<PackageVersion Include="Serilog.Sinks.File" Version="5.0.0" />
```

---

## Configuration

### Startup Configuration

**File**: `Api/Program.cs`

```csharp
// Early initialization (before host build)
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

// Integrate with ASP.NET Core
builder.Host.UseSerilog();

// Enable request logging
app.UseSerilogRequestLogging();
```

### Application Lifecycle

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

## Log Levels

| Level | When to Use | Example |
|-------|-------------|---------|
| `Verbose` | Detailed debugging | Loop iterations, variable values |
| `Debug` | Development diagnostics | Method entry/exit, state changes |
| `Information` | Normal operations | App start, request completed, feature used |
| `Warning` | Unexpected but handled | Retry, fallback, deprecated usage |
| `Error` | Errors that need attention | Exception caught, operation failed |
| `Fatal` | App cannot continue | Startup failure, unrecoverable error |

### Usage Examples

```csharp
// Information - Application lifecycle
Log.Information("Starting ScaffoldGenerator API");
Log.Information("Generation completed for project {ProjectName}", request.ProjectName);

// Warning - Handled issues
Log.Warning("Template cache miss, reloading from disk");
Log.Warning("Deprecated API endpoint called: {Endpoint}", endpoint);

// Error - Exceptions
Log.Error(ex, "Unhandled exception");
Log.Error(ex, "Failed to render template {TemplatePath}", templatePath);

// Fatal - Application cannot continue
Log.Fatal(ex, "Application terminated unexpectedly");
```

---

## Structured Logging

### Use Message Templates (Not String Interpolation)

```csharp
// GOOD - Structured logging
Log.Information("User {UserId} created order {OrderId}", userId, orderId);

// BAD - String interpolation (loses structure)
Log.Information($"User {userId} created order {orderId}");
```

### Property Naming

| Type | Format | Example |
|------|--------|---------|
| Identifiers | PascalCase | `{UserId}`, `{OrderId}` |
| Counts | End with `Count` | `{ItemCount}` |
| Durations | End with `Ms` or `Duration` | `{ElapsedMs}` |
| Destructured objects | Prefix with `@` | `{@Request}` |

```csharp
Log.Information("Processed {ItemCount} items in {ElapsedMs}ms", items.Count, sw.ElapsedMilliseconds);
Log.Debug("Request details: {@Request}", request);
```

---

## What to Log

### Must Log

| Event | Level | Example |
|-------|-------|---------|
| Application start/stop | Information | `Starting API`, `Shutting down` |
| Unhandled exceptions | Error | `Log.Error(ex, "Unhandled exception")` |
| Authentication failures | Warning | `Login failed for {Username}` |
| Critical business operations | Information | `Order {OrderId} created` |
| External service calls (failure) | Error | `Payment gateway returned error` |

### Consider Logging

| Event | Level |
|-------|-------|
| Request/response (via middleware) | Information |
| Cache hits/misses | Debug |
| Database query timing | Debug |
| Configuration loaded | Information |

---

## What NOT to Log

### Never Log

| Data Type | Reason |
|-----------|--------|
| Passwords | Security |
| API keys / Tokens | Security |
| Credit card numbers | PCI compliance |
| Full SSN / ID numbers | PII |
| Session tokens | Security |

### Mask Sensitive Data

```csharp
// BAD
Log.Information("User logged in with password {Password}", password);

// GOOD
Log.Information("User {Username} logged in successfully", username);

// If must include, mask it
Log.Information("Processing card ending in {CardLast4}", card[^4..]);
```

---

## Request Logging

Enabled via `UseSerilogRequestLogging()`:

```
[14:23:45 INF] HTTP GET /api/health responded 200 in 12.3456 ms
[14:23:46 INF] HTTP POST /api/generate responded 200 in 234.5678 ms
```

### Customize Request Logging

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

## Exception Logging

### Pattern

```csharp
catch (Exception ex)
{
    // Always include exception object first
    Log.Error(ex, "Failed to process request for {ProjectName}", request.ProjectName);

    // Return error response
    return new GenerationResult { Success = false, ErrorMessage = "..." };
}
```

### Middleware Exception Logging

```csharp
catch (Exception ex)
{
    Log.Error(ex, "Unhandled exception");
    context.Response.StatusCode = 500;
    await context.Response.WriteAsJsonAsync(new { error = "服务器内部错误" });
}
```

---

## Common Mistakes

| Mistake | Correct Approach |
|---------|------------------|
| String interpolation in log | Use message template with `{Property}` |
| Missing exception object | Put `ex` as first parameter |
| Logging PII | Mask or exclude sensitive data |
| Over-logging in loops | Log summary, not each iteration |
| Missing context | Include relevant IDs and names |

---

## Production Configuration (TODO)

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
