using System.Text.Json.Serialization;

namespace ScaffoldGenerator.Contracts.Packages;

/// <summary>
/// 包引用值对象 - 用于存储用户选择的包
/// </summary>
public sealed record PackageReference(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("source")] string? Source = null
);
