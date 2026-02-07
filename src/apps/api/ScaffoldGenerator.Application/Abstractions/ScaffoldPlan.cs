using ScaffoldGenerator.Contracts.Packages;

namespace ScaffoldGenerator.Application.Abstractions;

public sealed class ScaffoldPlan
{
    private readonly List<ScaffoldFile> _files = [];
    private readonly Dictionary<string, PackageReference> _nugetPackages = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, PackageReference> _npmPackages = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<ScaffoldFile> Files => _files;
    public IReadOnlyCollection<PackageReference> NugetPackages => _nugetPackages.Values;
    public IReadOnlyCollection<PackageReference> NpmPackages => _npmPackages.Values;

    public void AddFile(string relativePath, string content)
    {
        _files.Add(new ScaffoldFile(relativePath, content));
    }

    public void AddTemplateFile(string templatePath, string outputPath, object model)
    {
        _files.Add(new ScaffoldFile(outputPath, templatePath, model));
    }

    /// <summary>
    /// 添加 NuGet 包引用（完整版本）
    /// </summary>
    public void AddNugetPackage(PackageReference package)
    {
        _nugetPackages.TryAdd(package.Name, package);
    }

    /// <summary>
    /// 添加 NuGet 包（兼容旧版，仅包名）
    /// </summary>
    public void AddNugetPackage(string packageName)
    {
        _nugetPackages.TryAdd(packageName, new PackageReference(packageName, "*"));
    }

    /// <summary>
    /// 添加 npm 包引用（完整版本）
    /// </summary>
    public void AddNpmPackage(PackageReference package)
    {
        _npmPackages.TryAdd(package.Name, package);
    }

    /// <summary>
    /// 添加 npm 包（兼容旧版，仅包名）
    /// </summary>
    public void AddNpmPackage(string packageName)
    {
        _npmPackages.TryAdd(packageName, new PackageReference(packageName, "*"));
    }

    /// <summary>
    /// 检查 NuGet 包是否已存在（用于冲突检测）
    /// </summary>
    public bool HasNugetPackage(string packageName) => _nugetPackages.ContainsKey(packageName);

    /// <summary>
    /// 检查 npm 包是否已存在（用于冲突检测）
    /// </summary>
    public bool HasNpmPackage(string packageName) => _npmPackages.ContainsKey(packageName);
}

public sealed record ScaffoldFile
{
    public string OutputPath { get; }
    public string? Content { get; }
    public string? TemplatePath { get; }
    public object? Model { get; }
    public bool IsTemplate => TemplatePath is not null;

    public ScaffoldFile(string outputPath, string content)
    {
        OutputPath = outputPath;
        Content = content;
    }

    public ScaffoldFile(string outputPath, string templatePath, object model)
    {
        OutputPath = outputPath;
        TemplatePath = templatePath;
        Model = model;
    }
}
