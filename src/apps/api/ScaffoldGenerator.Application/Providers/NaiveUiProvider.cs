using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Contracts.Enums;
using ScaffoldGenerator.Contracts.Packages;

namespace ScaffoldGenerator.Application.Providers;

public sealed class NaiveUiProvider : IUiLibraryProvider
{
    public UiLibrary Library => UiLibrary.NaiveUI;

    public IEnumerable<PackageReference> GetNpmPackages()
    {
        yield return new PackageReference("naive-ui", "^2.38.0");
        yield return new PackageReference("@vicons/ionicons5", "^0.12.0");
    }

    public string GetMainTsTemplatePath() => "frontend/ui/naive-ui/main.ts.sbn";

    public IEnumerable<TemplateMapping> GetAdditionalTemplates()
    {
        yield break;
    }
}
