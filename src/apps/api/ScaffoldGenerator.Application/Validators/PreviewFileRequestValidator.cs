using FluentValidation;
using ScaffoldGenerator.Contracts.Preview;

namespace ScaffoldGenerator.Application.Validators;

/// <summary>
/// 文件预览请求验证器
/// 安全约束：拒绝路径穿越、绝对路径、反斜杠
/// </summary>
public sealed class PreviewFileRequestValidator : AbstractValidator<PreviewFileRequest>
{
    public PreviewFileRequestValidator()
    {
        RuleFor(x => x.Config)
            .NotNull().WithMessage("配置不能为空");

        RuleFor(x => x.OutputPath)
            .NotEmpty().WithMessage("输出路径不能为空")
            .Must(path => !path.Contains("..")).WithMessage("路径不能包含 '..'")
            .Must(path => !path.Contains('\\'.ToString())).WithMessage("路径不能包含反斜杠")
            .Must(path => !IsAbsolutePath(path)).WithMessage("路径不能是绝对路径");
    }

    private static bool IsAbsolutePath(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;

        // Windows: C:\ D:\ etc.
        if (path.Length >= 2 && char.IsLetter(path[0]) && path[1] == ':')
            return true;

        // Unix: /path
        if (path.StartsWith('/'))
            return true;

        return false;
    }
}
