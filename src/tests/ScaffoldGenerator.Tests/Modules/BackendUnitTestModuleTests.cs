using FluentAssertions;
using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Application.Modules;
using ScaffoldGenerator.Contracts.Enums;
using ScaffoldGenerator.Contracts.Requests;
using Xunit;

namespace ScaffoldGenerator.Tests.Modules;

public class BackendUnitTestModuleTests
{
    private readonly BackendUnitTestModule _module = new();

    [Fact]
    public void IsEnabled_None_ReturnsFalse()
    {
        var request = CreateRequest(BackendUnitTestFramework.None);
        _module.IsEnabled(request).Should().BeFalse();
    }

    [Theory]
    [InlineData(BackendUnitTestFramework.xUnit)]
    [InlineData(BackendUnitTestFramework.NUnit)]
    [InlineData(BackendUnitTestFramework.MSTest)]
    public void IsEnabled_NonNone_ReturnsTrue(BackendUnitTestFramework fw)
    {
        var request = CreateRequest(fw);
        _module.IsEnabled(request).Should().BeTrue();
    }

    [Fact]
    public async Task ContributeAsync_xUnit_AddsTestProject()
    {
        var plan = new ScaffoldPlan();
        var request = CreateRequest(BackendUnitTestFramework.xUnit);

        await _module.ContributeAsync(plan, request, default);

        plan.Files.Should().Contain(f =>
            f.OutputPath.Contains("UnitTests.csproj"));
    }

    private static GenerateScaffoldRequest CreateRequest(
        BackendUnitTestFramework fw) => new()
    {
        Basic = new BasicOptions
        {
            ProjectName = "TestProject",
            Namespace = "TestProject"
        },
        Backend = new BackendOptions { UnitTestFramework = fw }
    };
}
