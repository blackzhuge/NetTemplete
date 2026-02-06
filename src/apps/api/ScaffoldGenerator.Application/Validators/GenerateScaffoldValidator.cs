using FluentValidation;
using ScaffoldGenerator.Contracts.Requests;

namespace ScaffoldGenerator.Application.Validators;

public sealed class GenerateScaffoldValidator : AbstractValidator<GenerateScaffoldRequest>
{
    public GenerateScaffoldValidator()
    {
        RuleFor(x => x.Basic)
            .NotNull().WithMessage("基本配置不能为空");

        RuleFor(x => x.Basic.ProjectName)
            .NotEmpty().WithMessage("项目名称不能为空")
            .Length(2, 50).WithMessage("项目名称长度应在 2-50 个字符之间")
            .Matches(@"^[a-zA-Z][a-zA-Z0-9]*$").WithMessage("项目名称只能包含字母和数字，且必须以字母开头");

        RuleFor(x => x.Basic.Namespace)
            .NotEmpty().WithMessage("命名空间不能为空")
            .Matches(@"^[a-zA-Z][a-zA-Z0-9]*(\.[a-zA-Z][a-zA-Z0-9]*)*$")
            .WithMessage("命名空间格式无效");
    }
}
