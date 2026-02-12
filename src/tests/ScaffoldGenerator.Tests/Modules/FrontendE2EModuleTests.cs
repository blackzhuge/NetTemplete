using FluentAssertions;
using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Application.Modules;
using ScaffoldGenerator.Contracts.Enums;
using ScaffoldGenerator.Contracts.Requests;
using Xunit;

namespace ScaffoldGenerator.Tests.Modules;

public class FrontendE2EModuleTests
{
    private readonly FrontendE2EModule _module = new();

    [Fact]
    public void IsEnabled_None_ReturnsFalse()
    {
        var request = CreateRequest(FrontendE2EFramework.None);
        _module.IsEnabled(request).Should().BeFalse();
    }

    [Theory]
    [InlineData(FrontendE2EFramework.Playwright)]
    [InlineData(FrontendE2EFramework.Cypress)]
    public void IsEnabled_NonNone_ReturnsTrue(FrontendE2EFramework fw)
    {
        var request = CreateRequest(fw);
        _module.IsEnabled(request).Should().BeTrue();
    }

    [Fact]
    public async Task ContributeAsync_Playwright_AddsConfigAndPackage()
    {
        var plan = new ScaffoldPlan();
        var request = CreateRequest(FrontendE2EFramework.Playwright);

        await _module.ContributeAsync(plan, request, default);

        plan.Files.Should().Contain(f =>
            f.OutputPath.Contains("playwright.config.ts"));
        plan.NpmPackages.Should().Contain(p =>
            p.Name == "@playwright/test");
    }

    [Fact]
    public async Task ContributeAsync_Cypress_AddsConfigAndPackage()
    {
        var plan = new ScaffoldPlan();
        var request = CreateRequest(FrontendE2EFramework.Cypress);

        await _module.ContributeAsync(plan, request, default);

        plan.Files.Should().Contain(f =>
            f.OutputPath.Contains("cypress.config.ts"));
        plan.NpmPackages.Should().Contain(p =>
            p.Name == "cypress");
    }

    private static GenerateScaffoldRequest CreateRequest(
        FrontendE2EFramework fw) => new()
    {
        Basic = new BasicOptions
        {
            ProjectName = "TestProject",
            Namespace = "TestProject"
        },
        Frontend = new FrontendOptions { E2EFramework = fw }
    };
}
