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

            _archiveService.CreateArchive(
              new FileInfo(archivePath),
              [new ArchiveSource(sourceDirectory, sourceDirectory.Name)],
              recursePaths: false,
              password: string.Empty);

            File.Exists(archivePath).ShouldBeTrue();
            var entry = ZipArchiveReader.GetEntries(archivePath).Single();
            entry.Name.ShouldBe("source/test.txt");
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

            _archiveService.CreateArchive(
              new FileInfo(archivePath),
              [new ArchiveSource(sourceDirectory, sourceDirectory.Name)],
              recursePaths: false,
              password: string.Empty);

            ZipArchiveReader.GetEntries(archivePath).Select(entry => entry.Name)
              .ShouldBe(["source_flat/root.txt"]);
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

            _archiveService.CreateArchive(
              new FileInfo(archivePath),
              [new ArchiveSource(sourceDirectory, sourceDirectory.Name)],
              recursePaths: true,
              password: string.Empty);

            var entries = ZipArchiveReader.GetEntries(archivePath).OrderBy(entry => entry.Name).ToArray();
            entries.Select(entry => entry.Name)
              .ShouldBe(["source_recurse/", "source_recurse/root.txt", "source_recurse/subdir/", "source_recurse/subdir/sub.txt"]);
            entries.Single(entry => entry.Name == "source_recurse/root.txt").Content.ShouldBe("root");

            var rootDirectoryEntry = entries.Single(entry => entry.Name == "source_recurse/");
            rootDirectoryEntry.IsDirectory.ShouldBeTrue();

            var subDirectoryEntry = entries.Single(entry => entry.Name == "source_recurse/subdir/");
            subDirectoryEntry.IsDirectory.ShouldBeTrue();
            subDirectoryEntry.Size.ShouldBe(0);
            subDirectoryEntry.DateTime.ShouldBe(File.GetLastWriteTime(subDirectory.FullName), TimeSpan.FromSeconds(2));

            entries.Single(entry => entry.Name == "source_recurse/subdir/sub.txt").Content.ShouldBe("sub");
        }

        [Fact]
        public void CreateArchive_ShouldIncludeMultipleFilesAndDirectories()
        {
            var sourceDirectory = _workspace.CreateSourceDirectory("data");
            File.WriteAllText(Path.Combine(sourceDirectory.FullName, "inside.txt"), "inside");
            var singleFilePath = Path.Combine(_workspace.RootPath, "single.txt");
            File.WriteAllText(singleFilePath, "single");
            var archivePath = Path.Combine(_workspace.RootPath, "mixed.zip");

            _archiveService.CreateArchive(
              new FileInfo(archivePath),
              [new ArchiveSource(sourceDirectory, sourceDirectory.Name), new ArchiveSource(new FileInfo(singleFilePath), Path.GetFileName(singleFilePath))],
              recursePaths: true,
              password: string.Empty);

            var entries = ZipArchiveReader.GetEntries(archivePath).OrderBy(entry => entry.Name).ToArray();
            entries.Select(entry => entry.Name).ShouldBe(["data/", "data/inside.txt", "single.txt"]);
            entries.Single(entry => entry.Name == "single.txt").Content.ShouldBe("single");
            entries.Single(entry => entry.Name == "data/inside.txt").Content.ShouldBe("inside");
        }

        [Fact]
        public void CreateArchive_ShouldEncryptEntries_WhenPasswordIsProvided()
        {
            var sourceDirectory = _workspace.CreateSourceDirectory("source_encrypted");
            File.WriteAllText(Path.Combine(sourceDirectory.FullName, "secret.txt"), "very secret");
            var archivePath = Path.Combine(_workspace.RootPath, "output_encrypted.zip");

            _archiveService.CreateArchive(
              new FileInfo(archivePath),
              [new ArchiveSource(sourceDirectory, sourceDirectory.Name)],
              recursePaths: false,
              password: "p@ssw0rd");

            var entry = ZipArchiveReader.GetEntries(archivePath, "p@ssw0rd").Single();
            entry.Name.ShouldBe("source_encrypted/secret.txt");
            entry.IsCrypted.ShouldBeTrue();
            entry.Content.ShouldBe("very secret");
        }

        [Fact]
        public void CreateArchive_ShouldThrow_WhenArchiveDirectoryDoesNotExist()
        {
            var sourceDirectory = _workspace.CreateSourceDirectory("source");
            var archivePath = Path.Combine(_workspace.RootPath, "missing", "output.zip");

            Should.Throw<DirectoryNotFoundException>(() =>
              _archiveService.CreateArchive(
                new FileInfo(archivePath),
                [new ArchiveSource(sourceDirectory, sourceDirectory.Name)],
                recursePaths: false,
                password: string.Empty));
        }

        public void Dispose()
        {
            _workspace.Dispose();
        }
    }
}
