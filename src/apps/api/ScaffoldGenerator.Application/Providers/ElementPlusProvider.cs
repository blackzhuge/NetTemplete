using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Contracts.Enums;
using ScaffoldGenerator.Contracts.Packages;

namespace ScaffoldGenerator.Application.Providers;

public sealed class ElementPlusProvider : IUiLibraryProvider
{
    public UiLibrary Library => UiLibrary.ElementPlus;

    public IEnumerable<PackageReference> GetNpmPackages()
    {
        yield return new PackageReference("element-plus", "^2.5.0");
        yield return new PackageReference("@element-plus/icons-vue", "^2.3.0");
    }

    public string GetMainTsTemplatePath() => "frontend/main.ts.sbn";

    public IEnumerable<TemplateMapping> GetAdditionalTemplates()
    {
        yield break;
    }
}
