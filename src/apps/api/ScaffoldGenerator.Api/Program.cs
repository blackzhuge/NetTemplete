using FluentValidation;
using ScaffoldGenerator.Api.Endpoints;
using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Application.Packages;
using ScaffoldGenerator.Application.Presets;
using ScaffoldGenerator.Application.Preview;
using ScaffoldGenerator.Application.Validators;
using ScaffoldGenerator.Contracts.Preview;
using ScaffoldGenerator.Contracts.Responses;
using ScaffoldGenerator.Application.Modules;
using ScaffoldGenerator.Application.Providers;
using ScaffoldGenerator.Application.UseCases;
using ScaffoldGenerator.Contracts.Requests;
using ScaffoldGenerator.Infrastructure.FileSystem;
using ScaffoldGenerator.Infrastructure.Packages;
using ScaffoldGenerator.Infrastructure.Rendering;
using Serilog;
using System.Text.Json;
using System.Text.Json.Serialization;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// JSON 配置 - 支持枚举字符串
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// DI Registration
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IValidator<GenerateScaffoldRequest>, GenerateScaffoldValidator>();
builder.Services.AddScoped<IZipBuilder, SystemZipBuilder>();
builder.Services.AddScoped<ITemplateFileProvider>(_ =>
    new FileSystemTemplateProvider(Path.Combine(AppContext.BaseDirectory, "templates")));
builder.Services.AddScoped<ITemplateRenderer, ScribanTemplateRenderer>();

// Module Registration
builder.Services.AddScoped<IScaffoldModule, CoreModule>();
builder.Services.AddScoped<IScaffoldModule, ArchitectureModule>();
builder.Services.AddScoped<IScaffoldModule, OrmModule>();
builder.Services.AddScoped<IScaffoldModule, DatabaseModule>();
builder.Services.AddScoped<IScaffoldModule, CacheModule>();
builder.Services.AddScoped<IScaffoldModule, JwtModule>();
builder.Services.AddScoped<IScaffoldModule, SwaggerModule>();
builder.Services.AddScoped<IScaffoldModule, FrontendModule>();
builder.Services.AddScoped<ScaffoldPlanBuilder>();
builder.Services.AddScoped<GenerateScaffoldUseCase>();

// UI Library Providers
builder.Services.AddSingleton<IUiLibraryProvider, ElementPlusProvider>();
builder.Services.AddSingleton<IUiLibraryProvider, AntDesignProvider>();
builder.Services.AddSingleton<IUiLibraryProvider, NaiveUiProvider>();
builder.Services.AddSingleton<IUiLibraryProvider, TailwindProvider>();
builder.Services.AddSingleton<IUiLibraryProvider, ShadcnVueProvider>();
builder.Services.AddSingleton<IUiLibraryProvider, MateChatProvider>();

// Preset and Preview Services
builder.Services.AddScoped<IPresetService, PresetService>();
builder.Services.AddScoped<IPreviewService, PreviewService>();
builder.Services.AddScoped<IValidator<PreviewFileRequest>, PreviewFileRequestValidator>();

// Package Search Services
builder.Services.AddHttpClient<NuGetSearchService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddHttpClient<NpmSearchService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});

var app = builder.Build();

// 启动时验证所有预设
using (var scope = app.Services.CreateScope())
{
    var presetService = scope.ServiceProvider.GetRequiredService<IPresetService>();
    presetService.ValidateAllPresets();
    Log.Information("All presets validated successfully");
}

app.UseSerilogRequestLogging();
app.UseCors();

// Exception handling middleware
app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (BadHttpRequestException ex)
    {
        // 400 for invalid input (JSON parse errors, enum binding failures)
        Log.Warning(ex, "Bad request");
        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new { error = "请求格式无效", details = ex.Message });
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Unhandled exception");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { error = "服务器内部错误" });
    }
});

// Health check
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

// Generate scaffold endpoint - 符合规范的路由
app.MapPost("/api/v1/scaffolds/generate-zip", async (
    GenerateScaffoldRequest request,
    GenerateScaffoldUseCase useCase,
    CancellationToken ct) =>
{
    var result = await useCase.ExecuteAsync(request, ct);

    if (!result.Success)
    {
        // 根据错误类型返回不同状态码
        var statusCode = result.ErrorCode switch
        {
            ErrorCode.ValidationError => 400,
            ErrorCode.InvalidCombination => 422,
            ErrorCode.TemplateError => 500,
            _ => 400
        };
        return Results.Json(new { error = result.ErrorMessage }, statusCode: statusCode);
    }

    return Results.File(
        result.FileContent,
        "application/zip",
        result.FileName);
});

// 保留旧路由作为兼容别名
app.MapPost("/api/generate", async (
    GenerateScaffoldRequest request,
    GenerateScaffoldUseCase useCase,
    CancellationToken ct) =>
{
    var result = await useCase.ExecuteAsync(request, ct);

    if (!result.Success)
    {
        return Results.BadRequest(new { error = result.ErrorMessage });
    }

    return Results.File(
        result.FileContent,
        "application/zip",
        result.FileName);
});

Log.Information("Starting ScaffoldGenerator API");

// 注册端点
app.MapPresetsEndpoints();
app.MapPreviewEndpoints();
app.MapPackagesEndpoints();

app.Run();

// Expose Program class for integration tests
public partial class Program { }
