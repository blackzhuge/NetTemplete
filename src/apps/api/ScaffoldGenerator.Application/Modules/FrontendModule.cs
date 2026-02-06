using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Contracts.Requests;

namespace ScaffoldGenerator.Application.Modules;

public sealed class FrontendModule : IScaffoldModule
{
    public string Name => "Frontend";
    public int Order => 50;

    public bool IsEnabled(GenerateScaffoldRequest request) => true;

    public Task ContributeAsync(ScaffoldPlan plan, GenerateScaffoldRequest request, CancellationToken ct = default)
    {
        var model = new
        {
            request.ProjectName,
            request.Namespace,
            RouterMode = request.RouterMode.ToString()
        };

        plan.AddTemplateFile("frontend/package.json.sbn", $"src/{request.ProjectName}.Web/package.json", model);
        plan.AddTemplateFile("frontend/vite.config.ts.sbn", $"src/{request.ProjectName}.Web/vite.config.ts", model);
        plan.AddTemplateFile("frontend/tsconfig.json.sbn", $"src/{request.ProjectName}.Web/tsconfig.json", model);
        plan.AddTemplateFile("frontend/index.html.sbn", $"src/{request.ProjectName}.Web/index.html", model);
        plan.AddTemplateFile("frontend/main.ts.sbn", $"src/{request.ProjectName}.Web/src/main.ts", model);
        plan.AddTemplateFile("frontend/App.vue.sbn", $"src/{request.ProjectName}.Web/src/App.vue", model);
        plan.AddTemplateFile("frontend/router/index.ts.sbn", $"src/{request.ProjectName}.Web/src/router/index.ts", model);
        plan.AddTemplateFile("frontend/stores/index.ts.sbn", $"src/{request.ProjectName}.Web/src/stores/index.ts", model);
        plan.AddTemplateFile("frontend/api/index.ts.sbn", $"src/{request.ProjectName}.Web/src/api/index.ts", model);
        plan.AddTemplateFile("frontend/views/HomeView.vue.sbn", $"src/{request.ProjectName}.Web/src/views/HomeView.vue", model);

        plan.AddNpmPackage("vue");
        plan.AddNpmPackage("vue-router");
        plan.AddNpmPackage("pinia");
        plan.AddNpmPackage("axios");
        plan.AddNpmPackage("element-plus");

        return Task.CompletedTask;
    }
}
