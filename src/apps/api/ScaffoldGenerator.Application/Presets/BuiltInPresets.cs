using ScaffoldGenerator.Contracts.Enums;
using ScaffoldGenerator.Contracts.Presets;
using ScaffoldGenerator.Contracts.Requests;

namespace ScaffoldGenerator.Application.Presets;

/// <summary>
/// 内置预设定义
/// </summary>
public static class BuiltInPresets
{
    public static IReadOnlyList<ScaffoldPresetDto> All => new[]
    {
        new ScaffoldPresetDto(
            Id: "minimal",
            Name: "Minimal",
            Description: "最小化配置，仅包含核心功能",
            IsDefault: false,
            Tags: ["lightweight", "quick-start"],
            Config: new GenerateScaffoldRequest
            {
                Basic = new BasicOptions { ProjectName = "MyApp", Namespace = "MyApp" },
                Backend = new BackendOptions { Database = DatabaseProvider.SQLite, Cache = CacheProvider.None, Swagger = true, JwtAuth = false },
                Frontend = new FrontendOptions { RouterMode = RouterMode.Hash, MockData = false }
            }
        ),
        new ScaffoldPresetDto(
            Id: "standard",
            Name: "Standard",
            Description: "标准配置，适合大多数项目",
            IsDefault: true,
            Tags: ["recommended"],
            Config: new GenerateScaffoldRequest
            {
                Basic = new BasicOptions { ProjectName = "MyApp", Namespace = "MyApp" },
                Backend = new BackendOptions { Database = DatabaseProvider.SQLite, Cache = CacheProvider.MemoryCache, Swagger = true, JwtAuth = true },
                Frontend = new FrontendOptions { RouterMode = RouterMode.Hash, MockData = false }
            }
        ),
        new ScaffoldPresetDto(
            Id: "enterprise",
            Name: "Enterprise",
            Description: "企业级配置，包含完整功能",
            IsDefault: false,
            Tags: ["full-featured", "production"],
            Config: new GenerateScaffoldRequest
            {
                Basic = new BasicOptions { ProjectName = "MyApp", Namespace = "MyApp" },
                Backend = new BackendOptions { Database = DatabaseProvider.MySQL, Cache = CacheProvider.Redis, Swagger = true, JwtAuth = true },
                Frontend = new FrontendOptions { RouterMode = RouterMode.History, MockData = false }
            }
        )
    };
}
