using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Contracts.Requests;
using PackageReference = ScaffoldGenerator.Contracts.Packages.PackageReference;

namespace ScaffoldGenerator.Application.Modules;

public sealed class FrontendModule(IEnumerable<IUiLibraryProvider> providers) : IScaffoldModule
{
    public string Name => "Frontend";
    public int Order => 100;

    public bool IsEnabled(GenerateScaffoldRequest request) => true;

    public Task ContributeAsync(ScaffoldPlan plan, GenerateScaffoldRequest request, CancellationToken ct = default)
    {
        // 基础依赖
        plan.AddNpmPackage(new PackageReference("vue", "^3.4.0"));
        plan.AddNpmPackage(new PackageReference("vue-router", "^4.2.0"));
        plan.AddNpmPackage(new PackageReference("pinia", "^2.1.0"));
        plan.AddNpmPackage(new PackageReference("axios", "^1.6.0"));

        // 根据选择的 UI 库添加依赖和模板
        var provider = providers.FirstOrDefault(p => p.Library == request.Frontend.UiLibrary)
            ?? throw new InvalidOperationException($"不支持的 UI 库: {request.Frontend.UiLibrary}");

        foreach (var pkg in provider.GetNpmPackages())
        {
            plan.AddNpmPackage(pkg);
        }

        // 添加用户选择的 npm 包到 plan
        foreach (var pkg in request.Frontend.NpmPackages)
        {
            plan.AddNpmPackage(pkg);
        }

        var npmPackages = plan.NpmPackages.ToArray();

        var model = new
        {
            ProjectName = request.Basic.ProjectName,
            Namespace = request.Basic.Namespace,
            RouterMode = request.Frontend.RouterMode.ToString(),
            EnableMockData = request.Frontend.MockData,
            NpmPackages = npmPackages,
            DependencyPackages = npmPackages.Where(p => !p.IsDevDependency).ToArray(),
            DevDependencyPackages = npmPackages.Where(p => p.IsDevDependency).ToArray(),
            UiLibrary = request.Frontend.UiLibrary.ToString()
        };

        var webPath = $"src/{request.Basic.ProjectName}.Web";

        // 基础前端文件
        plan.AddTemplateFile("frontend/package.json.sbn", $"{webPath}/package.json", model);
        plan.AddTemplateFile("frontend/vite.config.ts.sbn", $"{webPath}/vite.config.ts", model);
        plan.AddTemplateFile("frontend/tsconfig.json.sbn", $"{webPath}/tsconfig.json", model);
        plan.AddTemplateFile("frontend/tsconfig.node.json.sbn", $"{webPath}/tsconfig.node.json", model);
        plan.AddTemplateFile("frontend/index.html.sbn", $"{webPath}/index.html", model);
        plan.AddTemplateFile("frontend/App.vue.sbn", $"{webPath}/src/App.vue", model);
        plan.AddTemplateFile("frontend/router/index.ts.sbn", $"{webPath}/src/router/index.ts", model);
        plan.AddTemplateFile("frontend/stores/index.ts.sbn", $"{webPath}/src/stores/index.ts", model);
        plan.AddTemplateFile("frontend/api/index.ts.sbn", $"{webPath}/src/api/index.ts", model);
        plan.AddTemplateFile("frontend/views/HomeView.vue.sbn", $"{webPath}/src/views/HomeView.vue", model);

        if (request.Frontend.MockData)
        {
            plan.AddTemplateFile("frontend/mock/index.ts.sbn", $"{webPath}/src/mock/index.ts", model);
        }

        // main.ts 使用 UI 库特定模板
        plan.AddTemplateFile(provider.GetMainTsTemplatePath(), $"{webPath}/src/main.ts", model);

        // 添加 UI 库额外模板
        foreach (var template in provider.GetAdditionalTemplates())
        {
            var outputPath = template.OutputPath.StartsWith("src/")
                ? $"{webPath}/{template.OutputPath}"
                : $"{webPath}/{template.OutputPath}";
            plan.AddTemplateFile(template.TemplatePath, outputPath, model);
        }

        return Task.CompletedTask;
    }
}
