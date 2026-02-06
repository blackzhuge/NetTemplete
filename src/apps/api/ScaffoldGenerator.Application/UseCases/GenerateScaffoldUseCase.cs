using FluentValidation;
using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Contracts.Requests;
using ScaffoldGenerator.Contracts.Responses;

namespace ScaffoldGenerator.Application.UseCases;

public sealed class GenerateScaffoldUseCase
{
    private readonly ScaffoldPlanBuilder _planBuilder;
    private readonly IZipBuilder _zipBuilder;
    private readonly IValidator<GenerateScaffoldRequest> _validator;

    public GenerateScaffoldUseCase(
        ScaffoldPlanBuilder planBuilder,
        IZipBuilder zipBuilder,
        IValidator<GenerateScaffoldRequest> validator)
    {
        _planBuilder = planBuilder;
        _zipBuilder = zipBuilder;
        _validator = validator;
    }

    public async Task<GenerationResult> ExecuteAsync(
        GenerateScaffoldRequest request,
        CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return GenerationResult.Fail(errors, ErrorCode.ValidationError);
        }

        try
        {
            _zipBuilder.Reset();

            var plan = await _planBuilder.BuildAsync(request, ct);
            var renderedFiles = await _planBuilder.RenderPlanAsync(plan, ct);

            foreach (var (path, content) in renderedFiles)
            {
                _zipBuilder.AddFile(path, content);
            }

            var zipContent = _zipBuilder.Build();
            var fileName = $"{request.Basic.ProjectName}.zip";

            return GenerationResult.Ok(fileName, zipContent);
        }
        catch (InvalidOperationException ex)
        {
            // 无效的选项组合
            return GenerationResult.Fail(ex.Message, ErrorCode.InvalidCombination);
        }
        catch (Exception)
        {
            // 模板渲染错误 - 不泄露内部细节
            return GenerationResult.Fail("模板生成失败，请稍后重试", ErrorCode.TemplateError);
        }
    }
}
