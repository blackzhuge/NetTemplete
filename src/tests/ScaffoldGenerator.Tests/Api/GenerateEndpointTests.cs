using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ScaffoldGenerator.Contracts.Enums;
using ScaffoldGenerator.Contracts.Preview;
using ScaffoldGenerator.Contracts.Requests;
using ScaffoldGenerator.Infrastructure.Rendering;
using Xunit;

namespace ScaffoldGenerator.Tests.Api;

public class GenerateEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public GenerateEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static GenerateScaffoldRequest CreateRequest(
        string projectName = "TestProject",
        string ns = "TestProject",
        DatabaseProvider database = DatabaseProvider.SQLite,
        OrmProvider orm = OrmProvider.SqlSugar,
        CacheProvider cache = CacheProvider.None,
        bool swagger = true,
        bool jwtAuth = true,
        UiLibrary uiLibrary = UiLibrary.ElementPlus,
        RouterMode routerMode = RouterMode.Hash,
        bool mockData = false)
    {
        return new GenerateScaffoldRequest
        {
            Basic = new BasicOptions
            {
                ProjectName = projectName,
                Namespace = ns
            },
            Backend = new BackendOptions
            {
                Orm = orm,
                Database = database,
                Cache = cache,
                Swagger = swagger,
                JwtAuth = jwtAuth
            },
            Frontend = new FrontendOptions
            {
                UiLibrary = uiLibrary,
                RouterMode = routerMode,
                MockData = mockData
            }
        };
    }

    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Generate_ValidRequest_ReturnsZipFile()
    {
        var request = CreateRequest();

        var response = await _client.PostAsJsonAsync("/api/v1/scaffolds/generate-zip", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/zip");

        var content = await response.Content.ReadAsByteArrayAsync();
        content.Should().NotBeEmpty();
        content[0].Should().Be(0x50);
        content[1].Should().Be(0x4B);
    }

    [Fact]
    public async Task Generate_LegacyEndpoint_StillWorks()
    {
        var request = CreateRequest();

        var response = await _client.PostAsJsonAsync("/api/generate", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/zip");
    }

    [Fact]
    public async Task Generate_EmptyProjectName_ReturnsBadRequest()
    {
        var request = CreateRequest(projectName: "", ns: "Test");

        var response = await _client.PostAsJsonAsync("/api/v1/scaffolds/generate-zip", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Generate_InvalidNamespace_ReturnsBadRequest()
    {
        var request = CreateRequest(projectName: "Test", ns: "123-invalid");

        var response = await _client.PostAsJsonAsync("/api/v1/scaffolds/generate-zip", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Generate_AllDatabaseProviders_Succeeds()
    {
        foreach (var provider in Enum.GetValues<DatabaseProvider>())
        {
            var request = CreateRequest(
                projectName: $"Test{provider}",
                ns: $"Test{provider}",
                database: provider);

            var response = await _client.PostAsJsonAsync("/api/v1/scaffolds/generate-zip", request);
            response.StatusCode.Should().Be(HttpStatusCode.OK,
                $"Database provider {provider} should work");
        }
    }

    [Fact]
    public async Task Generate_AllCacheProviders_Succeeds()
    {
        foreach (var provider in Enum.GetValues<CacheProvider>())
        {
            var request = CreateRequest(
                projectName: $"TestCache{provider}",
                ns: $"TestCache{provider}",
                cache: provider);

            var response = await _client.PostAsJsonAsync("/api/v1/scaffolds/generate-zip", request);
            response.StatusCode.Should().Be(HttpStatusCode.OK,
                $"Cache provider {provider} should work");
        }
    }

    [Fact]
    public async Task PreviewTree_ValidRequest_ReturnsTree()
    {
        var request = new PreviewTreeRequest(CreateRequest());

        var response = await _client.PostAsJsonAsync("/api/v1/scaffolds/preview-tree", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PreviewTree_EmptyProjectName_ReturnsBadRequest()
    {
        var invalidRequest = new GenerateScaffoldRequest
        {
            Basic = new BasicOptions { ProjectName = "", Namespace = "Test" }
        };
        var request = new PreviewTreeRequest(invalidRequest);

        var response = await _client.PostAsJsonAsync("/api/v1/scaffolds/preview-tree", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PreviewTree_TreeStructureMatchesContract()
    {
        var request = new PreviewTreeRequest(CreateRequest());

        var response = await _client.PostAsJsonAsync("/api/v1/scaffolds/preview-tree", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PreviewTreeResponse>();
        result.Should().NotBeNull();
        result!.Tree.Should().NotBeEmpty();

        foreach (var node in result.Tree)
        {
            node.Name.Should().NotBeEmpty();
            node.Path.Should().NotBeEmpty();

            if (node.IsDirectory)
            {
                node.Children.Should().NotBeNull();
            }
            else
            {
                node.Children.Should().BeNull();
            }
        }
    }

    [Fact]
    public async Task PreviewFile_EFCoreSelection_ContainsEfCorePackageReferencesInCsproj()
    {
        var request = CreateRequest(
            projectName: "MyApp",
            ns: "MyApp",
            database: DatabaseProvider.SQLite,
            orm: OrmProvider.EFCore);

        var previewRequest = new PreviewFileRequest(
            request,
            "src/MyApp.Api/MyApp.Api.csproj");

        var response = await _client.PostAsJsonAsync("/api/v1/scaffolds/preview-file", previewRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var preview = await response.Content.ReadFromJsonAsync<PreviewFileResponse>();
        preview.Should().NotBeNull();
        preview!.Content.Should().Contain("Microsoft.EntityFrameworkCore");
        preview.Content.Should().Contain("Microsoft.EntityFrameworkCore.Design");
        preview.Content.Should().Contain("Microsoft.EntityFrameworkCore.Sqlite");
        preview.Content.Should().NotContain("SqlSugarCore");
    }

    [Fact]
    public async Task PreviewFile_DapperSelection_UsesDapperWithoutSqlSugar()
    {
        var request = CreateRequest(
            projectName: "MyApp",
            ns: "MyApp",
            database: DatabaseProvider.SQLite,
            orm: OrmProvider.Dapper);

        var previewRequest = new PreviewFileRequest(
            request,
            "src/MyApp.Api/MyApp.Api.csproj");

        var response = await _client.PostAsJsonAsync("/api/v1/scaffolds/preview-file", previewRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var preview = await response.Content.ReadFromJsonAsync<PreviewFileResponse>();
        preview.Should().NotBeNull();
        preview!.Content.Should().Contain("Dapper");
        preview.Content.Should().NotContain("SqlSugarCore");
    }

    [Fact]
    public async Task PreviewFile_ProgramTemplate_UsesSelectedOrmSetupCall()
    {
        var request = CreateRequest(
            projectName: "MyApp",
            ns: "MyApp",
            database: DatabaseProvider.SQLite,
            orm: OrmProvider.EFCore);

        var previewRequest = new PreviewFileRequest(
            request,
            "src/MyApp.Api/Program.cs");

        var response = await _client.PostAsJsonAsync("/api/v1/scaffolds/preview-file", previewRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var preview = await response.Content.ReadFromJsonAsync<PreviewFileResponse>();
        preview.Should().NotBeNull();
        preview!.Content.Should().Contain("AddEFCore(builder.Configuration)");
        preview.Content.Should().NotContain("AddSqlSugar(builder.Configuration)");
    }

    [Fact]
    public async Task PreviewFile_AllUiLibrariesAndRouterModes_ShouldReturnOkAndRenderRouter()
    {
        foreach (var uiLibrary in Enum.GetValues<UiLibrary>())
        {
            foreach (var routerMode in Enum.GetValues<RouterMode>())
            {
                var request = CreateRequest(
                    projectName: $"MyApp{uiLibrary}{routerMode}",
                    ns: $"MyApp{uiLibrary}{routerMode}",
                    orm: OrmProvider.EFCore,
                    uiLibrary: uiLibrary,
                    routerMode: routerMode);

                var previewRequest = new PreviewFileRequest(
                    request,
                    $"src/MyApp{uiLibrary}{routerMode}.Web/src/router/index.ts");

                var response = await _client.PostAsJsonAsync("/api/v1/scaffolds/preview-file", previewRequest);

                response.StatusCode.Should().Be(HttpStatusCode.OK,
                    $"UI={uiLibrary}, Router={routerMode} should preview successfully");

                var preview = await response.Content.ReadFromJsonAsync<PreviewFileResponse>();
                preview.Should().NotBeNull();

                if (routerMode == RouterMode.Hash)
                {
                    preview!.Content.Should().Contain("createWebHashHistory");
                }
                else
                {
                    preview!.Content.Should().Contain("createWebHistory");
                }
            }
        }
    }

    [Theory]
    [InlineData(UiLibrary.ElementPlus, "element-plus")]
    [InlineData(UiLibrary.AntDesignVue, "ant-design-vue")]
    [InlineData(UiLibrary.NaiveUI, "naive-ui")]
    [InlineData(UiLibrary.TailwindHeadless, "@headlessui/vue")]
    [InlineData(UiLibrary.ShadcnVue, "radix-vue")]
    [InlineData(UiLibrary.MateChat, "@matechat/core")]
    public async Task PreviewFile_PackageJson_ShouldContainSelectedUiLibraryDependencies(
        UiLibrary uiLibrary,
        string expectedDependency)
    {
        var request = CreateRequest(
            projectName: $"MyApp{uiLibrary}",
            ns: $"MyApp{uiLibrary}",
            orm: OrmProvider.EFCore,
            uiLibrary: uiLibrary);

        var previewRequest = new PreviewFileRequest(
            request,
            $"src/MyApp{uiLibrary}.Web/package.json");

        var response = await _client.PostAsJsonAsync("/api/v1/scaffolds/preview-file", previewRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK, $"UI={uiLibrary} should preview package.json");

        var preview = await response.Content.ReadFromJsonAsync<PreviewFileResponse>();
        preview.Should().NotBeNull();
        preview!.Content.Should().Contain("\"vue\"");
        preview.Content.Should().Contain($"\"{expectedDependency}\"");
    }

    [Fact]
    public async Task PreviewFile_ShadcnVuePackageJson_ShouldPlaceBuildToolsInDevDependencies()
    {
        var request = CreateRequest(
            projectName: "MyAppShadcn",
            ns: "MyAppShadcn",
            orm: OrmProvider.EFCore,
            uiLibrary: UiLibrary.ShadcnVue);

        var previewRequest = new PreviewFileRequest(
            request,
            "src/MyAppShadcn.Web/package.json");

        var response = await _client.PostAsJsonAsync("/api/v1/scaffolds/preview-file", previewRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var preview = await response.Content.ReadFromJsonAsync<PreviewFileResponse>();
        preview.Should().NotBeNull();
        preview!.Content.Should().Contain("\"tailwindcss\": \"^3.4.0\"");
        preview.Content.Should().Contain("\"postcss\": \"^8.4.0\"");
        preview.Content.Should().Contain("\"autoprefixer\": \"^10.4.0\"");

        var dependenciesIndex = preview.Content.IndexOf("\"dependencies\":", StringComparison.Ordinal);
        var devDependenciesIndex = preview.Content.IndexOf("\"devDependencies\":", StringComparison.Ordinal);
        var tailwindIndex = preview.Content.IndexOf("\"tailwindcss\": \"^3.4.0\"", StringComparison.Ordinal);

        dependenciesIndex.Should().BeGreaterThan(-1);
        devDependenciesIndex.Should().BeGreaterThan(dependenciesIndex);
        tailwindIndex.Should().BeGreaterThan(devDependenciesIndex);
    }

    [Theory]
    [InlineData(RouterMode.Hash, "history: createWebHashHistory(import.meta.env.BASE_URL)")]
    [InlineData(RouterMode.History, "history: createWebHistory(import.meta.env.BASE_URL)")]
    public async Task PreviewFile_RouterIndex_ShouldFollowRouterModeSelection(
        RouterMode routerMode,
        string expectedHistoryLine)
    {
        var request = CreateRequest(
            projectName: $"MyAppRouter{routerMode}",
            ns: $"MyAppRouter{routerMode}",
            orm: OrmProvider.EFCore,
            uiLibrary: UiLibrary.ElementPlus,
            routerMode: routerMode);

        var previewRequest = new PreviewFileRequest(
            request,
            $"src/MyAppRouter{routerMode}.Web/src/router/index.ts");

        var response = await _client.PostAsJsonAsync("/api/v1/scaffolds/preview-file", previewRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var preview = await response.Content.ReadFromJsonAsync<PreviewFileResponse>();
        preview.Should().NotBeNull();
        preview!.Content.Should().Contain(expectedHistoryLine);
    }

    [Fact]
    public async Task PreviewTree_EnableMockData_ShouldIncludeMockFile()
    {
        var request = CreateRequest(
            projectName: "MyAppMock",
            ns: "MyAppMock",
            orm: OrmProvider.EFCore,
            mockData: true);

        var treeRequest = new PreviewTreeRequest(request);
        var response = await _client.PostAsJsonAsync("/api/v1/scaffolds/preview-tree", treeRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var tree = await response.Content.ReadFromJsonAsync<PreviewTreeResponse>();
        tree.Should().NotBeNull();

        var paths = FlattenPaths(tree!.Tree).ToList();
        paths.Should().Contain("src/MyAppMock.Web/src/mock/index.ts");
    }

    [Fact]
    public async Task PreviewFile_EnableMockData_MainTsShouldImportSetupMockData()
    {
        var request = CreateRequest(
            projectName: "MyAppMock",
            ns: "MyAppMock",
            orm: OrmProvider.EFCore,
            uiLibrary: UiLibrary.ElementPlus,
            mockData: true);

        var previewRequest = new PreviewFileRequest(
            request,
            "src/MyAppMock.Web/src/main.ts");

        var response = await _client.PostAsJsonAsync("/api/v1/scaffolds/preview-file", previewRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var preview = await response.Content.ReadFromJsonAsync<PreviewFileResponse>();
        preview.Should().NotBeNull();
        preview!.Content.Should().Contain("import { setupMockData } from './mock'");
        preview.Content.Should().Contain("setupMockData()");
    }

    [Fact]
    public async Task PreviewTree_AllUiLibraries_ShouldIncludeProviderSpecificFiles()
    {
        var expectedFiles = new Dictionary<UiLibrary, string>
        {
            { UiLibrary.ShadcnVue, "tailwind.config.js" },
            { UiLibrary.MateChat, "ChatLayout.vue" }
        };

        foreach (var uiLibrary in Enum.GetValues<UiLibrary>())
        {
            var request = CreateRequest(
                projectName: $"MyApp{uiLibrary}",
                ns: $"MyApp{uiLibrary}",
                orm: OrmProvider.EFCore,
                uiLibrary: uiLibrary);

            var treeRequest = new PreviewTreeRequest(request);
            var response = await _client.PostAsJsonAsync("/api/v1/scaffolds/preview-tree", treeRequest);

            response.StatusCode.Should().Be(HttpStatusCode.OK, $"UI={uiLibrary} should build tree");

            var result = await response.Content.ReadFromJsonAsync<PreviewTreeResponse>();
            result.Should().NotBeNull();

            if (expectedFiles.TryGetValue(uiLibrary, out var expectedFile))
            {
                FlattenNames(result!.Tree).Should().Contain(expectedFile);
            }
        }
    }

    private static IEnumerable<string> FlattenNames(IEnumerable<FileTreeNodeDto> nodes)
    {
        foreach (var node in nodes)
        {
            yield return node.Name;
            if (node.Children is { Count: > 0 })
            {
                foreach (var child in FlattenNames(node.Children))
                {
                    yield return child;
                }
            }
        }
    }

    private static IEnumerable<string> FlattenPaths(IEnumerable<FileTreeNodeDto> nodes)
    {
        foreach (var node in nodes)
        {
            yield return node.Path;
            if (node.Children is { Count: > 0 })
            {
                foreach (var child in FlattenPaths(node.Children))
                {
                    yield return child;
                }
            }
        }
    }
}

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing ITemplateFileProvider registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(ITemplateFileProvider));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Re-register with correct path for test environment
            // Templates are copied to output directory via .csproj config
            var templatesPath = FindTemplatesPath();
            services.AddScoped<ITemplateFileProvider>(_ =>
                new FileSystemTemplateProvider(templatesPath));
        });
    }

    private static string FindTemplatesPath()
    {
        // First: check if templates exist in AppContext.BaseDirectory (runtime scenario)
        var baseDir = AppContext.BaseDirectory;
        var baseDirTemplates = Path.Combine(baseDir, "templates");
        if (Directory.Exists(baseDirTemplates))
        {
            return baseDirTemplates;
        }

        // Fallback: search upward from current directory (development scenario)
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null)
        {
            var templatesPath = Path.Combine(dir.FullName, "apps", "api", "ScaffoldGenerator.Api", "templates");
            if (Directory.Exists(templatesPath))
            {
                return templatesPath;
            }
            dir = dir.Parent;
        }

        throw new InvalidOperationException(
            $"Could not find templates directory. BaseDirectory: {baseDir}, CurrentDirectory: {Directory.GetCurrentDirectory()}");
    }
}
