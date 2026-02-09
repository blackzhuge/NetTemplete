using FluentAssertions;
using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Application.Modules;
using ScaffoldGenerator.Contracts.Enums;
using ScaffoldGenerator.Contracts.Requests;
using Xunit;

namespace ScaffoldGenerator.Tests.Modules;

public class ArchitectureModuleTests
{
    private readonly ArchitectureModule _module = new();

    [Fact]
    public void Name_ReturnsArchitecture()
    {
        _module.Name.Should().Be("Architecture");
    }

    [Fact]
    public void Order_ShouldBe5()
    {
        _module.Order.Should().Be(5);
    }

    [Fact]
    public void IsEnabled_Always_ReturnsTrue()
    {
        var request = CreateRequest();
        _module.IsEnabled(request).Should().BeTrue();
    }

    [Fact]
    public async Task ContributeAsync_SimpleArchitecture_AddsNoExtraFiles()
    {
        var plan = new ScaffoldPlan();
        var request = CreateRequest(ArchitectureStyle.Simple);

        await _module.ContributeAsync(plan, request, default);

        plan.Files.Should().BeEmpty();
    }

    [Fact]
    public async Task ContributeAsync_CleanArchitecture_AddsLayerProjects()
    {
        var plan = new ScaffoldPlan();
        var request = CreateRequest(ArchitectureStyle.CleanArchitecture);

        await _module.ContributeAsync(plan, request, default);

        plan.Files.Should().Contain(f => f.OutputPath.Contains(".Application.csproj"));
        plan.Files.Should().Contain(f => f.OutputPath.Contains(".Domain.csproj"));
        plan.Files.Should().Contain(f => f.OutputPath.Contains(".Infrastructure.csproj"));
    }

    [Fact]
    public async Task ContributeAsync_VerticalSlice_AddsFeatureFile()
    {
        var plan = new ScaffoldPlan();
        var request = CreateRequest(ArchitectureStyle.VerticalSlice);

        await _module.ContributeAsync(plan, request, default);

        plan.Files.Should().Contain(f => f.OutputPath.Contains("Features/Example"));
    }

    [Fact]
    public async Task ContributeAsync_ModularMonolith_AddsModuleProject()
    {
        var plan = new ScaffoldPlan();
        var request = CreateRequest(ArchitectureStyle.ModularMonolith);

        await _module.ContributeAsync(plan, request, default);

        plan.Files.Should().Contain(f => f.OutputPath.Contains("Modules/"));
    }

    private static GenerateScaffoldRequest CreateRequest(
        ArchitectureStyle architecture = ArchitectureStyle.Simple) => new()
    {
        Basic = new BasicOptions
        {
            ProjectName = "TestProject",
            Namespace = "TestProject"
        },
        Backend = new BackendOptions
        {
            Architecture = architecture
        }
    };
}
