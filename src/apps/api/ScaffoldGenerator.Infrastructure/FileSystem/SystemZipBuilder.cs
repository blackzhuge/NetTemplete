using System.IO.Compression;
using System.Text;
using ScaffoldGenerator.Application.Abstractions;

namespace ScaffoldGenerator.Infrastructure.FileSystem;

public sealed class SystemZipBuilder : IZipBuilder
{
    private readonly MemoryStream _memoryStream;
    private ZipArchive? _archive;

    public SystemZipBuilder()
    {
        _memoryStream = new MemoryStream();
        _archive = new ZipArchive(_memoryStream, ZipArchiveMode.Create, leaveOpen: true);
    }

    public void AddFile(string relativePath, string content)
    {
        ArgumentNullException.ThrowIfNull(_archive);

        var entry = _archive.CreateEntry(relativePath, CompressionLevel.Optimal);
        using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
        writer.Write(content);
    }

    public void AddFile(string relativePath, byte[] content)
    {
        ArgumentNullException.ThrowIfNull(_archive);

        var entry = _archive.CreateEntry(relativePath, CompressionLevel.Optimal);
        using var stream = entry.Open();
        stream.Write(content, 0, content.Length);
    }

    public byte[] Build()
    {
        _archive?.Dispose();
        _archive = null;

        return _memoryStream.ToArray();
    }

    public void Reset()
    {
        _archive?.Dispose();
        _memoryStream.SetLength(0);
        _memoryStream.Position = 0;
        _archive = new ZipArchive(_memoryStream, ZipArchiveMode.Create, leaveOpen: true);
    }
}
