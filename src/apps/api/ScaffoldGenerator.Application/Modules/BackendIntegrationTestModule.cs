using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Contracts.Enums;
using ScaffoldGenerator.Contracts.Requests;

namespace ScaffoldGenerator.Application.Modules;

public sealed class BackendIntegrationTestModule : IScaffoldModule
{
    public string Name => "BackendIntegrationTest";
    public int Order => 65;

    public bool IsEnabled(GenerateScaffoldRequest request)
        => request.Backend.IntegrationTestFramework != BackendIntegrationTestFramework.None;

    public Task ContributeAsync(ScaffoldPlan plan, GenerateScaffoldRequest request, CancellationToken ct = default)
    {
        var projectName = request.Basic.ProjectName;
        var ns = request.Basic.Namespace;

        var model = new
        {
            ProjectName = projectName,
            Namespace = ns
        };

        plan.AddTemplateFile(
            "backend/tests/integration/xunit/IntegrationTests.csproj.sbn",
            $"tests/{projectName}.IntegrationTests/{projectName}.IntegrationTests.csproj",
            model);

        return Task.CompletedTask;
    }
}
