# 测试规范

> 后端测试的最佳实践。

---

## 技术栈

- **xUnit** - 测试框架
- **Moq** - Mock 框架
- **FluentAssertions** - 断言库
- **WebApplicationFactory** - 集成测试

---

## 命名规范

格式：`Method_Scenario_Expected`

```csharp
[Fact]
public async Task ExecuteAsync_InvalidRequest_ReturnsValidationError()

[Fact]
public async Task ContributeAsync_RedisEnabled_AddsRedisPackage()

[Theory]
[InlineData(DatabaseProvider.SQLite, "Sqlite")]
[InlineData(DatabaseProvider.MySQL, "MySql")]
public async Task ContributeAsync_AllDatabases_GeneratesCorrectDbType(...)
```

---

## UseCase 测试

```csharp
public class GenerateScaffoldUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_InvalidRequest_ReturnsValidationError()
    {
        // Arrange
        var validator = new GenerateScaffoldValidator();
        var planBuilder = new Mock<ScaffoldPlanBuilder>();
        var zipBuilder = new Mock<IZipBuilder>();
        var useCase = new GenerateScaffoldUseCase(
            planBuilder.Object, zipBuilder.Object, validator);

        var request = CreateRequest(projectName: ""); // 无效

        // Act
        var result = await useCase.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.ValidationError);
    }

    private static GenerateScaffoldRequest CreateRequest(
        string projectName = "TestProject") => new()
    {
        Basic = new() { ProjectName = projectName, Namespace = projectName }
    };
}
```

---

## Module 测试

```csharp
public class DatabaseModuleTests
{
    private readonly DatabaseModule _module = new();

    [Fact]
    public async Task ContributeAsync_SQLite_AddsSqlSugarPackage()
    {
        var plan = new ScaffoldPlan();
        var request = CreateRequest(DatabaseProvider.SQLite);

        await _module.ContributeAsync(plan, request, default);

        plan.NugetPackages.Should().Contain(p => p.Name == "SqlSugarCore");
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
}
```

---

## Validator 测试

```csharp
public class GenerateScaffoldValidatorTests
{
    private readonly GenerateScaffoldValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task Validate_EmptyProjectName_ReturnsError(string name)
    {
        var request = CreateRequest(name);

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Basic.ProjectName");
    }

    [Theory]
    [InlineData("123Start")]   // 数字开头
    [InlineData("My-Project")] // 非法字符
    public async Task Validate_InvalidNamespace_ReturnsError(string ns)
    {
        var request = CreateRequest(namespace: ns);

        var result = await _validator.ValidateAsync(request);

        result.Errors.Should().Contain(e =>
            e.PropertyName.Contains("Namespace"));
    }
}
```

---

## 集成测试

```csharp
public class ScaffoldApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ScaffoldApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GenerateZip_ValidRequest_ReturnsZipFile()
    {
        var request = CreateValidRequest();

        var response = await _client.PostAsJsonAsync(
            "/api/v1/scaffolds/generate-zip", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType
            .Should().Be("application/zip");
    }

    [Fact]
    public async Task GenerateZip_InvalidRequest_Returns400()
    {
        var request = new GenerateScaffoldRequest
        {
            Basic = new() { ProjectName = "", Namespace = "" }
        };

        var response = await _client.PostAsJsonAsync(
            "/api/v1/scaffolds/generate-zip", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
```

---

## 覆盖率要求

| 层级 | 最低覆盖率 | 重点 |
|------|-----------|------|
| UseCases | 90% | 业务逻辑核心 |
| Modules | 85% | 条件分支全覆盖 |
| Validators | 95% | 所有验证规则 |
| Infrastructure | 70% | Mock 外部依赖 |

---

## Mock 原则

```csharp
// ✅ Mock 外部依赖
var renderer = new Mock<ITemplateRenderer>();
var zipBuilder = new Mock<IZipBuilder>();

// ❌ 不 Mock 业务逻辑（使用真实 Validator）
var validator = new GenerateScaffoldValidator(); // 真实实例
```

---

## 测试目录结构

```
src/tests/ScaffoldGenerator.Tests/
├── Api/
│   └── GenerateEndpointTests.cs
├── Application/
│   ├── UseCases/
│   │   └── GenerateScaffoldUseCaseTests.cs
│   ├── Modules/
│   │   ├── CoreModuleTests.cs
│   │   └── DatabaseModuleTests.cs
│   └── Validators/
│       └── GenerateScaffoldValidatorTests.cs
└── Infrastructure/
    └── ScribanTemplateRendererTests.cs
```
