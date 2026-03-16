using System;
using System.IO;

namespace DnZip
{
    public sealed class DnZipCommand(IPasswordPrompt passwordPrompt, IArchiveService archiveService)
    {
        private readonly IPasswordPrompt _passwordPrompt = passwordPrompt;
        private readonly IArchiveService _archiveService = archiveService;

        public int Compress(
              string archiveFilePath,
              string sourceDirectoryPath,
              bool recurse = false,
              bool encrypt = false)
        {
            if (string.IsNullOrEmpty(archiveFilePath)) return 1;
            if (string.IsNullOrEmpty(sourceDirectoryPath)) return 1;

            var sourceDirectory = new DirectoryInfo(sourceDirectoryPath);
            if (!sourceDirectory.Exists)
            {
                Console.WriteLine("Error: Source Directory path not found.");
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
                _archiveService.CreateArchive(new FileInfo(archiveFilePath), sourceDirectory, recurse, password);
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
    }
}
