using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Contracts.Enums;
using ScaffoldGenerator.Contracts.Requests;

namespace ScaffoldGenerator.Application.Modules;

public sealed class ArchitectureModule : IScaffoldModule
{
    public string Name => "Architecture";
    public int Order => 5;

    public bool IsEnabled(GenerateScaffoldRequest request) => true;

    public Task ContributeAsync(ScaffoldPlan plan, GenerateScaffoldRequest request, CancellationToken ct = default)
    {
        var model = new
        {
            ProjectName = request.Basic.ProjectName,
            Namespace = request.Basic.Namespace
        };

        switch (request.Backend.Architecture)
        {
            case ArchitectureStyle.CleanArchitecture:
                AddCleanArchitectureFiles(plan, request, model);
                break;

            case ArchitectureStyle.VerticalSlice:
                AddVerticalSliceFiles(plan, request, model);
                break;

            case ArchitectureStyle.ModularMonolith:
                AddModularMonolithFiles(plan, request, model);
                break;

            case ArchitectureStyle.Simple:
            default:
                // Simple 架构不需要额外目录结构
                break;
        }

        return Task.CompletedTask;
    }

    private static void AddCleanArchitectureFiles(ScaffoldPlan plan, GenerateScaffoldRequest request, object model)
    {
        var projectName = request.Basic.ProjectName;

        // Application 层
        plan.AddTemplateFile(
            "backend/architecture/clean/Application.csproj.sbn",
            $"src/{projectName}.Application/{projectName}.Application.csproj",
            model);

        // Domain 层
        plan.AddTemplateFile(
            "backend/architecture/clean/Domain.csproj.sbn",
            $"src/{projectName}.Domain/{projectName}.Domain.csproj",
            model);

        // Infrastructure 层
        plan.AddTemplateFile(
            "backend/architecture/clean/Infrastructure.csproj.sbn",
            $"src/{projectName}.Infrastructure/{projectName}.Infrastructure.csproj",
            model);
    }

    private static void AddVerticalSliceFiles(ScaffoldPlan plan, GenerateScaffoldRequest request, object model)
    {
        var projectName = request.Basic.ProjectName;

        // Feature 示例结构
        plan.AddTemplateFile(
            "backend/architecture/vertical-slice/Feature.cs.sbn",
            $"src/{projectName}.Api/Features/Example/ExampleFeature.cs",
            model);
    }

    private static void AddModularMonolithFiles(ScaffoldPlan plan, GenerateScaffoldRequest request, object model)
    {
        var projectName = request.Basic.ProjectName;

        // 模块示例结构
        plan.AddTemplateFile(
            "backend/architecture/modular-monolith/Module.csproj.sbn",
            $"src/Modules/{projectName}.Core/{projectName}.Core.csproj",
            model);
    }
}
