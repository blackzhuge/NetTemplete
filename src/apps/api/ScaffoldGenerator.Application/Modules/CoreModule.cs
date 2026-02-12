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
        // 添加用户选择的 NuGet 包到 plan
        foreach (var pkg in request.Backend.NugetPackages)
        {
            plan.AddNugetPackage(pkg);
        }

        var model = new
        {
            ProjectName = request.Basic.ProjectName,
            Namespace = request.Basic.Namespace,
            Orm = request.Backend.Orm.ToString(),
            DbType = request.Backend.Database.ToString(),
            ConnectionString = GetConnectionString(request),
            CacheType = request.Backend.Cache.ToString(),
            EnableJwtAuth = request.Backend.JwtAuth,
            EnableSwagger = request.Backend.Swagger,
            RouterMode = request.Frontend.RouterMode.ToString(),
            EnableMockData = request.Frontend.MockData,
            NugetPackages = request.Backend.NugetPackages
        };

        // 核心后端文件
        plan.AddTemplateFile("backend/Program.cs.sbn", $"src/{request.Basic.ProjectName}.Api/Program.cs", model);
        plan.AddTemplateFile("backend/appsettings.json.sbn", $"src/{request.Basic.ProjectName}.Api/appsettings.json", model);

        // 项目文件 - 使生成的项目可运行
        plan.AddTemplateFile("backend/Api.csproj.sbn", $"src/{request.Basic.ProjectName}.Api/{request.Basic.ProjectName}.Api.csproj", model);
        plan.AddTemplateFile("backend/Solution.sln.sbn", $"{request.Basic.ProjectName}.sln", model);
        plan.AddTemplateFile("backend/Directory.Build.props.sbn", $"Directory.Build.props", model);

        return Task.CompletedTask;
    }

    private static string GetConnectionString(GenerateScaffoldRequest request)
    {
        return request.Backend.Database switch
        {
            Contracts.Enums.DatabaseProvider.SQLite => $"Data Source={request.Basic.ProjectName}.db",
            Contracts.Enums.DatabaseProvider.MySQL => $"Server=localhost;Database={request.Basic.ProjectName};User=root;Password=;",
            Contracts.Enums.DatabaseProvider.SQLServer => $"Server=localhost;Database={request.Basic.ProjectName};Trusted_Connection=True;TrustServerCertificate=True;",
            _ => $"Data Source={request.Basic.ProjectName}.db"
        };
    }
}
