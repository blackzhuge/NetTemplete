using ScaffoldGenerator.Contracts.Presets;

namespace ScaffoldGenerator.Application.Presets;

/// <summary>
/// 预设服务接口
/// </summary>
public interface IPresetService
{
    /// <summary>
    /// 获取所有可用预设
    /// </summary>
    Task<ScaffoldPresetsResponse> GetPresetsAsync(CancellationToken ct = default);

    /// <summary>
    /// 启动时验证所有预设的合法性
    /// </summary>
    void ValidateAllPresets();
}
