namespace ScaffoldGenerator.Infrastructure.Rendering;

public interface ITemplateFileProvider
{
    Task<string> ReadTemplateAsync(string templatePath, CancellationToken ct = default);

    IEnumerable<string> GetTemplateFiles(string directory);
}
