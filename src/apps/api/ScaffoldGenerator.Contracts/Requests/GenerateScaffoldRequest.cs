using System.Text.Json.Serialization;
using ScaffoldGenerator.Contracts.Enums;
using ScaffoldGenerator.Contracts.Packages;

namespace ScaffoldGenerator.Contracts.Requests;

/// <summary>
/// API 请求 DTO - 符合规范的嵌套结构
/// POST /api/v1/scaffolds/generate-zip
/// </summary>
public sealed record GenerateScaffoldRequest
{
    [JsonPropertyName("basic")]
    public required BasicOptions Basic { get; init; }

    [JsonPropertyName("backend")]
    public BackendOptions Backend { get; init; } = new();

    [JsonPropertyName("frontend")]
    public FrontendOptions Frontend { get; init; } = new();
}

public sealed record BasicOptions
{
    [JsonPropertyName("projectName")]
    public required string ProjectName { get; init; }

    [JsonPropertyName("namespace")]
    public required string Namespace { get; init; }
}

public sealed record BackendOptions
{
    [JsonPropertyName("architecture")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ArchitectureStyle Architecture { get; init; } = ArchitectureStyle.Simple;

    [JsonPropertyName("orm")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OrmProvider Orm { get; init; } = OrmProvider.SqlSugar;

    [JsonPropertyName("database")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DatabaseProvider Database { get; init; } = DatabaseProvider.SQLite;

    [JsonPropertyName("cache")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CacheProvider Cache { get; init; } = CacheProvider.None;

    [JsonPropertyName("swagger")]
    public bool Swagger { get; init; } = true;

    [JsonPropertyName("jwtAuth")]
    public bool JwtAuth { get; init; } = true;

    [JsonPropertyName("nugetPackages")]
    public List<PackageReference> NugetPackages { get; init; } = [];
}

public sealed record FrontendOptions
{
    [JsonPropertyName("uiLibrary")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UiLibrary UiLibrary { get; init; } = UiLibrary.ElementPlus;

    [JsonPropertyName("routerMode")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RouterMode RouterMode { get; init; } = RouterMode.Hash;

    [JsonPropertyName("mockData")]
    public bool MockData { get; init; }

    [JsonPropertyName("npmPackages")]
    public List<PackageReference> NpmPackages { get; init; } = [];
}
