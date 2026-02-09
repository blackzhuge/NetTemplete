using FluentAssertions;
using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Application.Modules;
using ScaffoldGenerator.Contracts.Enums;
using ScaffoldGenerator.Contracts.Requests;
using Xunit;

namespace ScaffoldGenerator.Tests.Modules;

public class OrmModuleTests
{
    private readonly OrmModule _module = new();

    [Fact]
    public void Name_ReturnsOrm()
    {
        _module.Name.Should().Be("Orm");
    }

    [Fact]
    public void Order_ShouldBe10()
    {
        _module.Order.Should().Be(10);
    }

    [Fact]
    public void IsEnabled_Always_ReturnsTrue()
    {
        var request = CreateRequest();
        _module.IsEnabled(request).Should().BeTrue();
    }

    [Fact]
    public async Task ContributeAsync_SqlSugar_AddsNoFiles()
    {
        var plan = new ScaffoldPlan();
        var request = CreateRequest(OrmProvider.SqlSugar);

        await _module.ContributeAsync(plan, request, default);

        // SqlSugar 由 DatabaseModule 处理
        plan.Files.Should().BeEmpty();
    }

    [Fact]
    public async Task ContributeAsync_EFCore_AddsDbContextAndSetup()
    {
        var plan = new ScaffoldPlan();
        var request = CreateRequest(OrmProvider.EFCore);

        await _module.ContributeAsync(plan, request, default);

        plan.Files.Should().Contain(f => f.OutputPath.Contains("DbContext.cs"));
        plan.Files.Should().Contain(f => f.OutputPath.Contains("EFCoreSetup.cs"));
        plan.NugetPackages.Should().Contain(p => p.Name == "Microsoft.EntityFrameworkCore");
    }

    [Fact]
    public async Task ContributeAsync_Dapper_AddsDapperSetup()
    {
        var plan = new ScaffoldPlan();
        var request = CreateRequest(OrmProvider.Dapper);

        await _module.ContributeAsync(plan, request, default);

        plan.Files.Should().Contain(f => f.OutputPath.Contains("DapperSetup.cs"));
        plan.NugetPackages.Should().Contain(p => p.Name == "Dapper");
    }

    [Fact]
    public async Task ContributeAsync_FreeSql_AddsFreeSqlSetup()
    {
        var plan = new ScaffoldPlan();
        var request = CreateRequest(OrmProvider.FreeSql);

        await _module.ContributeAsync(plan, request, default);

        plan.Files.Should().Contain(f => f.OutputPath.Contains("FreeSqlSetup.cs"));
        plan.NugetPackages.Should().Contain(p => p.Name == "FreeSql");
    }

    [Theory]
    [InlineData(OrmProvider.EFCore, DatabaseProvider.SQLite, "Sqlite")]
    [InlineData(OrmProvider.EFCore, DatabaseProvider.MySQL, "MySql")]
    [InlineData(OrmProvider.EFCore, DatabaseProvider.SQLServer, "SqlServer")]
    public async Task ContributeAsync_EFCore_AddsCorrectDbProvider(
        OrmProvider orm, DatabaseProvider db, string expectedPackageContains)
    {
        var plan = new ScaffoldPlan();
        var request = CreateRequest(orm, db);

        await _module.ContributeAsync(plan, request, default);

        plan.NugetPackages.Should().Contain(p =>
            p.Name.Contains(expectedPackageContains, StringComparison.OrdinalIgnoreCase));
    }

    private static GenerateScaffoldRequest CreateRequest(
        OrmProvider orm = OrmProvider.SqlSugar,
        DatabaseProvider database = DatabaseProvider.SQLite) => new()
    {
        Basic = new BasicOptions
        {
            ProjectName = "TestProject",
            Namespace = "TestProject"
        },
        Backend = new BackendOptions
        {
            Orm = orm,
            Database = database
        }
    };
}
