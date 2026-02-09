using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.Protected;
using ScaffoldGenerator.Infrastructure.Packages;
using Xunit;

namespace ScaffoldGenerator.Tests.Infrastructure;

public class NuGetSearchServiceTests
{
    private readonly IMemoryCache _cache;
    private const string IndexResponse = @"{
        ""resources"": [{
            ""@type"": ""SearchQueryService"",
            ""@id"": ""https://api.nuget.org/v3/query""
        }]
    }";

    public NuGetSearchServiceTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
    }

    private static Mock<HttpMessageHandler> CreateMockHandler(string searchResponse)
    {
        var callCount = 0;
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                callCount++;
                var content = callCount == 1 ? IndexResponse : searchResponse;
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(content)
                };
            });
        return mockHandler;
    }

    [Fact]
    public async Task SearchAsync_WithEmptyQuery_ReturnsEmptyResults()
    {
        // Arrange
        var mockHandler = CreateMockHandler(@"{""totalHits"":0,""data"":[]}");
        var httpClient = new HttpClient(mockHandler.Object);
        var service = new NuGetSearchService(httpClient, _cache);

        // Act
        var result = await service.SearchAsync("nonexistent-package-xyz", null, 20, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchAsync_WithValidQuery_ReturnsPackages()
    {
        // Arrange
        var mockHandler = CreateMockHandler(@"{
            ""totalHits"": 1,
            ""data"": [{
                ""id"": ""Newtonsoft.Json"",
                ""version"": ""13.0.3"",
                ""description"": ""Popular JSON framework"",
                ""iconUrl"": ""https://example.com/icon.png"",
                ""totalDownloads"": 1234567890
            }]
        }");
        var httpClient = new HttpClient(mockHandler.Object);
        var service = new NuGetSearchService(httpClient, _cache);

        // Act
        var result = await service.SearchAsync("Newtonsoft", null, 20, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Be("Newtonsoft.Json");
        result.Items[0].DownloadCount.Should().Be(1234567890);
        result.Items[0].LastUpdated.Should().BeNull();
    }

    [Fact]
    public async Task SearchAsync_WithMissingDownloads_ReturnsNullDownloadCount()
    {
        // Arrange
        var mockHandler = CreateMockHandler(@"{
            ""totalHits"": 1,
            ""data"": [{
                ""id"": ""SomePackage"",
                ""version"": ""1.0.0"",
                ""description"": ""Test package""
            }]
        }");
        var httpClient = new HttpClient(mockHandler.Object);
        var service = new NuGetSearchService(httpClient, _cache);

        // Act
        var result = await service.SearchAsync("SomePackage", null, 20, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].DownloadCount.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        // Arrange
        var httpClient = new HttpClient();
        var cache = new MemoryCache(new MemoryCacheOptions());

        // Act
        var service = new NuGetSearchService(httpClient, cache);

        // Assert
        service.Should().NotBeNull();
    }
}

public class NpmSearchServiceTests
{
    private readonly IMemoryCache _cache;

    public NpmSearchServiceTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
    }

    [Fact]
    public async Task SearchAsync_WithEmptyResults_ReturnsEmptyList()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("{\"total\":0,\"objects\":[]}")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var service = new NpmSearchService(httpClient, _cache);

        // Act
        var result = await service.SearchAsync("nonexistent-package-xyz", null, 20, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchAsync_WithValidQuery_ReturnsPackages()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""total"": 1,
                    ""objects"": [{
                        ""package"": {
                            ""name"": ""lodash"",
                            ""version"": ""4.17.21"",
                            ""description"": ""Lodash modular utilities"",
                            ""date"": ""2021-02-20T15:42:07.912Z""
                        },
                        ""downloads"": {
                            ""weekly"": 50000000
                        }
                    }]
                }")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var service = new NpmSearchService(httpClient, _cache);

        // Act
        var result = await service.SearchAsync("lodash", null, 20, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Be("lodash");
        result.Items[0].DownloadCount.Should().Be(50000000);
        result.Items[0].LastUpdated.Should().NotBeNull();
    }

    [Fact]
    public async Task SearchAsync_WithMissingDownloadsAndDate_ReturnsNullFields()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""total"": 1,
                    ""objects"": [{
                        ""package"": {
                            ""name"": ""some-package"",
                            ""version"": ""1.0.0"",
                            ""description"": ""Test package""
                        }
                    }]
                }")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var service = new NpmSearchService(httpClient, _cache);

        // Act
        var result = await service.SearchAsync("some-package", null, 20, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].DownloadCount.Should().BeNull();
        result.Items[0].LastUpdated.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        // Arrange
        var httpClient = new HttpClient();
        var cache = new MemoryCache(new MemoryCacheOptions());

        // Act
        var service = new NpmSearchService(httpClient, cache);

        // Assert
        service.Should().NotBeNull();
    }
}
