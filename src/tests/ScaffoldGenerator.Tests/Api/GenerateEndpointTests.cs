using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ScaffoldGenerator.Contracts.Enums;
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

    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Generate_ValidRequest_ReturnsZipFile()
    {
        var request = new GenerateScaffoldRequest
        {
            ProjectName = "TestProject",
            Namespace = "TestProject",
            Database = DatabaseProvider.SQLite,
            Cache = CacheProvider.None,
            EnableSwagger = true,
            EnableJwtAuth = true
        };

        var response = await _client.PostAsJsonAsync("/api/generate", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/zip");

        var content = await response.Content.ReadAsByteArrayAsync();
        content.Should().NotBeEmpty();
        content[0].Should().Be(0x50);
        content[1].Should().Be(0x4B);
    }

    [Fact]
    public async Task Generate_EmptyProjectName_ReturnsBadRequest()
    {
        var request = new GenerateScaffoldRequest
        {
            ProjectName = "",
            Namespace = "Test"
        };

        var response = await _client.PostAsJsonAsync("/api/generate", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Generate_InvalidNamespace_ReturnsBadRequest()
    {
        var request = new GenerateScaffoldRequest
        {
            ProjectName = "Test",
            Namespace = "123-invalid"
        };

        var response = await _client.PostAsJsonAsync("/api/generate", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Generate_AllDatabaseProviders_Succeeds()
    {
        foreach (var provider in Enum.GetValues<DatabaseProvider>())
        {
            var request = new GenerateScaffoldRequest
            {
                ProjectName = $"Test{provider}",
                Namespace = $"Test{provider}",
                Database = provider
            };

            var response = await _client.PostAsJsonAsync("/api/generate", request);
            response.StatusCode.Should().Be(HttpStatusCode.OK,
                $"Database provider {provider} should work");
        }
    }

    [Fact]
    public async Task Generate_AllCacheProviders_Succeeds()
    {
        foreach (var provider in Enum.GetValues<CacheProvider>())
        {
            var request = new GenerateScaffoldRequest
            {
                ProjectName = $"TestCache{provider}",
                Namespace = $"TestCache{provider}",
                Cache = provider
            };

            var response = await _client.PostAsJsonAsync("/api/generate", request);
            response.StatusCode.Should().Be(HttpStatusCode.OK,
                $"Cache provider {provider} should work");
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
