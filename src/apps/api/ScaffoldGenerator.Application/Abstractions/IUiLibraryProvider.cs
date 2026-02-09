using ScaffoldGenerator.Contracts.Enums;
using ScaffoldGenerator.Contracts.Packages;

namespace ScaffoldGenerator.Application.Abstractions;

/// <summary>
/// UI 库 Provider 接口
/// </summary>
public interface IUiLibraryProvider
{
    /// <summary>对应的 UI 库枚举值</summary>
    UiLibrary Library { get; }

    /// <summary>获取该 UI 库需要的 npm 包</summary>
    IEnumerable<PackageReference> GetNpmPackages();

    /// <summary>获取 main.ts 模板路径</summary>
    string GetMainTsTemplatePath();

    /// <summary>获取额外需要生成的模板（如 tailwind.config.js）</summary>
    IEnumerable<TemplateMapping> GetAdditionalTemplates();
}

/// <summary>
/// 模板映射记录
/// </summary>
public sealed record TemplateMapping(string TemplatePath, string OutputPath);
