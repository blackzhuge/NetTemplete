using ScaffoldGenerator.Application.Presets;

namespace ScaffoldGenerator.Api.Endpoints;

/// <summary>
/// 预设 API 端点
/// </summary>
public static class PresetsEndpoints
{
    public static void MapPresetsEndpoints(this WebApplication app)
    {
        app.MapGet("/api/v1/scaffolds/presets", async (
            IPresetService presetService,
            CancellationToken ct) =>
        {
            var response = await presetService.GetPresetsAsync(ct);
            return Results.Ok(response);
        });
    }
}
