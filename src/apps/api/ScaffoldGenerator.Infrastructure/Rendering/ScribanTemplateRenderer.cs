using Microsoft.Extensions.Caching.Memory;
using Scriban;
using Scriban.Runtime;
using ScaffoldGenerator.Application.Abstractions;

namespace ScaffoldGenerator.Infrastructure.Rendering;

public sealed class ScribanTemplateRenderer : ITemplateRenderer
{
    private readonly ITemplateFileProvider _fileProvider;
    private readonly IMemoryCache _cache;
    private static readonly MemoryCacheEntryOptions CacheOptions = new()
    {
        SlidingExpiration = TimeSpan.FromMinutes(30),
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
    };

    public ScribanTemplateRenderer(ITemplateFileProvider fileProvider, IMemoryCache cache)
    {
        _fileProvider = fileProvider;
        _cache = cache;
    }

    public async Task<string> RenderAsync(string templatePath, object model, CancellationToken ct = default)
    {
        var template = await GetOrParseTemplateAsync(templatePath, ct);

        var scriptObject = new ScriptObject();
        scriptObject.Import(model);

        var context = new TemplateContext();
        context.PushGlobal(scriptObject);

        return await template.RenderAsync(context);
    }

    private async Task<Template> GetOrParseTemplateAsync(string templatePath, CancellationToken ct)
    {
        var cacheKey = $"template:{templatePath}";

        if (_cache.TryGetValue(cacheKey, out Template? cachedTemplate) && cachedTemplate != null)
        {
            return cachedTemplate;
        }

        var templateContent = await _fileProvider.ReadTemplateAsync(templatePath, ct);
        var template = Template.Parse(templateContent);

        if (template.HasErrors)
        {
            var errors = string.Join("; ", template.Messages.Select(m => m.Message));
            throw new InvalidOperationException($"模板解析错误: {errors}");
        }

        _cache.Set(cacheKey, template, CacheOptions);
        return template;
    }
}
