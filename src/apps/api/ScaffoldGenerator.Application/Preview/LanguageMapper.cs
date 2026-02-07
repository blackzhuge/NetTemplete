namespace ScaffoldGenerator.Application.Preview;

/// <summary>
/// 语言映射器 - 根据文件扩展名返回语法高亮语言
/// </summary>
public static class LanguageMapper
{
    private static readonly Dictionary<string, string> ExtensionMap = new(StringComparer.OrdinalIgnoreCase)
    {
        [".cs"] = "csharp",
        [".ts"] = "typescript",
        [".vue"] = "vue",
        [".json"] = "json",
        [".xml"] = "xml",
        [".csproj"] = "xml",
        [".html"] = "html",
        [".css"] = "css",
        [".scss"] = "scss",
        [".md"] = "markdown",
        [".js"] = "javascript",
        [".tsx"] = "tsx",
        [".jsx"] = "jsx",
        [".yaml"] = "yaml",
        [".yml"] = "yaml"
    };

    public static string GetLanguage(string filePath)
        => ExtensionMap.TryGetValue(Path.GetExtension(filePath), out var lang)
            ? lang
            : "plaintext";
}
