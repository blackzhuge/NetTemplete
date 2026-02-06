using ScaffoldGenerator.Contracts.Enums;

namespace ScaffoldGenerator.Contracts.Requests;

public sealed record GenerateScaffoldRequest
{
    public required string ProjectName { get; init; }

    public required string Namespace { get; init; }

    public DatabaseProvider Database { get; init; } = DatabaseProvider.SQLite;

    public CacheProvider Cache { get; init; } = CacheProvider.None;

    public bool EnableSwagger { get; init; } = true;

    public bool EnableJwtAuth { get; init; } = true;

    public RouterMode RouterMode { get; init; } = RouterMode.Hash;

    public bool EnableMockData { get; init; }
}
