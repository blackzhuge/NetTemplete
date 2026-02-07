using System.Text.Json.Serialization;

namespace ScaffoldGenerator.Contracts.Presets;

/// <summary>
/// 预设列表响应 DTO
/// GET /api/v1/scaffolds/presets
/// </summary>
public sealed record ScaffoldPresetsResponse(
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("presets")] IReadOnlyList<ScaffoldPresetDto> Presets
);
