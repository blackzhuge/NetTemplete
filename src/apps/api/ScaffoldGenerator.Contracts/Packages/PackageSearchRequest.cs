namespace ScaffoldGenerator.Contracts.Packages;

/// <summary>
/// 包搜索请求
/// </summary>
public sealed record PackageSearchRequest(
    string Query,
    string? Source = null,
    int Take = 20
);
