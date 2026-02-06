namespace ScaffoldGenerator.Infrastructure.Rendering;

public sealed class FileSystemTemplateProvider : ITemplateFileProvider
{
    private readonly string _basePath;

    public FileSystemTemplateProvider(string basePath)
    {
        _basePath = basePath;
    }

    public async Task<string> ReadTemplateAsync(string templatePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_basePath, templatePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"模板文件不存在: {templatePath}");
        }

        return await File.ReadAllTextAsync(fullPath, ct);
    }

    public IEnumerable<string> GetTemplateFiles(string directory)
    {
        var fullPath = Path.Combine(_basePath, directory);
        if (!Directory.Exists(fullPath))
        {
            return [];
        }

        return Directory.EnumerateFiles(fullPath, "*.sbn", SearchOption.AllDirectories)
            .Select(f => Path.GetRelativePath(_basePath, f));
    }
}
