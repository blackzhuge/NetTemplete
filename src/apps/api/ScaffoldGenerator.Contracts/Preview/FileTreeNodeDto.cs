using System.Text.Json.Serialization;

namespace ScaffoldGenerator.Contracts.Preview;

/// <summary>
/// 文件树节点 DTO
/// </summary>
public sealed record FileTreeNodeDto(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("isDirectory")] bool IsDirectory,
    [property: JsonPropertyName("children")] List<FileTreeNodeDto>? Children = null
);
