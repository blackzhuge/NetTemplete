using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Contracts.Requests;

namespace ScaffoldGenerator.Application.Modules;

public sealed class SwaggerModule : IScaffoldModule
{
    public string Name => "Swagger";
    public int Order => 40;

    public bool IsEnabled(GenerateScaffoldRequest request) => request.EnableSwagger;

    public Task ContributeAsync(ScaffoldPlan plan, GenerateScaffoldRequest request, CancellationToken ct = default)
    {
        plan.AddNugetPackage("Swashbuckle.AspNetCore");

        var model = new
        {
            request.ProjectName,
            request.Namespace,
            EnableJwtAuth = request.EnableJwtAuth
        };

        plan.AddTemplateFile("backend/SwaggerSetup.cs.sbn", $"src/{request.ProjectName}.Api/Extensions/SwaggerSetup.cs", model);

        return Task.CompletedTask;
    }
}
