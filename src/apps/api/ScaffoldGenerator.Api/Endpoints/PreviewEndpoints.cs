using FluentValidation;
using ScaffoldGenerator.Application.Preview;
using ScaffoldGenerator.Contracts.Preview;

namespace ScaffoldGenerator.Api.Endpoints;

/// <summary>
/// 预览 API 端点
/// </summary>
public static class PreviewEndpoints
{
    public static void MapPreviewEndpoints(this WebApplication app)
    {
        app.MapPost("/api/v1/scaffolds/preview-file", async (
            PreviewFileRequest request,
            IPreviewService previewService,
            IValidator<PreviewFileRequest> validator,
            CancellationToken ct) =>
        {
            // 验证请求
            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return Results.BadRequest(new { error = errors });
            }

            // 预览文件
            var response = await previewService.PreviewFileAsync(request, ct);

            if (response == null)
            {
                return Results.NotFound(new { error = $"该配置下找不到目标文件: {request.OutputPath}" });
            }

            return Results.Ok(response);
        });
    }
}
