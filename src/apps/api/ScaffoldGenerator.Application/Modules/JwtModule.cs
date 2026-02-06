using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Contracts.Requests;

namespace ScaffoldGenerator.Application.Modules;

public sealed class JwtModule : IScaffoldModule
{
    public string Name => "JWT";
    public int Order => 30;

    public bool IsEnabled(GenerateScaffoldRequest request) => request.Backend.JwtAuth;

    public Task ContributeAsync(ScaffoldPlan plan, GenerateScaffoldRequest request, CancellationToken ct = default)
    {
        plan.AddNugetPackage("Microsoft.AspNetCore.Authentication.JwtBearer");

        var model = new
        {
            ProjectName = request.Basic.ProjectName,
            Namespace = request.Basic.Namespace
        };

        plan.AddTemplateFile("backend/JwtSetup.cs.sbn", $"src/{request.Basic.ProjectName}.Api/Extensions/JwtSetup.cs", model);
        plan.AddTemplateFile("backend/JwtOptions.cs.sbn", $"src/{request.Basic.ProjectName}.Api/Options/JwtOptions.cs", model);

        return Task.CompletedTask;
    }
}
