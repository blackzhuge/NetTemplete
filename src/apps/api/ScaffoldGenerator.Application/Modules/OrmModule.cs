using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Contracts.Enums;
using ScaffoldGenerator.Contracts.Packages;
using ScaffoldGenerator.Contracts.Requests;

namespace ScaffoldGenerator.Application.Modules;

public sealed class OrmModule : IScaffoldModule
{
    public string Name => "Orm";
    public int Order => 10;

    public bool IsEnabled(GenerateScaffoldRequest request) => true;

    public Task ContributeAsync(ScaffoldPlan plan, GenerateScaffoldRequest request, CancellationToken ct = default)
    {
        var model = new
        {
            ProjectName = request.Basic.ProjectName,
            Namespace = request.Basic.Namespace,
            DbType = request.Backend.Database.ToString()
        };

        switch (request.Backend.Orm)
        {
            case OrmProvider.EFCore:
                AddEFCoreFiles(plan, request, model);
                break;

            case OrmProvider.Dapper:
                AddDapperFiles(plan, request, model);
                break;

            case OrmProvider.FreeSql:
                AddFreeSqlFiles(plan, request, model);
                break;

            case OrmProvider.SqlSugar:
            default:
                // SqlSugar 由现有 DatabaseModule 处理
                break;
        }

        return Task.CompletedTask;
    }

    private static void AddEFCoreFiles(ScaffoldPlan plan, GenerateScaffoldRequest request, object model)
    {
        var projectName = request.Basic.ProjectName;

        plan.AddTemplateFile(
            "backend/orm/efcore/DbContext.cs.sbn",
            $"src/{projectName}.Api/Data/{projectName}DbContext.cs",
            model);

        plan.AddTemplateFile(
            "backend/orm/efcore/EFCoreSetup.cs.sbn",
            $"src/{projectName}.Api/Setup/EFCoreSetup.cs",
            model);

        // EF Core 包
        plan.AddNugetPackage(new PackageReference("Microsoft.EntityFrameworkCore", "9.0.0"));
        plan.AddNugetPackage(new PackageReference("Microsoft.EntityFrameworkCore.Design", "9.0.0"));

        // 根据数据库类型添加 Provider
        switch (request.Backend.Database)
        {
            case DatabaseProvider.SQLite:
                plan.AddNugetPackage(new PackageReference("Microsoft.EntityFrameworkCore.Sqlite", "9.0.0"));
                break;
            case DatabaseProvider.MySQL:
                plan.AddNugetPackage(new PackageReference("Pomelo.EntityFrameworkCore.MySql", "9.0.0"));
                break;
            case DatabaseProvider.SQLServer:
                plan.AddNugetPackage(new PackageReference("Microsoft.EntityFrameworkCore.SqlServer", "9.0.0"));
                break;
        }
    }

    private static void AddDapperFiles(ScaffoldPlan plan, GenerateScaffoldRequest request, object model)
    {
        var projectName = request.Basic.ProjectName;

        plan.AddTemplateFile(
            "backend/orm/dapper/DapperSetup.cs.sbn",
            $"src/{projectName}.Api/Setup/DapperSetup.cs",
            model);

        plan.AddNugetPackage(new PackageReference("Dapper", "2.1.35"));
    }

    private static void AddFreeSqlFiles(ScaffoldPlan plan, GenerateScaffoldRequest request, object model)
    {
        var projectName = request.Basic.ProjectName;

        plan.AddTemplateFile(
            "backend/orm/freesql/FreeSqlSetup.cs.sbn",
            $"src/{projectName}.Api/Setup/FreeSqlSetup.cs",
            model);

        plan.AddNugetPackage(new PackageReference("FreeSql", "3.2.833"));

        // 根据数据库类型添加 Provider
        switch (request.Backend.Database)
        {
            case DatabaseProvider.SQLite:
                plan.AddNugetPackage(new PackageReference("FreeSql.Provider.Sqlite", "3.2.833"));
                break;
            case DatabaseProvider.MySQL:
                plan.AddNugetPackage(new PackageReference("FreeSql.Provider.MySql", "3.2.833"));
                break;
            case DatabaseProvider.SQLServer:
                plan.AddNugetPackage(new PackageReference("FreeSql.Provider.SqlServer", "3.2.833"));
                break;
        }
    }
}
