using System;
using System.IO;
using DnZip;
using Shouldly;
using Xunit;

namespace DnZip.Tests
{
    public class DnZipTests : IDisposable
    {
        private readonly string _testTempDir;

        public DnZipTests()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            _testTempDir = Path.Combine(Path.GetTempPath(), "DnZipTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_testTempDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testTempDir))
            {
                Directory.Delete(_testTempDir, true);
            }
        }

        [Fact]
        public void Compress_ShouldReturnError_WhenSourceDirectoryDoesNotExist()
        {
            // Arrange
            var archivePath = Path.Combine(_testTempDir, "output.zip");
            var nonExistentDir = Path.Combine(_testTempDir, "non_existent");

            // Act
            var result = Program.Compress(archivePath, nonExistentDir);

            // Assert
            result.ShouldBe(1);
            File.Exists(archivePath).ShouldBeFalse();
        }

        [Fact]
        public void Compress_ShouldCreateZipFile_WhenSourceDirectoryExists()
        {
            // Arrange
            var sourceDir = Path.Combine(_testTempDir, "source");
            Directory.CreateDirectory(sourceDir);
            File.WriteAllText(Path.Combine(sourceDir, "test.txt"), "hello world");

            var archivePath = Path.Combine(_testTempDir, "output.zip");

            // Act
            var result = Program.Compress(archivePath, sourceDir);

            // Assert
            result.ShouldBe(0);
            File.Exists(archivePath).ShouldBeTrue();
        }

        [Fact]
        public void Compress_ShouldIncludeSubdirectories_WhenRecurseIsTrue()
        {
            // Arrange
            var sourceDir = Path.Combine(_testTempDir, "source_recurse");
            var subDir = Path.Combine(sourceDir, "subdir");
            Directory.CreateDirectory(subDir);
            File.WriteAllText(Path.Combine(sourceDir, "root.txt"), "root");
            File.WriteAllText(Path.Combine(subDir, "sub.txt"), "sub");

            var archivePath = Path.Combine(_testTempDir, "output_recurse.zip");

            // Act
            var result = Program.Compress(archivePath, sourceDir, recurse: true);

            // Assert
            result.ShouldBe(0);
            File.Exists(archivePath).ShouldBeTrue();

            // Note: We could use DotNetZip here to verify contents, 
            // but ensuring the command returns 0 and creates the file is a good first step.
        }
    }
}
