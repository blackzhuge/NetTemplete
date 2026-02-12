using FluentAssertions;
using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Application.Modules;
using ScaffoldGenerator.Contracts.Enums;
using ScaffoldGenerator.Contracts.Requests;
using Xunit;

namespace ScaffoldGenerator.Tests.Modules;

public class BackendIntegrationTestModuleTests
{
    private readonly BackendIntegrationTestModule _module = new();

    [Fact]
    public void IsEnabled_None_ReturnsFalse()
    {
        var request = CreateRequest(BackendIntegrationTestFramework.None);
        _module.IsEnabled(request).Should().BeFalse();
    }

    [Fact]
    public void IsEnabled_xUnit_ReturnsTrue()
    {
        var request = CreateRequest(BackendIntegrationTestFramework.xUnit);
        _module.IsEnabled(request).Should().BeTrue();
    }

    [Fact]
    public async Task ContributeAsync_xUnit_AddsIntegrationTestProject()
    {
        var plan = new ScaffoldPlan();
        var request = CreateRequest(BackendIntegrationTestFramework.xUnit);

        await _module.ContributeAsync(plan, request, default);

        plan.Files.Should().Contain(f =>
            f.OutputPath.Contains("IntegrationTests.csproj"));
    }

    [Fact]
    public void ContributeAsync_None_IsNotCalled()
    {
        var request = CreateRequest(BackendIntegrationTestFramework.None);
        _module.IsEnabled(request).Should().BeFalse();
    }

    private static GenerateScaffoldRequest CreateRequest(
        BackendIntegrationTestFramework fw) => new()
    {
        Basic = new BasicOptions
        {
            ProjectName = "TestProject",
            Namespace = "TestProject"
        },
        Backend = new BackendOptions
        {
            IntegrationTestFramework = fw
        }
    };
}
