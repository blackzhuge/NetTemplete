using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Contracts.Enums;
using ScaffoldGenerator.Contracts.Packages;
using ScaffoldGenerator.Contracts.Requests;

namespace ScaffoldGenerator.Application.Modules;

public sealed class BackendUnitTestModule : IScaffoldModule
{
    public string Name => "BackendUnitTest";
    public int Order => 60;

    public bool IsEnabled(GenerateScaffoldRequest request)
        => request.Backend.UnitTestFramework != BackendUnitTestFramework.None;

    public Task ContributeAsync(ScaffoldPlan plan, GenerateScaffoldRequest request, CancellationToken ct = default)
    {
        var framework = request.Backend.UnitTestFramework;
        var projectName = request.Basic.ProjectName;
        var ns = request.Basic.Namespace;

        var model = new
        {
            ProjectName = projectName,
            Namespace = ns
        };

        var templateDir = framework switch
        {
            BackendUnitTestFramework.xUnit => "backend/tests/unit/xunit",
            BackendUnitTestFramework.NUnit => "backend/tests/unit/nunit",
            BackendUnitTestFramework.MSTest => "backend/tests/unit/mstest",
            _ => throw new ArgumentOutOfRangeException()
        };

        plan.AddTemplateFile(
            $"{templateDir}/UnitTests.csproj.sbn",
            $"tests/{projectName}.UnitTests/{projectName}.UnitTests.csproj",
            model);

        return Task.CompletedTask;
    }
}
