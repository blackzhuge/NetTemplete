namespace ScaffoldGenerator.Contracts.Responses;

public enum ErrorCode
{
    None = 0,
    ValidationError = 1,
    InvalidCombination = 2,
    TemplateError = 3
}

public sealed record GenerationResult
{
    public bool Success { get; init; }

    public string FileName { get; init; } = string.Empty;

    public byte[] FileContent { get; init; } = [];

    public string? ErrorMessage { get; init; }

    public ErrorCode ErrorCode { get; init; } = ErrorCode.None;

    public static GenerationResult Fail(string message, ErrorCode code = ErrorCode.ValidationError)
        => new() { Success = false, ErrorMessage = message, ErrorCode = code };

    public static GenerationResult Ok(string fileName, byte[] content)
        => new() { Success = true, FileName = fileName, FileContent = content };
}
