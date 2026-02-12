using ScaffoldGenerator.Contracts.Preview;
using ScaffoldGenerator.Contracts.Requests;

namespace ScaffoldGenerator.Application.Preview;

/// <summary>
/// 文件预览服务接口
/// </summary>
public interface IPreviewService
{
    /// <summary>
    /// 预览单个文件内容
    /// </summary>
    /// <param name="request">预览请求（包含配置和目标路径）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>预览响应，包含渲染后的内容和语言类型</returns>
    Task<PreviewFileResponse?> PreviewFileAsync(PreviewFileRequest request, CancellationToken ct = default);

    /// <summary>
    /// 获取文件树预览
    /// </summary>
    /// <param name="request">配置请求</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>文件树响应</returns>
    Task<PreviewTreeResponse> GetPreviewTreeAsync(GenerateScaffoldRequest request, CancellationToken ct = default);
}
