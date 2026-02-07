using FluentAssertions;
using ScaffoldGenerator.Application.Validators;
using ScaffoldGenerator.Contracts.Enums;
using ScaffoldGenerator.Contracts.Preview;
using ScaffoldGenerator.Contracts.Requests;
using Xunit;

namespace ScaffoldGenerator.Tests.Application;

public class PreviewFileRequestValidatorTests
{
    private readonly PreviewFileRequestValidator _validator = new();

    private static GenerateScaffoldRequest CreateValidConfig()
    {
        return new GenerateScaffoldRequest
        {
            Basic = new BasicOptions { ProjectName = "Test", Namespace = "Test" },
            Backend = new BackendOptions { Database = DatabaseProvider.SQLite },
            Frontend = new FrontendOptions { RouterMode = RouterMode.Hash }
        };
    }

    [Fact]
    public void Validate_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new PreviewFileRequest(CreateValidConfig(), "src/Program.cs");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyOutputPath_ReturnsError()
    {
        // Arrange
        var request = new PreviewFileRequest(CreateValidConfig(), "");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("输出路径不能为空"));
    }

    [Theory]
    [InlineData("../etc/passwd")]
    [InlineData("src/../../../etc/passwd")]
    [InlineData("..\\windows\\system32")]
    public void Validate_PathTraversal_ReturnsError(string maliciousPath)
    {
        // Arrange
        var request = new PreviewFileRequest(CreateValidConfig(), maliciousPath);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains(".."));
    }

    [Theory]
    [InlineData("/etc/passwd")]
    [InlineData("/var/log/app.log")]
    [InlineData("C:\\Windows\\System32\\config")]
    [InlineData("D:\\Projects\\secret.txt")]
    public void Validate_AbsolutePath_ReturnsError(string absolutePath)
    {
        // Arrange
        var request = new PreviewFileRequest(CreateValidConfig(), absolutePath);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("绝对路径"));
    }

    [Theory]
    [InlineData("src\\Program.cs")]
    [InlineData("frontend\\src\\App.vue")]
    public void Validate_BackslashPath_ReturnsError(string backslashPath)
    {
        // Arrange
        var request = new PreviewFileRequest(CreateValidConfig(), backslashPath);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("反斜杠"));
    }

    [Theory]
    [InlineData("src/api/Program.cs")]
    [InlineData("frontend/src/App.vue")]
    [InlineData("README.md")]
    [InlineData("tests/unit/test.spec.ts")]
    public void Validate_ValidPaths_ReturnsSuccess(string validPath)
    {
        // Arrange
        var request = new PreviewFileRequest(CreateValidConfig(), validPath);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
