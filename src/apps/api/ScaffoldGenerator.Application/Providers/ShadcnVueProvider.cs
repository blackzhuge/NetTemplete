using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Contracts.Enums;
using ScaffoldGenerator.Contracts.Packages;

namespace ScaffoldGenerator.Application.Providers;

public sealed class ShadcnVueProvider : IUiLibraryProvider
{
    public UiLibrary Library => UiLibrary.ShadcnVue;

    public IEnumerable<PackageReference> GetNpmPackages()
    {
        yield return new PackageReference("tailwindcss", "^3.4.0", IsDevDependency: true);
        yield return new PackageReference("postcss", "^8.4.0", IsDevDependency: true);
        yield return new PackageReference("autoprefixer", "^10.4.0", IsDevDependency: true);
        yield return new PackageReference("class-variance-authority", "^0.7.0");
        yield return new PackageReference("clsx", "^2.1.0");
        yield return new PackageReference("tailwind-merge", "^2.2.0");
        yield return new PackageReference("radix-vue", "^1.4.0");
        yield return new PackageReference("lucide-vue-next", "^0.312.0");
    }

    public string GetMainTsTemplatePath() => "frontend/ui/shadcn-vue/main.ts.sbn";

    public IEnumerable<TemplateMapping> GetAdditionalTemplates()
    {
        yield return new TemplateMapping(
            "frontend/ui/shadcn-vue/components.json.sbn",
            "components.json");
        yield return new TemplateMapping(
            "frontend/ui/tailwind/tailwind.config.js.sbn",
            "tailwind.config.js");
        yield return new TemplateMapping(
            "frontend/ui/tailwind/postcss.config.js.sbn",
            "postcss.config.js");
    }
}
