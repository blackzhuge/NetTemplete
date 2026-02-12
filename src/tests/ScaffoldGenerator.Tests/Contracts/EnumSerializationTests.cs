using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using ScaffoldGenerator.Contracts.Enums;
using ScaffoldGenerator.Contracts.Requests;
using Xunit;

namespace ScaffoldGenerator.Tests.Contracts;

public class EnumSerializationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    [Theory]
    [InlineData(ArchitectureStyle.Simple, "\"Simple\"")]
    [InlineData(ArchitectureStyle.CleanArchitecture, "\"CleanArchitecture\"")]
    [InlineData(ArchitectureStyle.VerticalSlice, "\"VerticalSlice\"")]
    [InlineData(ArchitectureStyle.ModularMonolith, "\"ModularMonolith\"")]
    public void ArchitectureStyle_Serialization_ReturnsStringValue(ArchitectureStyle value, string expected)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        json.Should().Be(expected);
    }

    [Theory]
    [InlineData("\"Simple\"", ArchitectureStyle.Simple)]
    [InlineData("\"CleanArchitecture\"", ArchitectureStyle.CleanArchitecture)]
    public void ArchitectureStyle_Deserialization_ReturnsEnumValue(string json, ArchitectureStyle expected)
    {
        var result = JsonSerializer.Deserialize<ArchitectureStyle>(json, JsonOptions);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(OrmProvider.SqlSugar, "\"SqlSugar\"")]
    [InlineData(OrmProvider.EFCore, "\"EFCore\"")]
    [InlineData(OrmProvider.Dapper, "\"Dapper\"")]
    [InlineData(OrmProvider.FreeSql, "\"FreeSql\"")]
    public void OrmProvider_Serialization_ReturnsStringValue(OrmProvider value, string expected)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        json.Should().Be(expected);
    }

    [Theory]
    [InlineData(UiLibrary.ElementPlus, "\"ElementPlus\"")]
    [InlineData(UiLibrary.AntDesignVue, "\"AntDesignVue\"")]
    [InlineData(UiLibrary.NaiveUI, "\"NaiveUI\"")]
    [InlineData(UiLibrary.TailwindHeadless, "\"TailwindHeadless\"")]
    [InlineData(UiLibrary.ShadcnVue, "\"ShadcnVue\"")]
    [InlineData(UiLibrary.MateChat, "\"MateChat\"")]
    public void UiLibrary_Serialization_ReturnsStringValue(UiLibrary value, string expected)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        json.Should().Be(expected);
    }

    [Theory]
    [InlineData(BackendUnitTestFramework.None, "\"None\"")]
    [InlineData(BackendUnitTestFramework.xUnit, "\"xUnit\"")]
    [InlineData(BackendUnitTestFramework.NUnit, "\"NUnit\"")]
    [InlineData(BackendUnitTestFramework.MSTest, "\"MSTest\"")]
    public void BackendUnitTestFramework_Serialization_ReturnsStringValue(BackendUnitTestFramework value, string expected)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        json.Should().Be(expected);
    }

    [Theory]
    [InlineData("\"None\"", BackendUnitTestFramework.None)]
    [InlineData("\"xUnit\"", BackendUnitTestFramework.xUnit)]
    [InlineData("\"NUnit\"", BackendUnitTestFramework.NUnit)]
    [InlineData("\"MSTest\"", BackendUnitTestFramework.MSTest)]
    public void BackendUnitTestFramework_Deserialization_ReturnsEnumValue(string json, BackendUnitTestFramework expected)
    {
        var result = JsonSerializer.Deserialize<BackendUnitTestFramework>(json, JsonOptions);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(BackendIntegrationTestFramework.None, "\"None\"")]
    [InlineData(BackendIntegrationTestFramework.xUnit, "\"xUnit\"")]
    public void BackendIntegrationTestFramework_Serialization_ReturnsStringValue(BackendIntegrationTestFramework value, string expected)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        json.Should().Be(expected);
    }

    [Theory]
    [InlineData(FrontendUnitTestFramework.None, "\"None\"")]
    [InlineData(FrontendUnitTestFramework.Vitest, "\"Vitest\"")]
    public void FrontendUnitTestFramework_Serialization_ReturnsStringValue(FrontendUnitTestFramework value, string expected)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        json.Should().Be(expected);
    }

    [Theory]
    [InlineData(FrontendE2EFramework.None, "\"None\"")]
    [InlineData(FrontendE2EFramework.Playwright, "\"Playwright\"")]
    [InlineData(FrontendE2EFramework.Cypress, "\"Cypress\"")]
    public void FrontendE2EFramework_Serialization_ReturnsStringValue(FrontendE2EFramework value, string expected)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        json.Should().Be(expected);
    }

    [Fact]
    public void BackendOptions_DefaultValues_AreCorrect()
    {
        var options = new BackendOptions();

        options.Architecture.Should().Be(ArchitectureStyle.Simple);
        options.Orm.Should().Be(OrmProvider.SqlSugar);
        options.UnitTestFramework.Should().Be(BackendUnitTestFramework.None);
        options.IntegrationTestFramework.Should().Be(BackendIntegrationTestFramework.None);
    }

    [Fact]
    public void FrontendOptions_DefaultValues_AreCorrect()
    {
        var options = new FrontendOptions();

        options.UiLibrary.Should().Be(UiLibrary.ElementPlus);
        options.UnitTestFramework.Should().Be(FrontendUnitTestFramework.None);
        options.E2EFramework.Should().Be(FrontendE2EFramework.None);
    }

    [Fact]
    public void GenerateScaffoldRequest_BackwardCompatibility_OldFormatDeserializesWithDefaults()
    {
        // 旧版请求格式（无新字段）
        var oldFormatJson = """
        {
            "basic": {
                "projectName": "TestProject",
                "namespace": "TestProject"
            },
            "backend": {
                "database": "SQLite",
                "swagger": true
            },
            "frontend": {
                "routerMode": "Hash"
            }
        }
        """;

        var request = JsonSerializer.Deserialize<GenerateScaffoldRequest>(oldFormatJson, JsonOptions);

        request.Should().NotBeNull();
        request!.Backend.Architecture.Should().Be(ArchitectureStyle.Simple);
        request.Backend.Orm.Should().Be(OrmProvider.SqlSugar);
        request.Backend.UnitTestFramework.Should().Be(BackendUnitTestFramework.None);
        request.Backend.IntegrationTestFramework.Should().Be(BackendIntegrationTestFramework.None);
        request.Frontend.UiLibrary.Should().Be(UiLibrary.ElementPlus);
        request.Frontend.UnitTestFramework.Should().Be(FrontendUnitTestFramework.None);
        request.Frontend.E2EFramework.Should().Be(FrontendE2EFramework.None);
    }

    [Fact]
    public void GenerateScaffoldRequest_NewFormat_DeserializesCorrectly()
    {
        var newFormatJson = """
        {
            "basic": {
                "projectName": "TestProject",
                "namespace": "TestProject"
            },
            "backend": {
                "architecture": "CleanArchitecture",
                "orm": "EFCore",
                "database": "MySQL"
            },
            "frontend": {
                "uiLibrary": "NaiveUI",
                "routerMode": "History"
            }
        }
        """;

        var request = JsonSerializer.Deserialize<GenerateScaffoldRequest>(newFormatJson, JsonOptions);

        request.Should().NotBeNull();
        request!.Backend.Architecture.Should().Be(ArchitectureStyle.CleanArchitecture);
        request.Backend.Orm.Should().Be(OrmProvider.EFCore);
        request.Frontend.UiLibrary.Should().Be(UiLibrary.NaiveUI);
    }

    [Fact]
    public void GenerateScaffoldRequest_WithTestingOptions_DeserializesCorrectly()
    {
        var json = """
        {
            "basic": {
                "projectName": "TestProject",
                "namespace": "TestProject"
            },
            "backend": {
                "architecture": "Simple",
                "unitTestFramework": "xUnit",
                "integrationTestFramework": "xUnit"
            },
            "frontend": {
                "unitTestFramework": "Vitest",
                "e2eFramework": "Playwright"
            }
        }
        """;

        var request = JsonSerializer.Deserialize<GenerateScaffoldRequest>(json, JsonOptions);

        request.Should().NotBeNull();
        request!.Backend.UnitTestFramework.Should().Be(BackendUnitTestFramework.xUnit);
        request.Backend.IntegrationTestFramework.Should().Be(BackendIntegrationTestFramework.xUnit);
        request.Frontend.UnitTestFramework.Should().Be(FrontendUnitTestFramework.Vitest);
        request.Frontend.E2EFramework.Should().Be(FrontendE2EFramework.Playwright);
    }
}
