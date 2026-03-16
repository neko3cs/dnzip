using System;
using System.IO;
using DnZip;
using Shouldly;
using Xunit;

namespace DnZip.Tests
{
    public class DnZipCommandTests : IDisposable
    {
        private readonly TestWorkspace _workspace = new();

        [Fact]
        public void Compress_ShouldReturnError_WhenArchivePathIsEmpty()
        {
            var sourceDirectory = _workspace.CreateSourceDirectory("source");
            var command = CreateCommand();

            var result = command.Compress(string.Empty, sourceDirectory.FullName);

            result.ShouldBe(1);
        }

        [Fact]
        public void Compress_ShouldReturnError_WhenSourceDirectoryPathIsEmpty()
        {
            var archivePath = Path.Combine(_workspace.RootPath, "output.zip");
            var command = CreateCommand();

            var result = command.Compress(archivePath, string.Empty);

            result.ShouldBe(1);
            File.Exists(archivePath).ShouldBeFalse();
        }

        [Fact]
        public void Compress_ShouldReturnError_WhenSourceDirectoryDoesNotExist()
        {
            var archivePath = Path.Combine(_workspace.RootPath, "output.zip");
            var missingDirectory = Path.Combine(_workspace.RootPath, "missing");
            var command = CreateCommand();

            using var consoleOut = new StringWriter();
            var originalOut = Console.Out;
            Console.SetOut(consoleOut);

            try
            {
                var result = command.Compress(archivePath, missingDirectory);

                result.ShouldBe(1);
                File.Exists(archivePath).ShouldBeFalse();
                consoleOut.ToString().ShouldContain("Error: Source Directory path not found.");
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

        [Fact]
        public void Compress_ShouldDelegateToArchiveService_WhenInputIsValid()
        {
            var sourceDirectory = _workspace.CreateSourceDirectory("source");
            var archivePath = Path.Combine(_workspace.RootPath, "output.zip");
            var archiveService = new FakeArchiveService();
            var command = CreateCommand(archiveService: archiveService);

            var result = command.Compress(archivePath, sourceDirectory.FullName, recurse: true);

            result.ShouldBe(0);
            archiveService.CallCount.ShouldBe(1);
            archiveService.ArchiveFile.ShouldNotBeNull();
            archiveService.ArchiveFile!.FullName.ShouldBe(new FileInfo(archivePath).FullName);
            archiveService.SourceDirectory.ShouldNotBeNull();
            archiveService.SourceDirectory!.FullName.ShouldBe(sourceDirectory.FullName);
            archiveService.RecursePaths.ShouldBeTrue();
            archiveService.Password.ShouldBeEmpty();
        }

        [Fact]
        public void Compress_ShouldPassPasswordToArchiveService_WhenEncryptionIsEnabled()
        {
            var sourceDirectory = _workspace.CreateSourceDirectory("source");
            var archivePath = Path.Combine(_workspace.RootPath, "encrypted.zip");
            var archiveService = new FakeArchiveService();
            var passwordPrompt = new FakePasswordPrompt("secret", "secret");
            var command = CreateCommand(passwordPrompt, archiveService);

            var result = command.Compress(archivePath, sourceDirectory.FullName, encrypt: true);

            result.ShouldBe(0);
            archiveService.CallCount.ShouldBe(1);
            archiveService.Password.ShouldBe("secret");
            passwordPrompt.Messages.ShouldBe(new[] { "Enter password: ", "Verify password: " });
        }

        [Fact]
        public void Compress_ShouldReturnError_WhenPasswordVerificationFails()
        {
            var sourceDirectory = _workspace.CreateSourceDirectory("source");
            var archivePath = Path.Combine(_workspace.RootPath, "password-mismatch.zip");
            var command = CreateCommand(new FakePasswordPrompt("first", "second"));

            using var consoleOut = new StringWriter();
            var originalOut = Console.Out;
            Console.SetOut(consoleOut);

            try
            {
                var result = command.Compress(archivePath, sourceDirectory.FullName, encrypt: true);

                result.ShouldBe(1);
                consoleOut.ToString().ShouldContain("Error: Password verification failed.");
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

        [Fact]
        public void Compress_ShouldReturnError_WhenArchiveServiceThrows()
        {
            var sourceDirectory = _workspace.CreateSourceDirectory("source");
            var archivePath = Path.Combine(_workspace.RootPath, "throws.zip");
            var archiveService = new FakeArchiveService
            {
                ExceptionToThrow = new DirectoryNotFoundException("boom")
            };
            var command = CreateCommand(archiveService: archiveService);

            using var consoleOut = new StringWriter();
            var originalOut = Console.Out;
            Console.SetOut(consoleOut);

            try
            {
                var result = command.Compress(archivePath, sourceDirectory.FullName);

                result.ShouldBe(1);
                consoleOut.ToString().ShouldContain(nameof(DirectoryNotFoundException));
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

        [Fact]
        public void TryGetPassword_ShouldReturnFalse_WhenPasswordsDoNotMatch()
        {
            var prompt = new FakePasswordPrompt("first", "second");
            var command = CreateCommand(prompt);

            var result = command.TryGetPassword(out var password);

            result.ShouldBeFalse();
            password.ShouldBeEmpty();
            prompt.Messages.ShouldBe(new[] { "Enter password: ", "Verify password: " });
        }

        [Fact]
        public void TryGetPassword_ShouldReturnTrue_WhenPasswordsMatch()
        {
            var prompt = new FakePasswordPrompt("same-password", "same-password");
            var command = CreateCommand(prompt);

            var result = command.TryGetPassword(out var password);

            result.ShouldBeTrue();
            password.ShouldBe("same-password");
            prompt.Messages.ShouldBe(new[] { "Enter password: ", "Verify password: " });
        }

        public void Dispose()
        {
            _workspace.Dispose();
        }

        private static DnZipCommand CreateCommand(IPasswordPrompt? passwordPrompt = null, IArchiveService? archiveService = null)
        {
            return new DnZipCommand(
              passwordPrompt ?? new FakePasswordPrompt(),
              archiveService ?? new FakeArchiveService()
            );
        }
    }
}
