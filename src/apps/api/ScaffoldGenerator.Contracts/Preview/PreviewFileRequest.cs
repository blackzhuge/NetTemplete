using System.Text.Json.Serialization;
using ScaffoldGenerator.Contracts.Requests;

namespace ScaffoldGenerator.Contracts.Preview;

/// <summary>
/// 文件预览请求 DTO
/// POST /api/v1/scaffolds/preview-file
/// </summary>
public sealed record PreviewFileRequest(
    [property: JsonPropertyName("config")] GenerateScaffoldRequest Config,
    [property: JsonPropertyName("outputPath")] string OutputPath
);
