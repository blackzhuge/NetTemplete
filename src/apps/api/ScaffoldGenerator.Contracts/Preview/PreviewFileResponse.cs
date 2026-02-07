using System.Text.Json.Serialization;

namespace ScaffoldGenerator.Contracts.Preview;

/// <summary>
/// 文件预览响应 DTO
/// </summary>
public sealed record PreviewFileResponse(
    [property: JsonPropertyName("outputPath")] string OutputPath,
    [property: JsonPropertyName("content")] string Content,
    [property: JsonPropertyName("language")] string Language,
    [property: JsonPropertyName("isTemplate")] bool IsTemplate
);
