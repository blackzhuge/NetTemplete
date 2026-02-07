using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Contracts.Preview;

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
}
