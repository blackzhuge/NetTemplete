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

        var connectionString = request.Backend.Database switch
        {
            DatabaseProvider.SQLite => $"Data Source={request.Basic.ProjectName}.db",
            DatabaseProvider.MySQL => $"Server=localhost;Database={request.Basic.ProjectName};User=root;Password=;",
            DatabaseProvider.SQLServer => $"Server=localhost;Database={request.Basic.ProjectName};Trusted_Connection=True;TrustServerCertificate=True;",
            _ => $"Data Source={request.Basic.ProjectName}.db"
        };

        var dbType = request.Backend.Database switch
        {
            DatabaseProvider.SQLite => "Sqlite",
            DatabaseProvider.MySQL => "MySql",
            DatabaseProvider.SQLServer => "SqlServer",
            _ => "Sqlite"
        };

        var model = new
        {
            ProjectName = request.Basic.ProjectName,
            Namespace = request.Basic.Namespace,
            DbType = dbType,
            ConnectionString = connectionString
        };

        plan.AddTemplateFile("backend/SqlSugarSetup.cs.sbn", $"src/{request.Basic.ProjectName}.Api/Extensions/SqlSugarSetup.cs", model);

        return Task.CompletedTask;
    }
}
