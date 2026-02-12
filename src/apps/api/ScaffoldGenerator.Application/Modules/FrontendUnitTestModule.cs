using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Contracts.Enums;
using ScaffoldGenerator.Contracts.Packages;
using ScaffoldGenerator.Contracts.Requests;

namespace ScaffoldGenerator.Application.Modules;

public sealed class FrontendUnitTestModule : IScaffoldModule
{
    public string Name => "FrontendUnitTest";
    public int Order => 90;

    public bool IsEnabled(GenerateScaffoldRequest request)
        => request.Frontend.UnitTestFramework != FrontendUnitTestFramework.None;

    public Task ContributeAsync(ScaffoldPlan plan, GenerateScaffoldRequest request, CancellationToken ct = default)
    {
        var projectName = request.Basic.ProjectName;

        plan.AddTemplateFile(
            "frontend/tests/unit/vitest/vitest.config.ts.sbn",
            $"{projectName}.Web/vitest.config.ts",
            new { ProjectName = projectName });

        plan.AddNpmPackage(new PackageReference("vitest", "^3.0.0"));
        plan.AddNpmPackage(new PackageReference("@vue/test-utils", "^2.4.0"));
        plan.AddNpmPackage(new PackageReference("jsdom", "^25.0.0"));
        plan.AddNpmPackage(new PackageReference("@vitest/coverage-v8", "^3.0.0"));

        return Task.CompletedTask;
    }
}
