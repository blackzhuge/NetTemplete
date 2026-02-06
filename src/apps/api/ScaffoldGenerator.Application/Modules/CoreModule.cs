using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Contracts.Requests;

namespace ScaffoldGenerator.Application.Modules;

public sealed class CoreModule : IScaffoldModule
{
    public string Name => "Core";
    public int Order => 0;

    public bool IsEnabled(GenerateScaffoldRequest request) => true;

    public Task ContributeAsync(ScaffoldPlan plan, GenerateScaffoldRequest request, CancellationToken ct = default)
    {
        var model = new
        {
            request.ProjectName,
            request.Namespace,
            DbType = request.Database.ToString(),
            ConnectionString = GetConnectionString(request),
            CacheType = request.Cache.ToString(),
            EnableJwtAuth = request.EnableJwtAuth,
            EnableSwagger = request.EnableSwagger,
            RouterMode = request.RouterMode.ToString()
        };

        plan.AddTemplateFile("backend/Program.cs.sbn", $"src/{request.ProjectName}.Api/Program.cs", model);
        plan.AddTemplateFile("backend/appsettings.json.sbn", $"src/{request.ProjectName}.Api/appsettings.json", model);

        return Task.CompletedTask;
    }

    private static string GetConnectionString(GenerateScaffoldRequest request)
    {
        return request.Database switch
        {
            Contracts.Enums.DatabaseProvider.SQLite => $"Data Source={request.ProjectName}.db",
            Contracts.Enums.DatabaseProvider.MySQL => $"Server=localhost;Database={request.ProjectName};User=root;Password=;",
            Contracts.Enums.DatabaseProvider.SQLServer => $"Server=localhost;Database={request.ProjectName};Trusted_Connection=True;TrustServerCertificate=True;",
            _ => $"Data Source={request.ProjectName}.db"
        };
    }
}
