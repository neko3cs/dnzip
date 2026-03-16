using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DnZip;
using ICSharpCode.SharpZipLib.Zip;
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
            Program.ResetTestHooks();
        }

        public void Dispose()
        {
            Program.ResetTestHooks();

            if (Directory.Exists(_testTempDir))
            {
                Directory.Delete(_testTempDir, true);
            }
        }

        [Fact]
        public async Task Main_ShouldInvokeConfiguredConsoleRunner()
        {
            string[]? receivedArgs = null;

            Program.SetConsoleAppRunner(args =>
            {
                receivedArgs = args;
                return Task.CompletedTask;
            });

            var mainMethod = typeof(Program).GetMethod("Main", BindingFlags.Static | BindingFlags.NonPublic);
            mainMethod.ShouldNotBeNull();

            var task = (Task)mainMethod!.Invoke(null, new object[] { new[] { "--help" } })!;
            await task;

            receivedArgs.ShouldBe(new[] { "--help" });
        }

        [Fact]
        public async Task RunConsoleAppAsync_ShouldPrintHelp()
        {
            using var consoleOut = new StringWriter();
            var originalOut = Console.Out;
            Console.SetOut(consoleOut);

            try
            {
                var runnerMethod = typeof(Program).GetMethod("RunConsoleAppAsync", BindingFlags.Static | BindingFlags.NonPublic);
                runnerMethod.ShouldNotBeNull();

                var task = (Task)runnerMethod!.Invoke(null, new object[] { new[] { "--help" } })!;
                await task;

                consoleOut.ToString().ShouldContain("Usage:");
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

        [Fact]
        public void Compress_ShouldReturnError_WhenArchivePathIsEmpty()
        {
            var sourceDir = CreateSourceDirectory("source");

            var result = Program.Compress(string.Empty, sourceDir.FullName);

            result.ShouldBe(1);
        }

        [Fact]
        public void Compress_ShouldReturnError_WhenSourceDirectoryPathIsEmpty()
        {
            var archivePath = Path.Combine(_testTempDir, "output.zip");

            var result = Program.Compress(archivePath, string.Empty);

            result.ShouldBe(1);
            File.Exists(archivePath).ShouldBeFalse();
        }

        [Fact]
        public void Compress_ShouldReturnError_WhenSourceDirectoryDoesNotExist()
        {
            var archivePath = Path.Combine(_testTempDir, "output.zip");
            var nonExistentDir = Path.Combine(_testTempDir, "non_existent");

            using var consoleOut = new StringWriter();
            var originalOut = Console.Out;
            Console.SetOut(consoleOut);

            try
            {
                var result = Program.Compress(archivePath, nonExistentDir);

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
        public void Compress_ShouldReturnError_WhenArchiveDirectoryDoesNotExist()
        {
            var sourceDir = CreateSourceDirectory("source");
            var archivePath = Path.Combine(_testTempDir, "missing", "output.zip");

            using var consoleOut = new StringWriter();
            var originalOut = Console.Out;
            Console.SetOut(consoleOut);

            try
            {
                var result = Program.Compress(archivePath, sourceDir.FullName);

                result.ShouldBe(1);
                File.Exists(archivePath).ShouldBeFalse();
                consoleOut.ToString().ShouldContain(nameof(DirectoryNotFoundException));
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

        [Fact]
        public void Compress_ShouldCreateZipFile_WhenSourceDirectoryExists()
        {
            var sourceDir = CreateSourceDirectory("source");
            var testFile = Path.Combine(sourceDir.FullName, "test.txt");
            File.WriteAllText(testFile, "hello world");
            File.SetLastWriteTime(testFile, new DateTime(2024, 5, 6, 7, 8, 9, DateTimeKind.Local));
            var archivePath = Path.Combine(_testTempDir, "output.zip");

            var result = Program.Compress(archivePath, sourceDir.FullName);

            result.ShouldBe(0);
            File.Exists(archivePath).ShouldBeTrue();

            var entry = GetEntries(archivePath).Single();
            entry.Name.ShouldBe("test.txt");
            entry.IsCrypted.ShouldBeFalse();
            entry.Size.ShouldBe(new FileInfo(testFile).Length);
            entry.Content.ShouldBe("hello world");
            entry.DateTime.ShouldBe(File.GetLastWriteTime(testFile), TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Compress_ShouldNotIncludeSubdirectories_WhenRecurseIsFalse()
        {
            var sourceDir = CreateSourceDirectory("source_flat");
            var subDir = Directory.CreateDirectory(Path.Combine(sourceDir.FullName, "subdir"));
            File.WriteAllText(Path.Combine(sourceDir.FullName, "root.txt"), "root");
            File.WriteAllText(Path.Combine(subDir.FullName, "sub.txt"), "sub");
            var archivePath = Path.Combine(_testTempDir, "output_flat.zip");

            var result = Program.Compress(archivePath, sourceDir.FullName);

            result.ShouldBe(0);
            GetEntries(archivePath).Select(entry => entry.Name).ShouldBe(new[] { "root.txt" });
        }

        [Fact]
        public void Compress_ShouldIncludeSubdirectories_WhenRecurseIsTrue()
        {
            var sourceDir = CreateSourceDirectory("source_recurse");
            var rootFile = Path.Combine(sourceDir.FullName, "root.txt");
            var subDir = Directory.CreateDirectory(Path.Combine(sourceDir.FullName, "subdir"));
            var subFile = Path.Combine(subDir.FullName, "sub.txt");
            File.WriteAllText(rootFile, "root");
            File.WriteAllText(subFile, "sub");
            File.SetLastWriteTime(subDir.FullName, new DateTime(2024, 6, 7, 8, 9, 10, DateTimeKind.Local));
            var archivePath = Path.Combine(_testTempDir, "output_recurse.zip");

            var result = Program.Compress(archivePath, sourceDir.FullName, recurse: true);

            result.ShouldBe(0);
            File.Exists(archivePath).ShouldBeTrue();

            var entries = GetEntries(archivePath).OrderBy(entry => entry.Name).ToArray();
            entries.Select(entry => entry.Name).ShouldBe(new[] { "root.txt", "subdir/", "subdir/sub.txt" });
            entries.Single(entry => entry.Name == "root.txt").Content.ShouldBe("root");

            var directoryEntry = entries.Single(entry => entry.Name == "subdir/");
            directoryEntry.IsDirectory.ShouldBeTrue();
            directoryEntry.Size.ShouldBe(0);
            directoryEntry.DateTime.ShouldBe(File.GetLastWriteTime(subDir.FullName), TimeSpan.FromSeconds(2));

            entries.Single(entry => entry.Name == "subdir/sub.txt").Content.ShouldBe("sub");
        }

        [Fact]
        public void CreateArchive_ShouldEncryptEntries_WhenPasswordIsProvided()
        {
            var sourceDir = CreateSourceDirectory("source_encrypted");
            File.WriteAllText(Path.Combine(sourceDir.FullName, "secret.txt"), "very secret");
            var archivePath = Path.Combine(_testTempDir, "output_encrypted.zip");
            var archiveFile = new FileInfo(archivePath);

            Program.CreateArchive(archiveFile, sourceDir, recursePaths: false, password: "p@ssw0rd");

            var entry = GetEntries(archivePath, "p@ssw0rd").Single();
            entry.Name.ShouldBe("secret.txt");
            entry.IsCrypted.ShouldBeTrue();
            entry.Content.ShouldBe("very secret");
        }

        [Fact]
        public void Compress_ShouldReturnError_WhenPasswordVerificationFails()
        {
            var sourceDir = CreateSourceDirectory("source_password_mismatch");
            File.WriteAllText(Path.Combine(sourceDir.FullName, "secret.txt"), "top secret");
            var archivePath = Path.Combine(_testTempDir, "password-mismatch.zip");
            var responses = new Queue<string>(new[] { "first", "second" });
            Program.SetPasswordReader(_ => responses.Dequeue());

            using var consoleOut = new StringWriter();
            var originalOut = Console.Out;
            Console.SetOut(consoleOut);

            try
            {
                var result = Program.Compress(archivePath, sourceDir.FullName, encrypt: true);

                result.ShouldBe(1);
                File.Exists(archivePath).ShouldBeFalse();
                consoleOut.ToString().ShouldContain("Error: Password verification failed.");
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

        [Fact]
        public void TryGetPasswordFromConsole_ShouldReturnFalse_WhenPasswordsDoNotMatch()
        {
            var responses = new Queue<string>(new[] { "first", "second" });
            var prompts = new List<string>();
            Program.SetPasswordReader(message =>
            {
                prompts.Add(message);
                return responses.Dequeue();
            });

            var result = Program.TryGetPasswordFromConsole(out var password);

            result.ShouldBeFalse();
            password.ShouldBeEmpty();
            prompts.ShouldBe(new[] { "Enter password: ", "Verify password: " });
        }

        [Fact]
        public void TryGetPasswordFromConsole_ShouldReturnTrue_WhenPasswordsMatch()
        {
            var responses = new Queue<string>(new[] { "same-password", "same-password" });
            var prompts = new List<string>();
            Program.SetPasswordReader(message =>
            {
                prompts.Add(message);
                return responses.Dequeue();
            });

            var result = Program.TryGetPasswordFromConsole(out var password);

            result.ShouldBeTrue();
            password.ShouldBe("same-password");
            prompts.ShouldBe(new[] { "Enter password: ", "Verify password: " });
        }

        private DirectoryInfo CreateSourceDirectory(string directoryName)
        {
            return Directory.CreateDirectory(Path.Combine(_testTempDir, directoryName));
        }

        private static ArchiveEntry[] GetEntries(string archivePath, string? password = null)
        {
            using var fileStream = File.OpenRead(archivePath);
            using var zipFile = new ZipFile(fileStream);

            if (!string.IsNullOrEmpty(password))
            {
                zipFile.Password = password;
            }

            var entries = new List<ArchiveEntry>();
            foreach (ZipEntry entry in zipFile)
            {
                string? content = null;
                if (!entry.IsDirectory)
                {
                    using var entryStream = zipFile.GetInputStream(entry);
                    using var reader = new StreamReader(entryStream);
                    content = reader.ReadToEnd();
                }

                entries.Add(new ArchiveEntry(entry.Name, entry.Size, entry.DateTime, entry.IsCrypted, entry.IsDirectory, content));
            }

            return entries.ToArray();
        }

        private sealed record ArchiveEntry(
          string Name,
          long Size,
          DateTime DateTime,
          bool IsCrypted,
          bool IsDirectory,
          string? Content
        );
    }
}
