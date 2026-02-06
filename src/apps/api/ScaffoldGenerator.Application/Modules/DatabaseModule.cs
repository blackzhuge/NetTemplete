using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Contracts.Enums;
using ScaffoldGenerator.Contracts.Requests;

namespace ScaffoldGenerator.Application.Modules;

public sealed class DatabaseModule : IScaffoldModule
{
    public string Name => "Database";
    public int Order => 10;

    public bool IsEnabled(GenerateScaffoldRequest request) => true;

    public Task ContributeAsync(ScaffoldPlan plan, GenerateScaffoldRequest request, CancellationToken ct = default)
    {
        plan.AddNugetPackage("SqlSugarCore");

        var connectionString = request.Database switch
        {
            DatabaseProvider.SQLite => $"Data Source={request.ProjectName}.db",
            DatabaseProvider.MySQL => $"Server=localhost;Database={request.ProjectName};User=root;Password=;",
            DatabaseProvider.SQLServer => $"Server=localhost;Database={request.ProjectName};Trusted_Connection=True;TrustServerCertificate=True;",
            _ => $"Data Source={request.ProjectName}.db"
        };

        var dbType = request.Database switch
        {
            DatabaseProvider.SQLite => "Sqlite",
            DatabaseProvider.MySQL => "MySql",
            DatabaseProvider.SQLServer => "SqlServer",
            _ => "Sqlite"
        };

        var model = new
        {
            request.ProjectName,
            request.Namespace,
            DbType = dbType,
            ConnectionString = connectionString
        };

        plan.AddTemplateFile("backend/SqlSugarSetup.cs.sbn", $"src/{request.ProjectName}.Api/Extensions/SqlSugarSetup.cs", model);

        return Task.CompletedTask;
    }
}
