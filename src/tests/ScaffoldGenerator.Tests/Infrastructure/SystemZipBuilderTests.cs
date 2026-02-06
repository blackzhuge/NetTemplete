using System.IO.Compression;
using System.Text;
using FluentAssertions;
using ScaffoldGenerator.Infrastructure.FileSystem;
using Xunit;

namespace ScaffoldGenerator.Tests.Infrastructure;

public class SystemZipBuilderTests
{
    [Fact]
    public void AddFile_WithStringContent_CreatesValidZipEntry()
    {
        // Arrange
        var builder = new SystemZipBuilder();
        const string path = "test/file.txt";
        const string content = "Hello, World!";

        // Act
        builder.AddFile(path, content);
        var zipBytes = builder.Build();

        // Assert
        zipBytes.Should().NotBeEmpty();
        using var stream = new MemoryStream(zipBytes);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        archive.Entries.Should().ContainSingle();
        archive.Entries[0].FullName.Should().Be(path);

        using var reader = new StreamReader(archive.Entries[0].Open());
        reader.ReadToEnd().Should().Be(content);
    }

    [Fact]
    public void AddFile_WithByteContent_CreatesValidZipEntry()
    {
        // Arrange
        var builder = new SystemZipBuilder();
        const string path = "binary/data.bin";
        var content = new byte[] { 0x01, 0x02, 0x03, 0x04 };

        // Act
        builder.AddFile(path, content);
        var zipBytes = builder.Build();

        // Assert
        using var stream = new MemoryStream(zipBytes);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        archive.Entries.Should().ContainSingle();

        using var entryStream = archive.Entries[0].Open();
        using var ms = new MemoryStream();
        entryStream.CopyTo(ms);
        ms.ToArray().Should().BeEquivalentTo(content);
    }

    [Fact]
    public void AddFile_MultipleFiles_CreatesAllEntries()
    {
        // Arrange
        var builder = new SystemZipBuilder();

        // Act
        builder.AddFile("file1.txt", "Content 1");
        builder.AddFile("dir/file2.txt", "Content 2");
        builder.AddFile("dir/sub/file3.txt", "Content 3");
        var zipBytes = builder.Build();

        // Assert
        using var stream = new MemoryStream(zipBytes);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        archive.Entries.Should().HaveCount(3);
        archive.Entries.Select(e => e.FullName).Should().BeEquivalentTo(
            "file1.txt", "dir/file2.txt", "dir/sub/file3.txt");
    }

    [Fact]
    public void Reset_ClearsExistingEntries()
    {
        // Arrange
        var builder = new SystemZipBuilder();
        builder.AddFile("old.txt", "Old content");

        // Act
        builder.Reset();
        builder.AddFile("new.txt", "New content");
        var zipBytes = builder.Build();

        // Assert
        using var stream = new MemoryStream(zipBytes);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        archive.Entries.Should().ContainSingle();
        archive.Entries[0].FullName.Should().Be("new.txt");
    }

    [Fact]
    public void Build_EmptyArchive_ReturnsValidEmptyZip()
    {
        // Arrange
        var builder = new SystemZipBuilder();

        // Act
        var zipBytes = builder.Build();

        // Assert
        zipBytes.Should().NotBeEmpty();
        using var stream = new MemoryStream(zipBytes);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        archive.Entries.Should().BeEmpty();
    }

    [Fact]
    public void AddFile_ChineseContent_PreservesEncoding()
    {
        // Arrange
        var builder = new SystemZipBuilder();
        const string content = "你好，世界！这是中文内容测试。";

        // Act
        builder.AddFile("chinese.txt", content);
        var zipBytes = builder.Build();

        // Assert
        using var stream = new MemoryStream(zipBytes);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        using var reader = new StreamReader(archive.Entries[0].Open(), Encoding.UTF8);
        reader.ReadToEnd().Should().Be(content);
    }
}
