namespace ScaffoldGenerator.Contracts.Packages;

/// <summary>
/// 包搜索响应
/// </summary>
public sealed record PackageSearchResponse(
    IReadOnlyList<PackageInfo> Items,
    int TotalCount
);
