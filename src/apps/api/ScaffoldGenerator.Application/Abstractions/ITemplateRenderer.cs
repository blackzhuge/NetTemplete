namespace ScaffoldGenerator.Application.Abstractions;

public interface ITemplateRenderer
{
    Task<string> RenderAsync(string templatePath, object model, CancellationToken ct = default);
}
