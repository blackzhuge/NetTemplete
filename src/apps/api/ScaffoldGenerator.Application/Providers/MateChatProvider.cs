using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Contracts.Enums;
using ScaffoldGenerator.Contracts.Packages;

namespace ScaffoldGenerator.Application.Providers;

public sealed class MateChatProvider : IUiLibraryProvider
{
    public UiLibrary Library => UiLibrary.MateChat;

    public IEnumerable<PackageReference> GetNpmPackages()
    {
        yield return new PackageReference("@matechat/core", "^0.1.0");
        yield return new PackageReference("vue-devui", "^1.6.0");
        yield return new PackageReference("@devui-design/icons", "^1.4.0");
    }

    public string GetMainTsTemplatePath() => "frontend/ui/matechat/main.ts.sbn";

    public IEnumerable<TemplateMapping> GetAdditionalTemplates()
    {
        yield return new TemplateMapping(
            "frontend/ui/matechat/ChatLayout.vue.sbn",
            "src/components/ChatLayout.vue");
    }
}
