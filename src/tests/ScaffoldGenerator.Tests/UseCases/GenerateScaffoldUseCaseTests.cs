using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Application.UseCases;
using ScaffoldGenerator.Application.Validators;
using ScaffoldGenerator.Contracts.Enums;
using ScaffoldGenerator.Contracts.Requests;
using Xunit;

namespace ScaffoldGenerator.Tests.UseCases;

public class GenerateScaffoldUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_InvalidRequest_ReturnsErrorResult()
    {
        // Arrange - use real validator
        var validator = new GenerateScaffoldValidator();
        var templateRenderer = Substitute.For<ITemplateRenderer>();
        var modules = Enumerable.Empty<IScaffoldModule>();
        var planBuilder = new ScaffoldPlanBuilder(modules, templateRenderer);
        var zipBuilder = Substitute.For<IZipBuilder>();

        var useCase = new GenerateScaffoldUseCase(planBuilder, zipBuilder, validator);

        var request = new GenerateScaffoldRequest
        {
            ProjectName = "", // Invalid - empty
            Namespace = "Test"
        };

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
        var templateRenderer = Substitute.For<ITemplateRenderer>();
        var modules = Enumerable.Empty<IScaffoldModule>();
        var planBuilder = new ScaffoldPlanBuilder(modules, templateRenderer);
        var zipBuilder = Substitute.For<IZipBuilder>();
        zipBuilder.Build().Returns(new byte[] { 0x50, 0x4B, 0x03, 0x04 });

        var useCase = new GenerateScaffoldUseCase(planBuilder, zipBuilder, validator);

        var request = new GenerateScaffoldRequest
        {
            ProjectName = "TestProject",
            Namespace = "TestProject"
        };

        // Act
        var result = await useCase.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.FileName.Should().Be("TestProject.zip");
        zipBuilder.Received(1).Reset();
        zipBuilder.Received(1).Build();
    }

    [Fact]
    public async Task ExecuteAsync_ResetsZipBuilder_BeforeGeneration()
    {
        // Arrange
        var validator = new GenerateScaffoldValidator();
        var templateRenderer = Substitute.For<ITemplateRenderer>();
        var modules = Enumerable.Empty<IScaffoldModule>();
        var planBuilder = new ScaffoldPlanBuilder(modules, templateRenderer);
        var zipBuilder = Substitute.For<IZipBuilder>();
        zipBuilder.Build().Returns(new byte[] { 0x50, 0x4B });

        var useCase = new GenerateScaffoldUseCase(planBuilder, zipBuilder, validator);

        var request = new GenerateScaffoldRequest
        {
            ProjectName = "Test",
            Namespace = "Test"
        };

        // Act
        await useCase.ExecuteAsync(request);

        // Assert
        zipBuilder.Received(1).Reset();
    }

    [Fact]
    public async Task ExecuteAsync_InvalidNamespace_ReturnsError()
    {
        // Arrange
        var validator = new GenerateScaffoldValidator();
        var templateRenderer = Substitute.For<ITemplateRenderer>();
        var planBuilder = new ScaffoldPlanBuilder([], templateRenderer);
        var zipBuilder = Substitute.For<IZipBuilder>();

        var useCase = new GenerateScaffoldUseCase(planBuilder, zipBuilder, validator);

        var request = new GenerateScaffoldRequest
        {
            ProjectName = "Test",
            Namespace = "123-invalid" // Invalid namespace
        };

        // Act
        var result = await useCase.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeFalse();
    }
}
