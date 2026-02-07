using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using ScaffoldGenerator.Infrastructure.Rendering;
using Xunit;

namespace ScaffoldGenerator.Tests.Infrastructure;

public class ScribanTemplateRendererTests
{
    private readonly Mock<ITemplateFileProvider> _fileProvider;
    private readonly IMemoryCache _cache;
    private readonly ScribanTemplateRenderer _renderer;

    public ScribanTemplateRendererTests()
    {
        _fileProvider = new Mock<ITemplateFileProvider>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _renderer = new ScribanTemplateRenderer(_fileProvider.Object, _cache);
    }

    [Fact]
    public async Task RenderAsync_SimpleTemplate_ReturnsRenderedContent()
    {
        // Arrange
        const string template = "Hello, {{ name }}!";
        _fileProvider.Setup(x => x.ReadTemplateAsync("test.sbn", It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        var model = new { Name = "World" };

        // Act
        var result = await _renderer.RenderAsync("test.sbn", model);

        // Assert
        result.Should().Be("Hello, World!");
    }

    [Fact]
    public async Task RenderAsync_TemplateWithLoop_RendersAllItems()
    {
        // Arrange
        const string template = "{{ for item in items }}{{ item }},{{ end }}";
        _fileProvider.Setup(x => x.ReadTemplateAsync("loop.sbn", It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        var model = new { Items = new[] { "A", "B", "C" } };

        // Act
        var result = await _renderer.RenderAsync("loop.sbn", model);

        // Assert
        result.Should().Be("A,B,C,");
    }

    [Fact]
    public async Task RenderAsync_TemplateWithConditional_RendersCorrectBranch()
    {
        // Arrange
        const string template = "{{ if enabled }}ON{{ else }}OFF{{ end }}";
        _fileProvider.Setup(x => x.ReadTemplateAsync("cond.sbn", It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        // Act
        var resultEnabled = await _renderer.RenderAsync("cond.sbn", new { Enabled = true });
        var resultDisabled = await _renderer.RenderAsync("cond.sbn", new { Enabled = false });

        // Assert
        resultEnabled.Should().Be("ON");
        resultDisabled.Should().Be("OFF");
    }

    [Fact]
    public async Task RenderAsync_TemplateWithNestedProperties_AccessesNestedValues()
    {
        // Arrange
        const string template = "{{ config.database.provider }}";
        _fileProvider.Setup(x => x.ReadTemplateAsync("nested.sbn", It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        var model = new
        {
            Config = new
            {
                Database = new { Provider = "MySQL" }
            }
        };

        // Act
        var result = await _renderer.RenderAsync("nested.sbn", model);

        // Assert
        result.Should().Be("MySQL");
    }

    [Fact]
    public async Task RenderAsync_InvalidTemplate_ThrowsException()
    {
        // Arrange
        const string invalidTemplate = "{{ if }}"; // Missing condition
        _fileProvider.Setup(x => x.ReadTemplateAsync("invalid.sbn", It.IsAny<CancellationToken>()))
            .ReturnsAsync(invalidTemplate);

        // Act
        var act = async () => await _renderer.RenderAsync("invalid.sbn", new { });

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*模板解析错误*");
    }

    [Fact]
    public async Task RenderAsync_ChineseContent_PreservesCharacters()
    {
        // Arrange
        const string template = "项目名称: {{ project_name }}";
        _fileProvider.Setup(x => x.ReadTemplateAsync("chinese.sbn", It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        var model = new { ProjectName = "测试项目" };

        // Act
        var result = await _renderer.RenderAsync("chinese.sbn", model);

        // Assert
        result.Should().Be("项目名称: 测试项目");
    }

    [Fact]
    public async Task RenderAsync_EmptyModel_RendersStaticContent()
    {
        // Arrange
        const string template = "Static content only";
        _fileProvider.Setup(x => x.ReadTemplateAsync("static.sbn", It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        // Act
        var result = await _renderer.RenderAsync("static.sbn", new { });

        // Assert
        result.Should().Be("Static content only");
    }
}
