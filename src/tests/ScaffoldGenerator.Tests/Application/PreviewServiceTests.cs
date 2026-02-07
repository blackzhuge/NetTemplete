using FluentAssertions;
using NSubstitute;
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
        var templateRenderer = Substitute.For<ITemplateRenderer>();
        var modules = Enumerable.Empty<IScaffoldModule>();
        var planBuilder = new ScaffoldPlanBuilder(modules, templateRenderer);
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
        var templateRenderer = Substitute.For<ITemplateRenderer>();
        templateRenderer
            .RenderAsync(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns("rendered content");

        var module = Substitute.For<IScaffoldModule>();
        module.IsEnabled(Arg.Any<GenerateScaffoldRequest>()).Returns(true);
        module.Order.Returns(0);
        module.When(m => m.ContributeAsync(Arg.Any<ScaffoldPlan>(), Arg.Any<GenerateScaffoldRequest>(), Arg.Any<CancellationToken>()))
            .Do(callInfo =>
            {
                var plan = callInfo.Arg<ScaffoldPlan>();
                plan.AddTemplateFile("template.sbn", "src/TestProject.Api/Program.cs", new { });
            });

        var planBuilder = new ScaffoldPlanBuilder(new[] { module }, templateRenderer);
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
        var templateRenderer = Substitute.For<ITemplateRenderer>();

        var module = Substitute.For<IScaffoldModule>();
        module.IsEnabled(Arg.Any<GenerateScaffoldRequest>()).Returns(true);
        module.Order.Returns(0);
        module.When(m => m.ContributeAsync(Arg.Any<ScaffoldPlan>(), Arg.Any<GenerateScaffoldRequest>(), Arg.Any<CancellationToken>()))
            .Do(callInfo =>
            {
                var plan = callInfo.Arg<ScaffoldPlan>();
                plan.AddFile("config.json", "{\"key\": \"value\"}");
            });

        var planBuilder = new ScaffoldPlanBuilder(new[] { module }, templateRenderer);
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
        var templateRenderer = Substitute.For<ITemplateRenderer>();

        var module = Substitute.For<IScaffoldModule>();
        module.IsEnabled(Arg.Any<GenerateScaffoldRequest>()).Returns(true);
        module.Order.Returns(0);
        module.When(m => m.ContributeAsync(Arg.Any<ScaffoldPlan>(), Arg.Any<GenerateScaffoldRequest>(), Arg.Any<CancellationToken>()))
            .Do(callInfo =>
            {
                var plan = callInfo.Arg<ScaffoldPlan>();
                plan.AddFile("README.md", "# Hello");
            });

        var planBuilder = new ScaffoldPlanBuilder(new[] { module }, templateRenderer);
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
