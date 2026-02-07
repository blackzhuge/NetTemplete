using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using ScaffoldGenerator.Application.Packages;
using ScaffoldGenerator.Contracts.Packages;

namespace ScaffoldGenerator.Infrastructure.Packages;

/// <summary>
/// npm 包搜索服务实现
/// </summary>
public sealed class NpmSearchService : IPackageSearchService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private const string DefaultSource = "https://registry.npmjs.org";
    private const int CacheTtlMinutes = 5;
    private const int EmptyCacheTtlMinutes = 1;

    public NpmSearchService(HttpClient httpClient, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public async Task<PackageSearchResponse> SearchAsync(
        string query,
        string? source,
        int take,
        CancellationToken ct)
    {
        var sourceUrl = (source ?? DefaultSource).TrimEnd('/');
        var cacheKey = $"npm:{sourceUrl}:{query}:{take}";

        if (_cache.TryGetValue(cacheKey, out PackageSearchResponse? cached) && cached is not null)
        {
            return cached;
        }

        try
        {
            var requestUrl = $"{sourceUrl}/-/v1/search?text={Uri.EscapeDataString(query)}&size={take}";

            var response = await _httpClient.GetFromJsonAsync<NpmSearchResult>(requestUrl, ct);

            var items = response?.Objects?.Select(o => new PackageInfo(
                o.Package?.Name ?? string.Empty,
                o.Package?.Version ?? string.Empty,
                o.Package?.Description ?? string.Empty,
                null
            )).ToList() ?? [];

            var result = new PackageSearchResponse(items, response?.Total ?? 0);

            var ttl = items.Count > 0 ? CacheTtlMinutes : EmptyCacheTtlMinutes;
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(ttl));

            return result;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new InvalidOperationException("npm 包源响应超时或不可用", ex);
        }
    }

    public async Task<IReadOnlyList<string>> GetVersionsAsync(
        string packageId,
        string? source,
        CancellationToken ct)
    {
        var sourceUrl = (source ?? DefaultSource).TrimEnd('/');
        var cacheKey = $"npm:versions:{sourceUrl}:{packageId}";

        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<string>? cached) && cached is not null)
        {
            return cached;
        }

        try
        {
            var requestUrl = $"{sourceUrl}/{Uri.EscapeDataString(packageId)}";

            var response = await _httpClient.GetFromJsonAsync<NpmPackageMetadata>(requestUrl, ct);

            var versions = response?.Versions?.Keys
                .Where(v => !v.Contains('-'))
                .Reverse()
                .Take(20)
                .ToList() ?? [];

            _cache.Set(cacheKey, versions, TimeSpan.FromMinutes(CacheTtlMinutes));

            return versions;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new InvalidOperationException($"获取包 {packageId} 版本失败", ex);
        }
    }

    #region npm API DTOs

    private sealed record NpmSearchResult(
        [property: JsonPropertyName("objects")] NpmSearchObject[]? Objects,
        [property: JsonPropertyName("total")] int Total
    );

    private sealed record NpmSearchObject(
        [property: JsonPropertyName("package")] NpmPackageInfo? Package
    );

    private sealed record NpmPackageInfo(
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("version")] string? Version,
        [property: JsonPropertyName("description")] string? Description
    );

    private sealed record NpmPackageMetadata(
        [property: JsonPropertyName("versions")] Dictionary<string, object>? Versions
    );

    #endregion
}
