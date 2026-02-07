using FluentAssertions;
using ScaffoldGenerator.Application.Preview;
using Xunit;

namespace ScaffoldGenerator.Tests.Application;

public class LanguageMapperTests
{
    [Theory]
    [InlineData("Program.cs", "csharp")]
    [InlineData("app.ts", "typescript")]
    [InlineData("Component.vue", "vue")]
    [InlineData("config.json", "json")]
    [InlineData("web.config.xml", "xml")]
    [InlineData("Project.csproj", "xml")]
    [InlineData("index.html", "html")]
    [InlineData("styles.css", "css")]
    [InlineData("styles.scss", "scss")]
    [InlineData("README.md", "markdown")]
    [InlineData("script.js", "javascript")]
    [InlineData("Component.tsx", "tsx")]
    [InlineData("Component.jsx", "jsx")]
    [InlineData("config.yaml", "yaml")]
    [InlineData("config.yml", "yaml")]
    public void GetLanguage_KnownExtensions_ReturnsCorrectLanguage(string filePath, string expectedLanguage)
    {
        // Act
        var result = LanguageMapper.GetLanguage(filePath);

        // Assert
        result.Should().Be(expectedLanguage);
    }

    [Theory]
    [InlineData("file.unknown")]
    [InlineData("file.xyz")]
    [InlineData("file")]
    [InlineData(".gitignore")]
    public void GetLanguage_UnknownExtensions_ReturnsPlaintext(string filePath)
    {
        // Act
        var result = LanguageMapper.GetLanguage(filePath);

        // Assert
        result.Should().Be("plaintext");
    }

    [Theory]
    [InlineData("PROGRAM.CS", "csharp")]
    [InlineData("App.TS", "typescript")]
    [InlineData("CONFIG.JSON", "json")]
    public void GetLanguage_CaseInsensitive(string filePath, string expectedLanguage)
    {
        // Act
        var result = LanguageMapper.GetLanguage(filePath);

        // Assert
        result.Should().Be(expectedLanguage);
    }

    [Theory]
    [InlineData("src/api/Program.cs", "csharp")]
    [InlineData("frontend/src/App.vue", "vue")]
    [InlineData("tests/unit/test.spec.ts", "typescript")]
    public void GetLanguage_WithPath_ExtractsExtensionCorrectly(string filePath, string expectedLanguage)
    {
        // Act
        var result = LanguageMapper.GetLanguage(filePath);

        // Assert
        result.Should().Be(expectedLanguage);
    }

    [Fact]
    public void GetLanguage_EmptyString_ReturnsPlaintext()
    {
        // Act
        var result = LanguageMapper.GetLanguage("");

        // Assert
        result.Should().Be("plaintext");
    }
}
