namespace ScaffoldGenerator.Application.Abstractions;

public interface IZipBuilder
{
    void AddFile(string relativePath, string content);

    void AddFile(string relativePath, byte[] content);

    byte[] Build();

    void Reset();
}
