using FluentAssertions;
using ScaffoldGenerator.Application.Presets;
using ScaffoldGenerator.Application.Validators;
using ScaffoldGenerator.Contracts.Enums;
using Xunit;

namespace ScaffoldGenerator.Tests.Application;

public class PresetServiceTests
{
    [Fact]
    public async Task GetPresetsAsync_ReturnsAllBuiltInPresets()
    {
        // Arrange
        var validator = new GenerateScaffoldValidator();
        var service = new PresetService(validator);

        // Act
        var response = await service.GetPresetsAsync();

        // Assert
        response.Should().NotBeNull();
        response.Version.Should().Be("1.0.0");
        response.Presets.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetPresetsAsync_ContainsMinimalPreset()
    {
        // Arrange
        var validator = new GenerateScaffoldValidator();
        var service = new PresetService(validator);

        // Act
        var response = await service.GetPresetsAsync();

        // Assert
        var minimal = response.Presets.FirstOrDefault(p => p.Id == "minimal");
        minimal.Should().NotBeNull();
        minimal!.Name.Should().Be("Minimal");
        minimal.IsDefault.Should().BeFalse();
        minimal.Config.Backend.JwtAuth.Should().BeFalse();
    }

    [Fact]
    public async Task GetPresetsAsync_ContainsStandardPreset_AsDefault()
    {
        // Arrange
        var validator = new GenerateScaffoldValidator();
        var service = new PresetService(validator);

        // Act
        var response = await service.GetPresetsAsync();

        // Assert
        var standard = response.Presets.FirstOrDefault(p => p.Id == "standard");
        standard.Should().NotBeNull();
        standard!.Name.Should().Be("Standard");
        standard.IsDefault.Should().BeTrue();
    }

    [Fact]
    public async Task GetPresetsAsync_ContainsEnterprisePreset()
    {
        // Arrange
        var validator = new GenerateScaffoldValidator();
        var service = new PresetService(validator);

        // Act
        var response = await service.GetPresetsAsync();

        // Assert
        var enterprise = response.Presets.FirstOrDefault(p => p.Id == "enterprise");
        enterprise.Should().NotBeNull();
        enterprise!.Name.Should().Be("Enterprise");
        enterprise.Config.Backend.Database.Should().Be(DatabaseProvider.MySQL);
        enterprise.Config.Backend.Cache.Should().Be(CacheProvider.Redis);
    }

    [Fact]
    public void ValidateAllPresets_WithValidPresets_DoesNotThrow()
    {
        // Arrange
        var validator = new GenerateScaffoldValidator();
        var service = new PresetService(validator);

        // Act & Assert
        var action = () => service.ValidateAllPresets();
        action.Should().NotThrow();
    }

    [Fact]
    public void AllBuiltInPresets_PassValidation()
    {
        // Arrange
        var validator = new GenerateScaffoldValidator();

        // Act & Assert
        foreach (var preset in BuiltInPresets.All)
        {
            var result = validator.Validate(preset.Config);
            result.IsValid.Should().BeTrue($"预设 '{preset.Id}' 应通过验证");
        }
    }

    [Fact]
    public void AllBuiltInPresets_HaveUniqueIds()
    {
        // Act
        var ids = BuiltInPresets.All.Select(p => p.Id).ToList();

        // Assert
        ids.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void AllBuiltInPresets_HaveOnlyOneDefault()
    {
        // Act
        var defaults = BuiltInPresets.All.Where(p => p.IsDefault).ToList();

        // Assert
        defaults.Should().HaveCount(1);
        defaults[0].Id.Should().Be("standard");
    }

    [Fact]
    public void MinimalPreset_HasNoTestFrameworks()
    {
        var minimal = BuiltInPresets.All.First(p => p.Id == "minimal");

        minimal.Config.Backend.UnitTestFramework.Should().Be(BackendUnitTestFramework.None);
        minimal.Config.Backend.IntegrationTestFramework.Should().Be(BackendIntegrationTestFramework.None);
        minimal.Config.Frontend.UnitTestFramework.Should().Be(FrontendUnitTestFramework.None);
        minimal.Config.Frontend.E2EFramework.Should().Be(FrontendE2EFramework.None);
    }

    [Fact]
    public void StandardPreset_HasBasicTestFrameworks()
    {
        var standard = BuiltInPresets.All.First(p => p.Id == "standard");

        standard.Config.Backend.UnitTestFramework.Should().Be(BackendUnitTestFramework.xUnit);
        standard.Config.Backend.IntegrationTestFramework.Should().Be(BackendIntegrationTestFramework.None);
        standard.Config.Frontend.UnitTestFramework.Should().Be(FrontendUnitTestFramework.Vitest);
        standard.Config.Frontend.E2EFramework.Should().Be(FrontendE2EFramework.None);
    }

    [Fact]
    public void EnterprisePreset_HasAllTestFrameworks()
    {
        var enterprise = BuiltInPresets.All.First(p => p.Id == "enterprise");

        enterprise.Config.Backend.UnitTestFramework.Should().Be(BackendUnitTestFramework.xUnit);
        enterprise.Config.Backend.IntegrationTestFramework.Should().Be(BackendIntegrationTestFramework.xUnit);
        enterprise.Config.Frontend.UnitTestFramework.Should().Be(FrontendUnitTestFramework.Vitest);
        enterprise.Config.Frontend.E2EFramework.Should().Be(FrontendE2EFramework.Playwright);
    }
}
