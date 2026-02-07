using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Application.UseCases;
using ScaffoldGenerator.Application.Validators;
using ScaffoldGenerator.Contracts.Enums;
using ScaffoldGenerator.Contracts.Requests;
using Xunit;

namespace ScaffoldGenerator.Tests.UseCases;

public class GenerateScaffoldUseCaseTests
{
    private static GenerateScaffoldRequest CreateRequest(
        string projectName = "TestProject",
        string ns = "TestProject",
        DatabaseProvider database = DatabaseProvider.SQLite,
        CacheProvider cache = CacheProvider.None,
        bool swagger = true,
        bool jwtAuth = true)
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
                Database = database,
                Cache = cache,
                Swagger = swagger,
                JwtAuth = jwtAuth
            },
            Frontend = new FrontendOptions
            {
                RouterMode = RouterMode.Hash,
                MockData = false
            }
        };
    }

    [Fact]
    public async Task ExecuteAsync_InvalidRequest_ReturnsErrorResult()
    {
        // Arrange - use real validator
        var validator = new GenerateScaffoldValidator();
        var templateRenderer = new Mock<ITemplateRenderer>();
        var modules = Enumerable.Empty<IScaffoldModule>();
        var planBuilder = new ScaffoldPlanBuilder(modules, templateRenderer.Object);
        var zipBuilder = new Mock<IZipBuilder>();

        var useCase = new GenerateScaffoldUseCase(planBuilder, zipBuilder.Object, validator);

        var request = CreateRequest(projectName: "", ns: "Test");

        // Act
        var result = await useCase.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_ValidRequest_WithNoModules_ReturnsEmptyZip()
    {
        // Arrange
        var validator = new GenerateScaffoldValidator();
        var templateRenderer = new Mock<ITemplateRenderer>();
        var modules = Enumerable.Empty<IScaffoldModule>();
        var planBuilder = new ScaffoldPlanBuilder(modules, templateRenderer.Object);
        var zipBuilder = new Mock<IZipBuilder>();
        zipBuilder.Setup(x => x.Build()).Returns(new byte[] { 0x50, 0x4B, 0x03, 0x04 });

        var useCase = new GenerateScaffoldUseCase(planBuilder, zipBuilder.Object, validator);

        var request = CreateRequest();

        // Act
        var result = await useCase.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.FileName.Should().Be("TestProject.zip");
        zipBuilder.Verify(x => x.Reset(), Times.Once);
        zipBuilder.Verify(x => x.Build(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ResetsZipBuilder_BeforeGeneration()
    {
        // Arrange
        var validator = new GenerateScaffoldValidator();
        var templateRenderer = new Mock<ITemplateRenderer>();
        var modules = Enumerable.Empty<IScaffoldModule>();
        var planBuilder = new ScaffoldPlanBuilder(modules, templateRenderer.Object);
        var zipBuilder = new Mock<IZipBuilder>();
        zipBuilder.Setup(x => x.Build()).Returns(new byte[] { 0x50, 0x4B });

        var useCase = new GenerateScaffoldUseCase(planBuilder, zipBuilder.Object, validator);

        var request = CreateRequest(projectName: "Test", ns: "Test");

        // Act
        await useCase.ExecuteAsync(request);

        // Assert
        zipBuilder.Verify(x => x.Reset(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidNamespace_ReturnsError()
    {
        // Arrange
        var validator = new GenerateScaffoldValidator();
        var templateRenderer = new Mock<ITemplateRenderer>();
        var planBuilder = new ScaffoldPlanBuilder([], templateRenderer.Object);
        var zipBuilder = new Mock<IZipBuilder>();

        var useCase = new GenerateScaffoldUseCase(planBuilder, zipBuilder.Object, validator);

        var request = CreateRequest(projectName: "Test", ns: "123-invalid");

        // Act
        var result = await useCase.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeFalse();
    }
}
