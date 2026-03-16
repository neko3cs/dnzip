using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace DnZip
{
    public sealed class DnZipCommand(IPasswordPrompt passwordPrompt, IArchiveService archiveService)
    {
        private readonly IPasswordPrompt _passwordPrompt = passwordPrompt;
        private readonly IArchiveService _archiveService = archiveService;

        /// <summary>
        /// Create a ZIP archive from one or more files or directories.
        /// </summary>
        /// <param name="archiveFilePath">Output path of the ZIP archive to create.</param>
        /// <param name="sourcePaths">One or more files or directories to archive.</param>
        /// <param name="recurse">-r, Include subdirectories recursively.</param>
        /// <param name="encrypt">-e, Prompt for a password and create an encrypted ZIP archive.</param>
        /// <param name="noDirEntries">-D, Do not create entries for directories.</param>
        public int Compress(
          string archiveFilePath,
          string[] sourcePaths,
          bool recurse = false,
          bool encrypt = false,
          bool noDirEntries = false)
        {
            if (string.IsNullOrEmpty(archiveFilePath)) return 1;
            if (sourcePaths.Length == 0) return 1;

            var sources = ResolveSources(sourcePaths);
            if (sources is null)
            {
                return 1;
            }

            var password = string.Empty;
            if (encrypt)
            {
                if (!TryGetPassword(out password))
                {
                    Console.WriteLine("Error: Password verification failed.");
                    return 1;
                }
            }

            try
            {
                _archiveService.CreateArchive(new FileInfo(archiveFilePath), sources, recurse, password, noDirEntries);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 1;
            }

            return 0;
        }

        public bool TryGetPassword(out string password)
        {
            password = string.Empty;

            var pw1 = _passwordPrompt.ReadPassword("Enter password: ");
            var pw2 = _passwordPrompt.ReadPassword("Verify password: ");

            if (!pw1.Equals(pw2)) return false;

            password = pw1;
            return true;
        }

        private static List<ArchiveSource>? ResolveSources(string[] sourcePaths)
        {
            var sources = new List<ArchiveSource>(sourcePaths.Length);

            foreach (var sourcePath in sourcePaths)
            {
                if (string.IsNullOrEmpty(sourcePath))
                {
                    Console.WriteLine("Error: Source path not found.");
                    return null;
                }

                if (Directory.Exists(sourcePath))
                {
                    var sourceDirectory = new DirectoryInfo(sourcePath);
                    sources.Add(new ArchiveSource(sourceDirectory, BuildEntryPath(sourceDirectory, sourcePath)));
                    continue;
                }

                if (File.Exists(sourcePath))
                {
                    var sourceFile = new FileInfo(sourcePath);
                    sources.Add(new ArchiveSource(sourceFile, BuildEntryPath(sourceFile, sourcePath)));
                    continue;
                }

                Console.WriteLine($"Error: Source path not found: {sourcePath}");
                return null;
            }

            return sources;
        }

        private static string BuildEntryPath(FileSystemInfo source, string sourcePath)
        {
            if (Path.IsPathRooted(sourcePath))
            {
                return ZipEntry.CleanName(source.Name);
            }

            var trimmedPath = Path.TrimEndingDirectorySeparator(sourcePath);
            return ZipEntry.CleanName(trimmedPath);
        }
    }
}
