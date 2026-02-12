using System.Text.Json.Serialization;
using ScaffoldGenerator.Contracts.Requests;

namespace ScaffoldGenerator.Contracts.Preview;

/// <summary>
/// 文件树预览请求 DTO
/// POST /api/scaffold/preview/tree
/// </summary>
public sealed record PreviewTreeRequest(
    [property: JsonPropertyName("config")] GenerateScaffoldRequest Config
);

/// <summary>
/// 文件树预览响应 DTO
/// </summary>
public sealed record PreviewTreeResponse(
    [property: JsonPropertyName("tree")] List<FileTreeNodeDto> Tree
);
