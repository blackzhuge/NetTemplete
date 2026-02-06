namespace ScaffoldGenerator.Contracts.Responses;

public sealed record GenerationResult
{
    public bool Success { get; init; }

    public string FileName { get; init; } = string.Empty;

    public byte[] FileContent { get; init; } = [];

    public string? ErrorMessage { get; init; }
}
