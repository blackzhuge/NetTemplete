using ScaffoldGenerator.Contracts.Requests;

namespace ScaffoldGenerator.Application.Abstractions;

public sealed class ScaffoldPlanBuilder
{
    private readonly IEnumerable<IScaffoldModule> _modules;
    private readonly ITemplateRenderer _templateRenderer;

    public ScaffoldPlanBuilder(
        IEnumerable<IScaffoldModule> modules,
        ITemplateRenderer templateRenderer)
    {
        _modules = modules;
        _templateRenderer = templateRenderer;
    }

    public async Task<ScaffoldPlan> BuildAsync(GenerateScaffoldRequest request, CancellationToken ct = default)
    {
        var plan = new ScaffoldPlan();

        var enabledModules = _modules
            .Where(m => m.IsEnabled(request))
            .OrderBy(m => m.Order)
            .ToList();

        foreach (var module in enabledModules)
        {
            await module.ContributeAsync(plan, request, ct);
        }

        return plan;
    }

    public async Task<Dictionary<string, string>> RenderPlanAsync(ScaffoldPlan plan, CancellationToken ct = default)
    {
        var result = new Dictionary<string, string>();

        foreach (var file in plan.Files)
        {
            if (file.IsTemplate)
            {
                var content = await _templateRenderer.RenderAsync(file.TemplatePath!, file.Model!, ct);
                result[file.OutputPath] = content;
            }
            else
            {
                result[file.OutputPath] = file.Content!;
            }
        }

        return result;
    }
}
