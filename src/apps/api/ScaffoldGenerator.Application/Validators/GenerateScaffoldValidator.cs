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

        // Backend enum validation
        RuleFor(x => x.Backend.Architecture).IsInEnum().WithMessage("无效的架构风格");
        RuleFor(x => x.Backend.Orm).IsInEnum().WithMessage("无效的 ORM 选项");
        RuleFor(x => x.Backend.Database).IsInEnum().WithMessage("无效的数据库选项");
        RuleFor(x => x.Backend.Cache).IsInEnum().WithMessage("无效的缓存选项");
        RuleFor(x => x.Backend.UnitTestFramework).IsInEnum().WithMessage("无效的后端单元测试框架");
        RuleFor(x => x.Backend.IntegrationTestFramework).IsInEnum().WithMessage("无效的后端集成测试框架");

        // Frontend enum validation
        RuleFor(x => x.Frontend.UiLibrary).IsInEnum().WithMessage("无效的 UI 库选项");
        RuleFor(x => x.Frontend.RouterMode).IsInEnum().WithMessage("无效的路由模式");
        RuleFor(x => x.Frontend.UnitTestFramework).IsInEnum().WithMessage("无效的前端单元测试框架");
        RuleFor(x => x.Frontend.E2EFramework).IsInEnum().WithMessage("无效的前端 E2E 测试框架");
    }
}
