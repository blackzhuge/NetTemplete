using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Contracts.Enums;
using ScaffoldGenerator.Contracts.Packages;

namespace ScaffoldGenerator.Application.Providers;

public sealed class TailwindProvider : IUiLibraryProvider
{
    public UiLibrary Library => UiLibrary.TailwindHeadless;

    public IEnumerable<PackageReference> GetNpmPackages()
    {
        yield return new PackageReference("tailwindcss", "^3.4.0");
        yield return new PackageReference("postcss", "^8.4.0");
        yield return new PackageReference("autoprefixer", "^10.4.0");
        yield return new PackageReference("@headlessui/vue", "^1.7.0");
    }

    public string GetMainTsTemplatePath() => "frontend/ui/tailwind/main.ts.sbn";

    public IEnumerable<TemplateMapping> GetAdditionalTemplates()
    {
        yield return new TemplateMapping(
            "frontend/ui/tailwind/tailwind.config.js.sbn",
            "tailwind.config.js");
        yield return new TemplateMapping(
            "frontend/ui/tailwind/postcss.config.js.sbn",
            "postcss.config.js");
    }
}
