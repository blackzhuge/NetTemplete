namespace ScaffoldGenerator.Application.Abstractions;

public sealed class ScaffoldPlan
{
    private readonly List<ScaffoldFile> _files = [];
    private readonly List<string> _nugetPackages = [];
    private readonly List<string> _npmPackages = [];

    public IReadOnlyList<ScaffoldFile> Files => _files;
    public IReadOnlyList<string> NugetPackages => _nugetPackages;
    public IReadOnlyList<string> NpmPackages => _npmPackages;

    public void AddFile(string relativePath, string content)
    {
        _files.Add(new ScaffoldFile(relativePath, content));
    }

    public void AddTemplateFile(string templatePath, string outputPath, object model)
    {
        _files.Add(new ScaffoldFile(outputPath, templatePath, model));
    }

    public void AddNugetPackage(string packageName)
    {
        if (!_nugetPackages.Contains(packageName))
            _nugetPackages.Add(packageName);
    }

    public void AddNpmPackage(string packageName)
    {
        if (!_npmPackages.Contains(packageName))
            _npmPackages.Add(packageName);
    }
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
