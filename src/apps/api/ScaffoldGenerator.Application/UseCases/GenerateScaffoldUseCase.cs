using FluentValidation;
using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Contracts.Requests;
using ScaffoldGenerator.Contracts.Responses;

namespace ScaffoldGenerator.Application.UseCases;

public sealed class GenerateScaffoldUseCase
{
    private readonly ITemplateRenderer _templateRenderer;
    private readonly IZipBuilder _zipBuilder;
    private readonly IValidator<GenerateScaffoldRequest> _validator;

    public GenerateScaffoldUseCase(
        ITemplateRenderer templateRenderer,
        IZipBuilder zipBuilder,
        IValidator<GenerateScaffoldRequest> validator)
    {
        _templateRenderer = templateRenderer;
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
            return new GenerationResult
            {
                Success = false,
                ErrorMessage = errors
            };
        }

        try
        {
            _zipBuilder.Reset();

            // TODO: 渲染模板并添加到 ZIP (Phase 3-4 实现)

            var zipContent = _zipBuilder.Build();
            var fileName = $"{request.ProjectName}.zip";

            return new GenerationResult
            {
                Success = true,
                FileName = fileName,
                FileContent = zipContent
            };
        }
        catch (Exception ex)
        {
            return new GenerationResult
            {
                Success = false,
                ErrorMessage = $"生成失败: {ex.Message}"
            };
        }
    }
}
