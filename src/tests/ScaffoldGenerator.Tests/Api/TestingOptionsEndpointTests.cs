using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ScaffoldGenerator.Contracts.Enums;
using ScaffoldGenerator.Contracts.Preview;
using ScaffoldGenerator.Contracts.Requests;
using Xunit;

namespace ScaffoldGenerator.Tests.Api;

public class TestingOptionsEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TestingOptionsEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static GenerateScaffoldRequest CreateRequest(
        BackendUnitTestFramework unitTest = BackendUnitTestFramework.None,
        BackendIntegrationTestFramework integrationTest = BackendIntegrationTestFramework.None,
        FrontendUnitTestFramework frontendUnit = FrontendUnitTestFramework.None,
        FrontendE2EFramework e2e = FrontendE2EFramework.None)
    {
        return new GenerateScaffoldRequest
        {
            Basic = new BasicOptions
            {
                ProjectName = "TestApp",
                Namespace = "TestApp"
            },
            Backend = new BackendOptions
            {
                Orm = OrmProvider.SqlSugar,
                Database = DatabaseProvider.SQLite,
                Swagger = true,
                JwtAuth = false,
                UnitTestFramework = unitTest,
                IntegrationTestFramework = integrationTest
            },
            Frontend = new FrontendOptions
            {
                UiLibrary = UiLibrary.ElementPlus,
                RouterMode = RouterMode.Hash,
                MockData = false,
                UnitTestFramework = frontendUnit,
                E2EFramework = e2e
            }
        };
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

    private async Task<List<string>> GetTreePaths(GenerateScaffoldRequest config)
    {
        var request = new PreviewTreeRequest(config);
        var response = await _client.PostAsJsonAsync(
            "/api/v1/scaffolds/preview-tree", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tree = await response.Content.ReadFromJsonAsync<PreviewTreeResponse>();
        tree.Should().NotBeNull();
        return FlattenPaths(tree!.Tree).ToList();
    }

    [Fact]
    public async Task PreviewTree_AllNone_NoTestProjects()
    {
        var paths = await GetTreePaths(CreateRequest());

        paths.Should().NotContain(p => p.Contains("UnitTests"));
        paths.Should().NotContain(p => p.Contains("IntegrationTests"));
        paths.Should().NotContain(p => p.Contains("vitest.config"));
        paths.Should().NotContain(p => p.Contains("playwright.config"));
        paths.Should().NotContain(p => p.Contains("cypress.config"));
    }

    [Fact]
    public async Task PreviewTree_BackendUnitTest_xUnit_IncludesProject()
    {
        var paths = await GetTreePaths(
            CreateRequest(unitTest: BackendUnitTestFramework.xUnit));

        paths.Should().Contain(p =>
            p.Contains("TestApp.UnitTests.csproj"));
    }

    [Fact]
    public async Task PreviewTree_BackendIntegrationTest_IncludesProject()
    {
        var paths = await GetTreePaths(
            CreateRequest(integrationTest: BackendIntegrationTestFramework.xUnit));

        paths.Should().Contain(p =>
            p.Contains("TestApp.IntegrationTests.csproj"));
    }

    [Fact]
    public async Task PreviewTree_FrontendVitest_IncludesConfig()
    {
        var paths = await GetTreePaths(
            CreateRequest(frontendUnit: FrontendUnitTestFramework.Vitest));

        paths.Should().Contain(p =>
            p.Contains("vitest.config.ts"));
    }

    [Fact]
    public async Task PreviewTree_FrontendPlaywright_IncludesConfig()
    {
        var paths = await GetTreePaths(
            CreateRequest(e2e: FrontendE2EFramework.Playwright));

        paths.Should().Contain(p =>
            p.Contains("playwright.config.ts"));
    }

    [Fact]
    public async Task PreviewTree_FrontendCypress_IncludesConfig()
    {
        var paths = await GetTreePaths(
            CreateRequest(e2e: FrontendE2EFramework.Cypress));

        paths.Should().Contain(p =>
            p.Contains("cypress.config.ts"));
    }

    [Fact]
    public async Task PreviewTree_AllEnabled_IncludesAllTestFiles()
    {
        var paths = await GetTreePaths(CreateRequest(
            unitTest: BackendUnitTestFramework.xUnit,
            integrationTest: BackendIntegrationTestFramework.xUnit,
            frontendUnit: FrontendUnitTestFramework.Vitest,
            e2e: FrontendE2EFramework.Playwright));

        paths.Should().Contain(p =>
            p.Contains("TestApp.UnitTests.csproj"));
        paths.Should().Contain(p =>
            p.Contains("TestApp.IntegrationTests.csproj"));
        paths.Should().Contain(p =>
            p.Contains("vitest.config.ts"));
        paths.Should().Contain(p =>
            p.Contains("playwright.config.ts"));
    }
}