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

    [Fact]
    public void BackendOptions_DefaultValues_AreCorrect()
    {
        var options = new BackendOptions();

        options.Architecture.Should().Be(ArchitectureStyle.Simple);
        options.Orm.Should().Be(OrmProvider.SqlSugar);
    }

    [Fact]
    public void FrontendOptions_DefaultValues_AreCorrect()
    {
        var options = new FrontendOptions();

        options.UiLibrary.Should().Be(UiLibrary.ElementPlus);
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
        request.Frontend.UiLibrary.Should().Be(UiLibrary.ElementPlus);
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
}
