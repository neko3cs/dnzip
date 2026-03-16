using System;
using System.IO;
using System.Linq;
using DnZip;
using Shouldly;
using Xunit;

namespace DnZip.Tests
{
    public class ZipArchiveServiceTests : IDisposable
    {
        private readonly TestWorkspace _workspace = new();
        private readonly ZipArchiveService _archiveService = new();

        [Fact]
        public void CreateArchive_ShouldCreateZipFile_WhenSourceDirectoryContainsFiles()
        {
            var sourceDirectory = _workspace.CreateSourceDirectory("source");
            var testFile = Path.Combine(sourceDirectory.FullName, "test.txt");
            File.WriteAllText(testFile, "hello world");
            File.SetLastWriteTime(testFile, new DateTime(2024, 5, 6, 7, 8, 9, DateTimeKind.Local));
            var archivePath = Path.Combine(_workspace.RootPath, "output.zip");

            _archiveService.CreateArchive(new FileInfo(archivePath), sourceDirectory, recursePaths: false, password: string.Empty);

            File.Exists(archivePath).ShouldBeTrue();
            var entry = ZipArchiveReader.GetEntries(archivePath).Single();
            entry.Name.ShouldBe("test.txt");
            entry.IsCrypted.ShouldBeFalse();
            entry.Size.ShouldBe(new FileInfo(testFile).Length);
            entry.Content.ShouldBe("hello world");
            entry.DateTime.ShouldBe(File.GetLastWriteTime(testFile), TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void CreateArchive_ShouldNotIncludeSubdirectories_WhenRecursePathsIsFalse()
        {
            var sourceDirectory = _workspace.CreateSourceDirectory("source_flat");
            var subDirectory = Directory.CreateDirectory(Path.Combine(sourceDirectory.FullName, "subdir"));
            File.WriteAllText(Path.Combine(sourceDirectory.FullName, "root.txt"), "root");
            File.WriteAllText(Path.Combine(subDirectory.FullName, "sub.txt"), "sub");
            var archivePath = Path.Combine(_workspace.RootPath, "output_flat.zip");

            _archiveService.CreateArchive(new FileInfo(archivePath), sourceDirectory, recursePaths: false, password: string.Empty);

            ZipArchiveReader.GetEntries(archivePath).Select(entry => entry.Name).ShouldBe(new[] { "root.txt" });
        }

        [Fact]
        public void CreateArchive_ShouldIncludeSubdirectories_WhenRecursePathsIsTrue()
        {
            var sourceDirectory = _workspace.CreateSourceDirectory("source_recurse");
            var subDirectory = Directory.CreateDirectory(Path.Combine(sourceDirectory.FullName, "subdir"));
            var rootFile = Path.Combine(sourceDirectory.FullName, "root.txt");
            var subFile = Path.Combine(subDirectory.FullName, "sub.txt");
            File.WriteAllText(rootFile, "root");
            File.WriteAllText(subFile, "sub");
            File.SetLastWriteTime(subDirectory.FullName, new DateTime(2024, 6, 7, 8, 9, 10, DateTimeKind.Local));
            var archivePath = Path.Combine(_workspace.RootPath, "output_recurse.zip");

            _archiveService.CreateArchive(new FileInfo(archivePath), sourceDirectory, recursePaths: true, password: string.Empty);

            var entries = ZipArchiveReader.GetEntries(archivePath).OrderBy(entry => entry.Name).ToArray();
            entries.Select(entry => entry.Name).ShouldBe(new[] { "root.txt", "subdir/", "subdir/sub.txt" });
            entries.Single(entry => entry.Name == "root.txt").Content.ShouldBe("root");

            var directoryEntry = entries.Single(entry => entry.Name == "subdir/");
            directoryEntry.IsDirectory.ShouldBeTrue();
            directoryEntry.Size.ShouldBe(0);
            directoryEntry.DateTime.ShouldBe(File.GetLastWriteTime(subDirectory.FullName), TimeSpan.FromSeconds(2));

            entries.Single(entry => entry.Name == "subdir/sub.txt").Content.ShouldBe("sub");
        }

        [Fact]
        public void CreateArchive_ShouldEncryptEntries_WhenPasswordIsProvided()
        {
            var sourceDirectory = _workspace.CreateSourceDirectory("source_encrypted");
            File.WriteAllText(Path.Combine(sourceDirectory.FullName, "secret.txt"), "very secret");
            var archivePath = Path.Combine(_workspace.RootPath, "output_encrypted.zip");

            _archiveService.CreateArchive(new FileInfo(archivePath), sourceDirectory, recursePaths: false, password: "p@ssw0rd");

            var entry = ZipArchiveReader.GetEntries(archivePath, "p@ssw0rd").Single();
            entry.Name.ShouldBe("secret.txt");
            entry.IsCrypted.ShouldBeTrue();
            entry.Content.ShouldBe("very secret");
        }

        [Fact]
        public void CreateArchive_ShouldThrow_WhenArchiveDirectoryDoesNotExist()
        {
            var sourceDirectory = _workspace.CreateSourceDirectory("source");
            var archivePath = Path.Combine(_workspace.RootPath, "missing", "output.zip");

            Should.Throw<DirectoryNotFoundException>(() =>
              _archiveService.CreateArchive(new FileInfo(archivePath), sourceDirectory, recursePaths: false, password: string.Empty));
        }

        public void Dispose()
        {
            _workspace.Dispose();
        }
    }
}
