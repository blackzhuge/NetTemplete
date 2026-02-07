using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using ScaffoldGenerator.Application.Packages;
using ScaffoldGenerator.Contracts.Packages;

namespace ScaffoldGenerator.Infrastructure.Packages;

/// <summary>
/// NuGet 包搜索服务实现
/// </summary>
public sealed class NuGetSearchService : IPackageSearchService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private const string DefaultSource = "https://api.nuget.org/v3/index.json";
    private const int CacheTtlMinutes = 5;
    private const int EmptyCacheTtlMinutes = 1;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public NuGetSearchService(HttpClient httpClient, IMemoryCache cache)
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
        var sourceUrl = source ?? DefaultSource;
        var cacheKey = $"nuget:{sourceUrl}:{query}:{take}";

        if (_cache.TryGetValue(cacheKey, out PackageSearchResponse? cached) && cached is not null)
        {
            return cached;
        }

        try
        {
            var searchUrl = await GetSearchUrlAsync(sourceUrl, ct);
            var requestUrl = $"{searchUrl}?q={Uri.EscapeDataString(query)}&take={take}&prerelease=false";

            var response = await _httpClient.GetFromJsonAsync<NuGetSearchResult>(requestUrl, JsonOptions, ct);

            var items = response?.Data?.Select(p => new PackageInfo(
                p.Id ?? string.Empty,
                p.Version ?? string.Empty,
                p.Description ?? string.Empty,
                p.IconUrl
            )).ToList() ?? [];

            var result = new PackageSearchResponse(items, response?.TotalHits ?? 0);

            var ttl = items.Count > 0 ? CacheTtlMinutes : EmptyCacheTtlMinutes;
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(ttl));

            return result;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new InvalidOperationException("NuGet 包源响应超时或不可用", ex);
        }
    }

    public async Task<IReadOnlyList<string>> GetVersionsAsync(
        string packageId,
        string? source,
        CancellationToken ct)
    {
        var sourceUrl = source ?? DefaultSource;
        var cacheKey = $"nuget:versions:{sourceUrl}:{packageId}";

        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<string>? cached) && cached is not null)
        {
            return cached;
        }

        try
        {
            var baseUrl = await GetPackageBaseAddressAsync(sourceUrl, ct);
            var versionsUrl = $"{baseUrl}{packageId.ToLowerInvariant()}/index.json";

            var response = await _httpClient.GetFromJsonAsync<NuGetVersionsResult>(versionsUrl, JsonOptions, ct);

            var versions = response?.Versions?
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

    private async Task<string> GetSearchUrlAsync(string sourceUrl, CancellationToken ct)
    {
        var cacheKey = $"nuget:searchUrl:{sourceUrl}";

        if (_cache.TryGetValue(cacheKey, out string? cached) && cached is not null)
        {
            return cached;
        }

        var json = await _httpClient.GetStringAsync(sourceUrl, ct);
        using var doc = JsonDocument.Parse(json);

        var resources = doc.RootElement.GetProperty("resources");
        foreach (var resource in resources.EnumerateArray())
        {
            if (resource.TryGetProperty("@type", out var typeElement))
            {
                var type = typeElement.GetString();
                if (type?.Contains("SearchQueryService") == true)
                {
                    var url = resource.GetProperty("@id").GetString()
                        ?? throw new InvalidOperationException("无法获取 NuGet 搜索服务地址");
                    _cache.Set(cacheKey, url, TimeSpan.FromHours(1));
                    return url;
                }
            }
        }

        throw new InvalidOperationException("无法获取 NuGet 搜索服务地址");
    }

    private async Task<string> GetPackageBaseAddressAsync(string sourceUrl, CancellationToken ct)
    {
        var cacheKey = $"nuget:baseAddress:{sourceUrl}";

        if (_cache.TryGetValue(cacheKey, out string? cached) && cached is not null)
        {
            return cached;
        }

        var json = await _httpClient.GetStringAsync(sourceUrl, ct);
        using var doc = JsonDocument.Parse(json);

        var resources = doc.RootElement.GetProperty("resources");
        foreach (var resource in resources.EnumerateArray())
        {
            if (resource.TryGetProperty("@type", out var typeElement))
            {
                var type = typeElement.GetString();
                if (type?.Contains("PackageBaseAddress") == true)
                {
                    var url = resource.GetProperty("@id").GetString()
                        ?? throw new InvalidOperationException("无法获取 NuGet 包基地址");
                    _cache.Set(cacheKey, url, TimeSpan.FromHours(1));
                    return url;
                }
            }
        }

        throw new InvalidOperationException("无法获取 NuGet 包基地址");
    }

    #region NuGet API DTOs

    private sealed record NuGetSearchResult(
        [property: JsonPropertyName("totalHits")] int TotalHits,
        [property: JsonPropertyName("data")] NuGetPackageData[]? Data
    );

    private sealed record NuGetPackageData(
        [property: JsonPropertyName("id")] string? Id,
        [property: JsonPropertyName("version")] string? Version,
        [property: JsonPropertyName("description")] string? Description,
        [property: JsonPropertyName("iconUrl")] string? IconUrl
    );

    private sealed record NuGetVersionsResult(
        [property: JsonPropertyName("versions")] string[]? Versions
    );

    #endregion
}
