using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Contracts.Enums;
using ScaffoldGenerator.Contracts.Requests;

namespace ScaffoldGenerator.Application.Modules;

public sealed class CacheModule : IScaffoldModule
{
    public string Name => "Cache";
    public int Order => 20;

    public bool IsEnabled(GenerateScaffoldRequest request) => request.Backend.Cache != CacheProvider.None;

    public Task ContributeAsync(ScaffoldPlan plan, GenerateScaffoldRequest request, CancellationToken ct = default)
    {
        var model = new
        {
            ProjectName = request.Basic.ProjectName,
            Namespace = request.Basic.Namespace,
            CacheType = request.Backend.Cache.ToString(),
            UseRedis = request.Backend.Cache == CacheProvider.Redis
        };

        if (request.Backend.Cache == CacheProvider.Redis)
        {
            plan.AddNugetPackage("StackExchange.Redis");
            plan.AddTemplateFile("backend/RedisSetup.cs.sbn", $"src/{request.Basic.ProjectName}.Api/Extensions/RedisSetup.cs", model);
        }
        else
        {
            plan.AddTemplateFile("backend/MemoryCacheSetup.cs.sbn", $"src/{request.Basic.ProjectName}.Api/Extensions/MemoryCacheSetup.cs", model);
        }

        return Task.CompletedTask;
    }
}
