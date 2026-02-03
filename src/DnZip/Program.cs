using System;
using System.IO;
using System.Threading.Tasks;
using Sharprompt;
using ConsoleAppFramework;
using System.Text;
using Ionic.Zip;

namespace DnZip
{
    // HACK: 複数ファイル指定に対応
    // HACK: --no-dir-entries(-D) に対応
    public class Program
    {
        static async Task Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            await ConsoleApp.RunAsync(args, Compress);
        }


        public static int Compress(
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
                if (!TryGetPasswordFromConsole(out password))
                {
                    Console.WriteLine("Error: Password verification failed.");
                    return 1;
                }
            }
            try
            {
                CreateArchive(new FileInfo(archiveFilePath), sourceDirectory, recurse, password);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 1;
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

        public static void CreateArchive(
                  FileInfo archiveFile,
                  DirectoryInfo sourceDirectory,
                  bool recursePaths,
                  string password
              )
        {
            using var zip = new ZipFile(Encoding.GetEncoding("Shift_JIS"))
            {
                CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression
            };
            if (!string.IsNullOrEmpty(password)) zip.Password = password;
            AddEntry(zip, sourceDirectory, sourceDirectory, recursePaths);
            zip.Save(archiveFile.FullName);
        }

        private static void AddEntry(
            ZipFile zip,
            DirectoryInfo root,
            DirectoryInfo target,
            bool recursePaths
        )
        {
            foreach (var file in target.GetFiles())
            {
                var entryPath = Path.GetRelativePath(root.FullName, file.FullName);
                zip.AddFile(file.FullName, Path.GetDirectoryName(entryPath));
            }

            if (!recursePaths) return;

            foreach (var subDir in target.GetDirectories())
            {
                if (subDir.GetFiles().Length == 0)
                    zip.AddDirectory(subDir.FullName, subDir.Name);

                AddEntry(zip, root, subDir, recursePaths);
            }
        }
    }
}
