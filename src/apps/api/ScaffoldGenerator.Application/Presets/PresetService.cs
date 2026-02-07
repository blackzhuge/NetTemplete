using FluentValidation;
using ScaffoldGenerator.Contracts.Presets;
using ScaffoldGenerator.Contracts.Requests;

namespace ScaffoldGenerator.Application.Presets;

/// <summary>
/// 预设服务实现
/// </summary>
public sealed class PresetService : IPresetService
{
    private const string Version = "1.0.0";
    private readonly IValidator<GenerateScaffoldRequest> _validator;

    public PresetService(IValidator<GenerateScaffoldRequest> validator)
    {
        _validator = validator;
    }

    public Task<ScaffoldPresetsResponse> GetPresetsAsync(CancellationToken ct = default)
    {
        var response = new ScaffoldPresetsResponse(Version, BuiltInPresets.All);
        return Task.FromResult(response);
    }

    public void ValidateAllPresets()
    {
        var errors = new List<string>();

        foreach (var preset in BuiltInPresets.All)
        {
            var result = _validator.Validate(preset.Config);
            if (!result.IsValid)
            {
                var messages = string.Join("; ", result.Errors.Select(e => e.ErrorMessage));
                errors.Add($"预设 '{preset.Id}' 验证失败: {messages}");
            }
        }

        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                $"预设验证失败，应用启动中止:\n{string.Join("\n", errors)}");
        }
    }
}
