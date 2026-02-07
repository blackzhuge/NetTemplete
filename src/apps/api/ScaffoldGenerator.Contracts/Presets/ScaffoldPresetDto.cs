using System.Text.Json.Serialization;
using ScaffoldGenerator.Contracts.Requests;

namespace ScaffoldGenerator.Contracts.Presets;

/// <summary>
/// 预设模板 DTO
/// </summary>
public sealed record ScaffoldPresetDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("isDefault")] bool IsDefault,
    [property: JsonPropertyName("tags")] IReadOnlyList<string> Tags,
    [property: JsonPropertyName("config")] GenerateScaffoldRequest Config
);
