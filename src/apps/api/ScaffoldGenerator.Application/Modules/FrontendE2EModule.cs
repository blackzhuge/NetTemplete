using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Contracts.Enums;
using ScaffoldGenerator.Contracts.Packages;
using ScaffoldGenerator.Contracts.Requests;

namespace ScaffoldGenerator.Application.Modules;

public sealed class FrontendE2EModule : IScaffoldModule
{
    public string Name => "FrontendE2E";
    public int Order => 95;

    public bool IsEnabled(GenerateScaffoldRequest request)
        => request.Frontend.E2EFramework != FrontendE2EFramework.None;

    public Task ContributeAsync(ScaffoldPlan plan, GenerateScaffoldRequest request, CancellationToken ct = default)
    {
        var projectName = request.Basic.ProjectName;
        var framework = request.Frontend.E2EFramework;

        if (framework == FrontendE2EFramework.Playwright)
        {
            plan.AddTemplateFile(
                "frontend/tests/e2e/playwright/playwright.config.ts.sbn",
                $"{projectName}.Web/playwright.config.ts",
                new { ProjectName = projectName });

            plan.AddNpmPackage(new PackageReference("@playwright/test", "^1.49.0"));
        }
        else if (framework == FrontendE2EFramework.Cypress)
        {
            plan.AddTemplateFile(
                "frontend/tests/e2e/cypress/cypress.config.ts.sbn",
                $"{projectName}.Web/cypress.config.ts",
                new { ProjectName = projectName });

            plan.AddNpmPackage(new PackageReference("cypress", "^13.0.0"));
        }

        return Task.CompletedTask;
    }
}
