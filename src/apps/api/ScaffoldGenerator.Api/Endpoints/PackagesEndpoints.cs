using ScaffoldGenerator.Application.Packages;
using ScaffoldGenerator.Infrastructure.Packages;

namespace ScaffoldGenerator.Api.Endpoints;

/// <summary>
/// 包管理 API 端点
/// </summary>
public static class PackagesEndpoints
{
    public static void MapPackagesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/packages");

        // NuGet 搜索
        group.MapGet("/nuget/search", async (
            string q,
            string? source,
            int? take,
            NuGetSearchService nugetService,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Results.BadRequest(new { error = "搜索关键词不能为空" });
            }

            try
            {
                var result = await nugetService.SearchAsync(q, source, take ?? 20, ct);
                return Results.Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: 504);
            }
        });

        // npm 搜索
        group.MapGet("/npm/search", async (
            string q,
            string? source,
            int? take,
            NpmSearchService npmService,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Results.BadRequest(new { error = "搜索关键词不能为空" });
            }

            try
            {
                var result = await npmService.SearchAsync(q, source, take ?? 20, ct);
                return Results.Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: 504);
            }
        });

        // NuGet 包版本
        group.MapGet("/nuget/{id}/versions", async (
            string id,
            string? source,
            NuGetSearchService nugetService,
            CancellationToken ct) =>
        {
            try
            {
                var versions = await nugetService.GetVersionsAsync(id, source, ct);
                return Results.Ok(new { versions });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: 504);
            }
        });

        // npm 包版本
        group.MapGet("/npm/{name}/versions", async (
            string name,
            string? source,
            NpmSearchService npmService,
            CancellationToken ct) =>
        {
            try
            {
                var versions = await npmService.GetVersionsAsync(name, source, ct);
                return Results.Ok(new { versions });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: 504);
            }
        });
    }
}
