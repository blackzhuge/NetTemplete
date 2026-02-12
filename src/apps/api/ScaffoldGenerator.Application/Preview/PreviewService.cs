using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Contracts.Preview;
using ScaffoldGenerator.Contracts.Requests;

namespace ScaffoldGenerator.Application.Preview;

/// <summary>
/// 文件预览服务实现
/// 复用 ScaffoldPlanBuilder 构建计划，按 outputPath 匹配目标文件并渲染
/// </summary>
public sealed class PreviewService : IPreviewService
{
    private readonly ScaffoldPlanBuilder _planBuilder;

    public PreviewService(ScaffoldPlanBuilder planBuilder)
    {
        _planBuilder = planBuilder;
    }

    public async Task<PreviewFileResponse?> PreviewFileAsync(PreviewFileRequest request, CancellationToken ct = default)
    {
        // 1. 构建完整计划
        var plan = await _planBuilder.BuildAsync(request.Config, ct);

        // 2. 查找目标文件
        var targetFile = plan.Files.FirstOrDefault(f =>
            f.OutputPath.Equals(request.OutputPath, StringComparison.OrdinalIgnoreCase));

        if (targetFile == null)
        {
            return null;
        }

        // 3. 渲染单个文件内容
        var renderedFiles = await _planBuilder.RenderPlanAsync(plan, ct);

        if (!renderedFiles.TryGetValue(targetFile.OutputPath, out var content))
        {
            return null;
        }

        // 4. 返回预览响应
        return new PreviewFileResponse(
            OutputPath: targetFile.OutputPath,
            Content: content,
            Language: LanguageMapper.GetLanguage(targetFile.OutputPath),
            IsTemplate: targetFile.IsTemplate
        );
    }

    /// <inheritdoc />
    public async Task<PreviewTreeResponse> GetPreviewTreeAsync(GenerateScaffoldRequest request, CancellationToken ct = default)
    {
        var plan = await _planBuilder.BuildAsync(request, ct);
        var tree = BuildFileTree(plan.Files);
        return new PreviewTreeResponse(tree);
    }

    /// <summary>
    /// 从文件列表构建文件树
    /// 算法：Files → Trie → 递归树
    /// </summary>
    private static List<FileTreeNodeDto> BuildFileTree(IReadOnlyList<ScaffoldFile> files)
    {
        if (files.Count == 0)
        {
            return [];
        }

        // 构建 Trie 结构
        var root = new TrieNode("", "");

        foreach (var file in files)
        {
            var segments = file.OutputPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var current = root;
            var pathBuilder = new List<string>();

            for (var i = 0; i < segments.Length; i++)
            {
                var segment = segments[i];
                pathBuilder.Add(segment);
                var fullPath = string.Join("/", pathBuilder);
                var isFile = i == segments.Length - 1;

                if (!current.Children.TryGetValue(segment, out var child))
                {
                    child = new TrieNode(segment, fullPath) { IsFile = isFile };
                    current.Children[segment] = child;
                }

                current = child;
            }
        }

        // 递归转换为 FileTreeNodeDto
        return ConvertTrieToTree(root.Children.Values);
    }

    /// <summary>
    /// 递归转换 Trie 节点为 FileTreeNodeDto
    /// 排序：目录优先，同级按名称排序
    /// </summary>
    private static List<FileTreeNodeDto> ConvertTrieToTree(IEnumerable<TrieNode> nodes)
    {
        return nodes
            .OrderByDescending(n => !n.IsFile) // 目录优先
            .ThenBy(n => n.Name, StringComparer.OrdinalIgnoreCase)
            .Select(node => new FileTreeNodeDto(
                Name: node.Name,
                Path: node.Path,
                IsDirectory: !node.IsFile,
                Children: node.Children.Count > 0 ? ConvertTrieToTree(node.Children.Values) : (node.IsFile ? null : [])
            ))
            .ToList();
    }

    /// <summary>
    /// Trie 节点，用于构建文件树
    /// </summary>
    private sealed class TrieNode
    {
        public string Name { get; }
        public string Path { get; }
        public bool IsFile { get; set; }
        public Dictionary<string, TrieNode> Children { get; } = new(StringComparer.OrdinalIgnoreCase);

        public TrieNode(string name, string path)
        {
            Name = name;
            Path = path;
        }
    }
}
