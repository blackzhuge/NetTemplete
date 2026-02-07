using FluentAssertions;
using Moq;
using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Application.Preview;
using ScaffoldGenerator.Contracts.Enums;
using ScaffoldGenerator.Contracts.Preview;
using ScaffoldGenerator.Contracts.Requests;
using Xunit;

namespace ScaffoldGenerator.Tests.Application;

public class PreviewServiceTests
{
    private static GenerateScaffoldRequest CreateRequest(
        string projectName = "TestProject",
        string ns = "TestProject")
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
                Database = DatabaseProvider.SQLite,
                Cache = CacheProvider.None,
                Swagger = true,
                JwtAuth = true
            },
            Frontend = new FrontendOptions
            {
                RouterMode = RouterMode.Hash,
                MockData = false
            }
        };
    }

    [Fact]
    public async Task PreviewFileAsync_FileNotFound_ReturnsNull()
    {
        // Arrange
        var templateRenderer = new Mock<ITemplateRenderer>();
        var modules = Enumerable.Empty<IScaffoldModule>();
        var planBuilder = new ScaffoldPlanBuilder(modules, templateRenderer.Object);
        var service = new PreviewService(planBuilder);

        var request = new PreviewFileRequest(
            CreateRequest(),
            "nonexistent/file.cs"
        );

        // Act
        var result = await service.PreviewFileAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task PreviewFileAsync_WithValidFile_ReturnsContent()
    {
        // Arrange
        var templateRenderer = new Mock<ITemplateRenderer>();
        templateRenderer
            .Setup(x => x.RenderAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("rendered content");

        var module = new Mock<IScaffoldModule>();
        module.Setup(m => m.IsEnabled(It.IsAny<GenerateScaffoldRequest>())).Returns(true);
        module.Setup(m => m.Order).Returns(0);
        module.Setup(m => m.ContributeAsync(It.IsAny<ScaffoldPlan>(), It.IsAny<GenerateScaffoldRequest>(), It.IsAny<CancellationToken>()))
            .Callback<ScaffoldPlan, GenerateScaffoldRequest, CancellationToken>((plan, _, _) =>
            {
                plan.AddTemplateFile("template.sbn", "src/TestProject.Api/Program.cs", new { });
            })
            .Returns(Task.CompletedTask);

        var planBuilder = new ScaffoldPlanBuilder(new[] { module.Object }, templateRenderer.Object);
        var service = new PreviewService(planBuilder);

        var request = new PreviewFileRequest(
            CreateRequest(),
            "src/TestProject.Api/Program.cs"
        );

        // Act
        var result = await service.PreviewFileAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.OutputPath.Should().Be("src/TestProject.Api/Program.cs");
        result.Content.Should().Be("rendered content");
        result.Language.Should().Be("csharp");
        result.IsTemplate.Should().BeTrue();
    }

    [Fact]
    public async Task PreviewFileAsync_StaticFile_ReturnsContentWithIsTemplateFalse()
    {
        // Arrange
        var templateRenderer = new Mock<ITemplateRenderer>();

        var module = new Mock<IScaffoldModule>();
        module.Setup(m => m.IsEnabled(It.IsAny<GenerateScaffoldRequest>())).Returns(true);
        module.Setup(m => m.Order).Returns(0);
        module.Setup(m => m.ContributeAsync(It.IsAny<ScaffoldPlan>(), It.IsAny<GenerateScaffoldRequest>(), It.IsAny<CancellationToken>()))
            .Callback<ScaffoldPlan, GenerateScaffoldRequest, CancellationToken>((plan, _, _) =>
            {
                plan.AddFile("config.json", "{\"key\": \"value\"}");
            })
            .Returns(Task.CompletedTask);

        var planBuilder = new ScaffoldPlanBuilder(new[] { module.Object }, templateRenderer.Object);
        var service = new PreviewService(planBuilder);

        var request = new PreviewFileRequest(
            CreateRequest(),
            "config.json"
        );

        // Act
        var result = await service.PreviewFileAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.OutputPath.Should().Be("config.json");
        result.Content.Should().Be("{\"key\": \"value\"}");
        result.Language.Should().Be("json");
        result.IsTemplate.Should().BeFalse();
    }

    [Fact]
    public async Task PreviewFileAsync_CaseInsensitivePathMatch()
    {
        // Arrange
        var templateRenderer = new Mock<ITemplateRenderer>();

        var module = new Mock<IScaffoldModule>();
        module.Setup(m => m.IsEnabled(It.IsAny<GenerateScaffoldRequest>())).Returns(true);
        module.Setup(m => m.Order).Returns(0);
        module.Setup(m => m.ContributeAsync(It.IsAny<ScaffoldPlan>(), It.IsAny<GenerateScaffoldRequest>(), It.IsAny<CancellationToken>()))
            .Callback<ScaffoldPlan, GenerateScaffoldRequest, CancellationToken>((plan, _, _) =>
            {
                plan.AddFile("README.md", "# Hello");
            })
            .Returns(Task.CompletedTask);

        var planBuilder = new ScaffoldPlanBuilder(new[] { module.Object }, templateRenderer.Object);
        var service = new PreviewService(planBuilder);

        var request = new PreviewFileRequest(
            CreateRequest(),
            "readme.md" // lowercase
        );

        // Act
        var result = await service.PreviewFileAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Language.Should().Be("markdown");
    }
}
