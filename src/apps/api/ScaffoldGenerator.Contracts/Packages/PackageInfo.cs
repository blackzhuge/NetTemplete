namespace ScaffoldGenerator.Contracts.Packages;

/// <summary>
/// 包信息 DTO - 用于搜索结果展示
/// </summary>
public sealed record PackageInfo(
    string Name,
    string Version,
    string Description,
    string? IconUrl = null,
    long? DownloadCount = null,
    DateTimeOffset? LastUpdated = null
);
