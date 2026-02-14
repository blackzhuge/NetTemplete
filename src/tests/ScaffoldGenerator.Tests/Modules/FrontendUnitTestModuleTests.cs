using FluentAssertions;
using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Application.Modules;
using ScaffoldGenerator.Contracts.Enums;
using ScaffoldGenerator.Contracts.Requests;
using Xunit;

namespace ScaffoldGenerator.Tests.Modules;

public class FrontendUnitTestModuleTests
{
    private readonly FrontendUnitTestModule _module = new();

    [Fact]
    public void IsEnabled_None_ReturnsFalse()
    {
        var request = CreateRequest(FrontendUnitTestFramework.None);
        _module.IsEnabled(request).Should().BeFalse();
    }

    [Fact]
    public void IsEnabled_Vitest_ReturnsTrue()
    {
        var request = CreateRequest(FrontendUnitTestFramework.Vitest);
        _module.IsEnabled(request).Should().BeTrue();
    }

    [Fact]
    public async Task ContributeAsync_Vitest_AddsConfigAndPackages()
    {
        var plan = new ScaffoldPlan();
        var request = CreateRequest(FrontendUnitTestFramework.Vitest);

        await _module.ContributeAsync(plan, request, default);

        plan.Files.Should().Contain(f =>
            f.OutputPath.Contains("vitest.config.ts"));
        plan.NpmPackages.Should().Contain(p => p.Name == "vitest");
        plan.NpmPackages.Should().Contain(p => p.Name == "@vue/test-utils");
        plan.NpmPackages.Should().Contain(p => p.Name == "jsdom");
    }

    [Fact]
    public async Task ContributeAsync_Vitest_OutputPathStartsWithSrcPrefix()
    {
        var plan = new ScaffoldPlan();
        var request = CreateRequest(FrontendUnitTestFramework.Vitest);

        await _module.ContributeAsync(plan, request, default);

        plan.Files.Should().Contain(f =>
            f.OutputPath == "src/TestProject.Web/vitest.config.ts");
    }

    private static GenerateScaffoldRequest CreateRequest(
        FrontendUnitTestFramework fw) => new()
    {
        Basic = new BasicOptions
        {
            ProjectName = "TestProject",
            Namespace = "TestProject"
        },
        Frontend = new FrontendOptions
        {
            UnitTestFramework = fw
        }
    };
}
