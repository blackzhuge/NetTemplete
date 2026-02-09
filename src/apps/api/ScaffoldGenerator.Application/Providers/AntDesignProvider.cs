using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Contracts.Enums;
using ScaffoldGenerator.Contracts.Packages;

namespace ScaffoldGenerator.Application.Providers;

public sealed class AntDesignProvider : IUiLibraryProvider
{
    public UiLibrary Library => UiLibrary.AntDesignVue;

    public IEnumerable<PackageReference> GetNpmPackages()
    {
        yield return new PackageReference("ant-design-vue", "^4.2.0");
        yield return new PackageReference("@ant-design/icons-vue", "^7.0.0");
    }

    public string GetMainTsTemplatePath() => "frontend/ui/antd/main.ts.sbn";

    public IEnumerable<TemplateMapping> GetAdditionalTemplates()
    {
        yield break;
    }
}
