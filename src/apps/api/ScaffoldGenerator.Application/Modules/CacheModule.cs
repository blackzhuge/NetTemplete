using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Contracts.Enums;
using ScaffoldGenerator.Contracts.Requests;

namespace ScaffoldGenerator.Application.Modules;

public sealed class CacheModule : IScaffoldModule
{
    public string Name => "Cache";
    public int Order => 20;

    public bool IsEnabled(GenerateScaffoldRequest request) => request.Cache != CacheProvider.None;

    public Task ContributeAsync(ScaffoldPlan plan, GenerateScaffoldRequest request, CancellationToken ct = default)
    {
        var model = new
        {
            request.ProjectName,
            request.Namespace,
            CacheType = request.Cache.ToString(),
            UseRedis = request.Cache == CacheProvider.Redis
        };

        if (request.Cache == CacheProvider.Redis)
        {
            plan.AddNugetPackage("StackExchange.Redis");
            plan.AddTemplateFile("backend/RedisSetup.cs.sbn", $"src/{request.ProjectName}.Api/Extensions/RedisSetup.cs", model);
        }
        else
        {
            plan.AddTemplateFile("backend/MemoryCacheSetup.cs.sbn", $"src/{request.ProjectName}.Api/Extensions/MemoryCacheSetup.cs", model);
        }

        return Task.CompletedTask;
    }
}
