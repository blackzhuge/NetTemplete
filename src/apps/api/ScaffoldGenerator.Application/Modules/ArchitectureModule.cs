using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Contracts.Enums;
using PackageReference = ScaffoldGenerator.Contracts.Packages.PackageReference;
using ScaffoldGenerator.Contracts.Requests;

namespace ScaffoldGenerator.Application.Modules;

public sealed class ArchitectureModule : IScaffoldModule
{
    public string Name => "Architecture";
    public int Order => 5;

    public bool IsEnabled(GenerateScaffoldRequest request) => true;

    public Task ContributeAsync(ScaffoldPlan plan, GenerateScaffoldRequest request, CancellationToken ct = default)
    {
        var model = new
        {
            ProjectName = request.Basic.ProjectName,
            Namespace = request.Basic.Namespace,
            Orm = request.Backend.Orm.ToString(),
            DbType = request.Backend.Database.ToString()
        };

        switch (request.Backend.Architecture)
        {
            case ArchitectureStyle.CleanArchitecture:
                AddCleanArchitectureFiles(plan, request, model);
                break;

            case ArchitectureStyle.VerticalSlice:
                AddVerticalSliceFiles(plan, request, model);
                break;

            case ArchitectureStyle.ModularMonolith:
                AddModularMonolithFiles(plan, request, model);
                break;

            case ArchitectureStyle.Simple:
            default:
                // Simple 架构不需要额外目录结构
                break;
        }

        return Task.CompletedTask;
    }

    private static void AddCleanArchitectureFiles(ScaffoldPlan plan, GenerateScaffoldRequest request, object model)
    {
        var projectName = request.Basic.ProjectName;

        // Application 层
        plan.AddTemplateFile(
            "backend/architecture/clean/Application.csproj.sbn",
            $"src/{projectName}.Application/{projectName}.Application.csproj",
            model);

        // Domain 层
        plan.AddTemplateFile(
            "backend/architecture/clean/Domain.csproj.sbn",
            $"src/{projectName}.Domain/{projectName}.Domain.csproj",
            model);

        // Infrastructure 层
        plan.AddTemplateFile(
            "backend/architecture/clean/Infrastructure.csproj.sbn",
            $"src/{projectName}.Infrastructure/{projectName}.Infrastructure.csproj",
            model);

        if (request.Backend.Orm == OrmProvider.EFCore)
        {
            plan.AddNugetPackage(new PackageReference("Microsoft.EntityFrameworkCore", "9.0.0"));

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
        else if (request.Backend.Orm == OrmProvider.SqlSugar)
        {
            plan.AddNugetPackage(new PackageReference("SqlSugarCore", "5.1.4.169"));
        }
        else if (request.Backend.Orm == OrmProvider.Dapper)
        {
            plan.AddNugetPackage(new PackageReference("Dapper", "2.1.35"));
        }
        else if (request.Backend.Orm == OrmProvider.FreeSql)
        {
            plan.AddNugetPackage(new PackageReference("FreeSql", "3.2.833"));

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

    private static void AddVerticalSliceFiles(ScaffoldPlan plan, GenerateScaffoldRequest request, object model)
    {
        var projectName = request.Basic.ProjectName;

        // Feature 示例结构
        plan.AddTemplateFile(
            "backend/architecture/vertical-slice/Feature.cs.sbn",
            $"src/{projectName}.Api/Features/Example/ExampleFeature.cs",
            model);
    }

    private static void AddModularMonolithFiles(ScaffoldPlan plan, GenerateScaffoldRequest request, object model)
    {
        var projectName = request.Basic.ProjectName;

        // 模块示例结构
        plan.AddTemplateFile(
            "backend/architecture/modular-monolith/Module.csproj.sbn",
            $"src/Modules/{projectName}.Core/{projectName}.Core.csproj",
            model);
    }
}
