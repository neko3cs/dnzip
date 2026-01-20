using System;
using System.IO;
using Cocona;
using Sharprompt;

namespace DnZip
{
    // HACK: 複数ファイル指定に対応
    // HACK: --no-dir-entries(-D) に対応
    class Program
    {
        static void Main(string[] args)
        {
            CoconaLiteApp.Run<Program>(args);
        }

        [PrimaryCommand]
        public static int CompressZipFile(
            [Argument] string archiveFilePath,
            [Argument] string sourceDirectoryPath,
            [Option('r')] bool recursePaths,
            [Option('e')] bool encrypt
        )
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
                if (!TryGetPasswordFromConsole(out password))
                {
                    Console.WriteLine("Error: Password verification failed.");
                    return 1;
                }
            }
            try
            {
                ZipArchiver.CreateArchive(new FileInfo(archiveFilePath), sourceDirectory, recursePaths, password);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return 0;
        }

        public static bool TryGetPasswordFromConsole(out string password)
        {
            password = string.Empty;
            var pw1 = Prompt.Password("Enter password: ");
            var pw2 = Prompt.Password("Verify password: ");

            if (!pw1.Equals(pw2)) return false;

            password = pw1;
            return true;
        }
    }
}
