using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Contracts.Requests;

namespace ScaffoldGenerator.Application.Modules;

public sealed class SwaggerModule : IScaffoldModule
{
    public string Name => "Swagger";
    public int Order => 40;

    public bool IsEnabled(GenerateScaffoldRequest request) => request.Backend.Swagger;

    public Task ContributeAsync(ScaffoldPlan plan, GenerateScaffoldRequest request, CancellationToken ct = default)
    {
        plan.AddNugetPackage("Swashbuckle.AspNetCore");

        var model = new
        {
            ProjectName = request.Basic.ProjectName,
            Namespace = request.Basic.Namespace,
            EnableJwtAuth = request.Backend.JwtAuth
        };

        plan.AddTemplateFile("backend/SwaggerSetup.cs.sbn", $"src/{request.Basic.ProjectName}.Api/Extensions/SwaggerSetup.cs", model);

        return Task.CompletedTask;
    }
}
