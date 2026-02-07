using ScaffoldGenerator.Contracts.Packages;

namespace ScaffoldGenerator.Application.Packages;

/// <summary>
/// 包搜索服务接口
/// </summary>
public interface IPackageSearchService
{
    Task<PackageSearchResponse> SearchAsync(
        string query,
        string? source,
        int take,
        CancellationToken ct);

    Task<IReadOnlyList<string>> GetVersionsAsync(
        string packageId,
        string? source,
        CancellationToken ct);
}
