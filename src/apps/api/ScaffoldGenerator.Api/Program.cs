using FluentValidation;
using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Application.Modules;
using ScaffoldGenerator.Application.UseCases;
using ScaffoldGenerator.Application.Validators;
using ScaffoldGenerator.Contracts.Requests;
using ScaffoldGenerator.Infrastructure.FileSystem;
using ScaffoldGenerator.Infrastructure.Rendering;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

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
    builder.Services.AddScoped<IValidator<GenerateScaffoldRequest>, GenerateScaffoldValidator>();
    builder.Services.AddScoped<IZipBuilder, SystemZipBuilder>();
    builder.Services.AddScoped<ITemplateFileProvider>(_ =>
        new FileSystemTemplateProvider(Path.Combine(Directory.GetCurrentDirectory(), "templates")));
    builder.Services.AddScoped<ITemplateRenderer, ScribanTemplateRenderer>();

    // Module Registration
    builder.Services.AddScoped<IScaffoldModule, CoreModule>();
    builder.Services.AddScoped<IScaffoldModule, DatabaseModule>();
    builder.Services.AddScoped<IScaffoldModule, CacheModule>();
    builder.Services.AddScoped<IScaffoldModule, JwtModule>();
    builder.Services.AddScoped<IScaffoldModule, SwaggerModule>();
    builder.Services.AddScoped<IScaffoldModule, FrontendModule>();
    builder.Services.AddScoped<ScaffoldPlanBuilder>();
    builder.Services.AddScoped<GenerateScaffoldUseCase>();

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.UseCors();

    // Exception handling middleware
    app.Use(async (context, next) =>
    {
        try
        {
            await next(context);
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

    // Generate scaffold endpoint
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
