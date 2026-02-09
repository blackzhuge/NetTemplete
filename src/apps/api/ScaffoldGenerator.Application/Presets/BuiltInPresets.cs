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
                Backend = new BackendOptions
                {
                    Architecture = ArchitectureStyle.Simple,
                    Orm = OrmProvider.SqlSugar,
                    Database = DatabaseProvider.SQLite,
                    Cache = CacheProvider.None,
                    Swagger = true,
                    JwtAuth = false
                },
                Frontend = new FrontendOptions
                {
                    UiLibrary = UiLibrary.ElementPlus,
                    RouterMode = RouterMode.Hash,
                    MockData = false
                }
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
                Backend = new BackendOptions
                {
                    Architecture = ArchitectureStyle.Simple,
                    Orm = OrmProvider.SqlSugar,
                    Database = DatabaseProvider.SQLite,
                    Cache = CacheProvider.MemoryCache,
                    Swagger = true,
                    JwtAuth = true
                },
                Frontend = new FrontendOptions
                {
                    UiLibrary = UiLibrary.ElementPlus,
                    RouterMode = RouterMode.Hash,
                    MockData = false
                }
            }
        ),
        new ScaffoldPresetDto(
            Id: "enterprise",
            Name: "Enterprise",
            Description: "企业级配置，Clean Architecture + EF Core",
            IsDefault: false,
            Tags: ["full-featured", "production", "clean-architecture"],
            Config: new GenerateScaffoldRequest
            {
                Basic = new BasicOptions { ProjectName = "MyApp", Namespace = "MyApp" },
                Backend = new BackendOptions
                {
                    Architecture = ArchitectureStyle.CleanArchitecture,
                    Orm = OrmProvider.EFCore,
                    Database = DatabaseProvider.MySQL,
                    Cache = CacheProvider.Redis,
                    Swagger = true,
                    JwtAuth = true
                },
                Frontend = new FrontendOptions
                {
                    UiLibrary = UiLibrary.ElementPlus,
                    RouterMode = RouterMode.History,
                    MockData = false
                }
            }
        ),
        new ScaffoldPresetDto(
            Id: "startup",
            Name: "Startup",
            Description: "快速启动配置，Simple + SqlSugar + Naive UI",
            IsDefault: false,
            Tags: ["startup", "quick-start", "naive-ui"],
            Config: new GenerateScaffoldRequest
            {
                Basic = new BasicOptions { ProjectName = "MyApp", Namespace = "MyApp" },
                Backend = new BackendOptions
                {
                    Architecture = ArchitectureStyle.Simple,
                    Orm = OrmProvider.SqlSugar,
                    Database = DatabaseProvider.SQLite,
                    Cache = CacheProvider.MemoryCache,
                    Swagger = true,
                    JwtAuth = false
                },
                Frontend = new FrontendOptions
                {
                    UiLibrary = UiLibrary.NaiveUI,
                    RouterMode = RouterMode.Hash,
                    MockData = true
                }
            }
        ),
        new ScaffoldPresetDto(
            Id: "ai-ready",
            Name: "AI Ready",
            Description: "AI 对话应用配置，集成 MateChat",
            IsDefault: false,
            Tags: ["ai", "chat", "matechat"],
            Config: new GenerateScaffoldRequest
            {
                Basic = new BasicOptions { ProjectName = "MyApp", Namespace = "MyApp" },
                Backend = new BackendOptions
                {
                    Architecture = ArchitectureStyle.Simple,
                    Orm = OrmProvider.SqlSugar,
                    Database = DatabaseProvider.SQLite,
                    Cache = CacheProvider.MemoryCache,
                    Swagger = true,
                    JwtAuth = true
                },
                Frontend = new FrontendOptions
                {
                    UiLibrary = UiLibrary.MateChat,
                    RouterMode = RouterMode.Hash,
                    MockData = false
                }
            }
        )
    };
}
