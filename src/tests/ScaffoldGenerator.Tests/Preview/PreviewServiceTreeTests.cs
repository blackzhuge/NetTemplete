using FluentAssertions;
using Moq;
using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Application.Preview;
using ScaffoldGenerator.Contracts.Enums;
using ScaffoldGenerator.Contracts.Preview;
using ScaffoldGenerator.Contracts.Requests;
using Xunit;

namespace ScaffoldGenerator.Tests.Preview;

public class PreviewServiceTreeTests
{
    private static GenerateScaffoldRequest CreateRequest(
        string projectName = "TestProject",
        string ns = "TestProject")
    {
        return new GenerateScaffoldRequest
        {
            Basic = new BasicOptions
            {
                ProjectName = projectName,
                Namespace = ns
            },
            Backend = new BackendOptions
            {
                Database = DatabaseProvider.SQLite,
                Cache = CacheProvider.None,
                Swagger = true,
                JwtAuth = true
            },
            Frontend = new FrontendOptions
            {
                RouterMode = RouterMode.Hash,
                MockData = false
            }
        };
    }

    private static PreviewService CreateServiceWithFiles(params string[] filePaths)
    {
        var templateRenderer = new Mock<ITemplateRenderer>();

        var module = new Mock<IScaffoldModule>();
        module.Setup(m => m.IsEnabled(It.IsAny<GenerateScaffoldRequest>())).Returns(true);
        module.Setup(m => m.Order).Returns(0);
        module.Setup(m => m.ContributeAsync(It.IsAny<ScaffoldPlan>(), It.IsAny<GenerateScaffoldRequest>(), It.IsAny<CancellationToken>()))
            .Callback<ScaffoldPlan, GenerateScaffoldRequest, CancellationToken>((plan, _, _) =>
            {
                foreach (var path in filePaths)
                {
                    plan.AddFile(path, "content");
                }
            })
            .Returns(Task.CompletedTask);

        var planBuilder = new ScaffoldPlanBuilder(new[] { module.Object }, templateRenderer.Object);
        return new PreviewService(planBuilder);
    }

    [Fact]
    public async Task GetPreviewTreeAsync_EmptyFiles_ReturnsEmptyTree()
    {
        // Arrange
        var templateRenderer = new Mock<ITemplateRenderer>();
        var modules = Enumerable.Empty<IScaffoldModule>();
        var planBuilder = new ScaffoldPlanBuilder(modules, templateRenderer.Object);
        var service = new PreviewService(planBuilder);

        // Act
        var result = await service.GetPreviewTreeAsync(CreateRequest());

        // Assert
        result.Tree.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPreviewTreeAsync_SingleLayerFiles_ReturnsFlatTree()
    {
        // Arrange
        var service = CreateServiceWithFiles(
            "README.md",
            "package.json",
            ".gitignore"
        );

        // Act
        var result = await service.GetPreviewTreeAsync(CreateRequest());

        // Assert
        result.Tree.Should().HaveCount(3);
        result.Tree.Should().AllSatisfy(node =>
        {
            node.IsDirectory.Should().BeFalse();
            node.Children.Should().BeNull();
        });
    }

    [Fact]
    public async Task GetPreviewTreeAsync_MultiLayerDirectories_ReturnsNestedTree()
    {
        // Arrange
        var service = CreateServiceWithFiles(
            "src/Api/Program.cs",
            "src/Api/appsettings.json",
            "src/Web/package.json"
        );

        // Act
        var result = await service.GetPreviewTreeAsync(CreateRequest());

        // Assert
        result.Tree.Should().HaveCount(1);

        var srcNode = result.Tree[0];
        srcNode.Name.Should().Be("src");
        srcNode.Path.Should().Be("src");
        srcNode.IsDirectory.Should().BeTrue();
        srcNode.Children.Should().HaveCount(2);

        var apiNode = srcNode.Children!.First(c => c.Name == "Api");
        apiNode.Path.Should().Be("src/Api");
        apiNode.IsDirectory.Should().BeTrue();
        apiNode.Children.Should().HaveCount(2);

        var webNode = srcNode.Children!.First(c => c.Name == "Web");
        webNode.Path.Should().Be("src/Web");
        webNode.IsDirectory.Should().BeTrue();
        webNode.Children.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPreviewTreeAsync_Sorting_DirectoriesFirst()
    {
        // Arrange
        var service = CreateServiceWithFiles(
            "README.md",
            "src/Program.cs",
            "docs/guide.md"
        );

        // Act
        var result = await service.GetPreviewTreeAsync(CreateRequest());

        // Assert
        result.Tree.Should().HaveCount(3);

        // First two should be directories (docs, src), last should be file (README.md)
        result.Tree[0].IsDirectory.Should().BeTrue();
        result.Tree[1].IsDirectory.Should().BeTrue();
        result.Tree[2].IsDirectory.Should().BeFalse();
        result.Tree[2].Name.Should().Be("README.md");
    }

    [Fact]
    public async Task GetPreviewTreeAsync_Sorting_AlphabeticalWithinSameType()
    {
        // Arrange
        var service = CreateServiceWithFiles(
            "src/Program.cs",
            "docs/guide.md",
            "api/endpoint.cs"
        );

        // Act
        var result = await service.GetPreviewTreeAsync(CreateRequest());

        // Assert
        result.Tree.Should().HaveCount(3);
        result.Tree[0].Name.Should().Be("api");
        result.Tree[1].Name.Should().Be("docs");
        result.Tree[2].Name.Should().Be("src");
    }

    [Fact]
    public async Task GetPreviewTreeAsync_PathCorrectness_ChildPathIncludesParent()
    {
        // Arrange
        var service = CreateServiceWithFiles(
            "src/Api/Controllers/HomeController.cs"
        );

        // Act
        var result = await service.GetPreviewTreeAsync(CreateRequest());

        // Assert
        var srcNode = result.Tree[0];
        srcNode.Path.Should().Be("src");

        var apiNode = srcNode.Children![0];
        apiNode.Path.Should().Be("src/Api");

        var controllersNode = apiNode.Children![0];
        controllersNode.Path.Should().Be("src/Api/Controllers");

        var fileNode = controllersNode.Children![0];
        fileNode.Path.Should().Be("src/Api/Controllers/HomeController.cs");
        fileNode.Name.Should().Be("HomeController.cs");
    }

    [Fact]
    public async Task GetPreviewTreeAsync_DirectoryNode_HasEmptyChildrenArray()
    {
        // Arrange - create a scenario where intermediate directory has files
        var service = CreateServiceWithFiles(
            "src/file.cs"
        );

        // Act
        var result = await service.GetPreviewTreeAsync(CreateRequest());

        // Assert
        var srcNode = result.Tree[0];
        srcNode.IsDirectory.Should().BeTrue();
        srcNode.Children.Should().NotBeNull();
        srcNode.Children.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPreviewTreeAsync_FileNode_HasNullChildren()
    {
        // Arrange
        var service = CreateServiceWithFiles("README.md");

        // Act
        var result = await service.GetPreviewTreeAsync(CreateRequest());

        // Assert
        var fileNode = result.Tree[0];
        fileNode.IsDirectory.Should().BeFalse();
        fileNode.Children.Should().BeNull();
    }

    [Fact]
    public async Task GetPreviewTreeAsync_MixedFilesAndDirectories_CorrectStructure()
    {
        // Arrange
        var service = CreateServiceWithFiles(
            "MyProject.sln",
            "src/MyProject.Api/Program.cs",
            "src/MyProject.Api/appsettings.json",
            "src/MyProject.Web/package.json",
            "README.md"
        );

        // Act
        var result = await service.GetPreviewTreeAsync(CreateRequest());

        // Assert
        result.Tree.Should().HaveCount(3);

        // Directories first, then files
        var srcNode = result.Tree.First(n => n.Name == "src");
        srcNode.IsDirectory.Should().BeTrue();

        var slnNode = result.Tree.First(n => n.Name == "MyProject.sln");
        slnNode.IsDirectory.Should().BeFalse();

        var readmeNode = result.Tree.First(n => n.Name == "README.md");
        readmeNode.IsDirectory.Should().BeFalse();
    }

    [Fact]
    public async Task GetPreviewTreeAsync_Idempotency_SameConfigReturnsSameTree()
    {
        // Arrange
        var service = CreateServiceWithFiles(
            "src/Api/Program.cs",
            "README.md"
        );
        var request = CreateRequest();

        // Act
        var result1 = await service.GetPreviewTreeAsync(request);
        var result2 = await service.GetPreviewTreeAsync(request);
        var result3 = await service.GetPreviewTreeAsync(request);

        // Assert
        result1.Tree.Should().HaveCount(result2.Tree.Count);
        result2.Tree.Should().HaveCount(result3.Tree.Count);

        result1.Tree[0].Name.Should().Be(result2.Tree[0].Name);
        result2.Tree[0].Name.Should().Be(result3.Tree[0].Name);
    }
}
