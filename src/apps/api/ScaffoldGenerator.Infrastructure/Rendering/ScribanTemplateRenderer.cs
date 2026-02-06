using Scriban;
using Scriban.Runtime;
using ScaffoldGenerator.Application.Abstractions;

namespace ScaffoldGenerator.Infrastructure.Rendering;

public sealed class ScribanTemplateRenderer : ITemplateRenderer
{
    private readonly ITemplateFileProvider _fileProvider;

    public ScribanTemplateRenderer(ITemplateFileProvider fileProvider)
    {
        _fileProvider = fileProvider;
    }

    public async Task<string> RenderAsync(string templatePath, object model, CancellationToken ct = default)
    {
        var templateContent = await _fileProvider.ReadTemplateAsync(templatePath, ct);
        var template = Template.Parse(templateContent);

        if (template.HasErrors)
        {
            var errors = string.Join("; ", template.Messages.Select(m => m.Message));
            throw new InvalidOperationException($"模板解析错误: {errors}");
        }

        var scriptObject = new ScriptObject();
        scriptObject.Import(model);

        var context = new TemplateContext();
        context.PushGlobal(scriptObject);

        return await template.RenderAsync(context);
    }
}
