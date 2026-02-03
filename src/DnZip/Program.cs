using System;
using System.IO;
using System.Threading.Tasks;
using Sharprompt;
using ConsoleAppFramework;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;

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
            using var fsOut = File.Create(archiveFile.FullName);
            // Shift_JIS エンコーディングを設定
            var encoding = Encoding.GetEncoding("Shift_JIS");
            
            using var zipStream = new ZipOutputStream(fsOut, encoding.CodePage)
            {
                IsStreamOwner = true
            };

            if (!string.IsNullOrEmpty(password))
            {
                zipStream.Password = password;
            }

            zipStream.SetLevel(9); // BestCompression 相当

            AddEntry(zipStream, sourceDirectory, sourceDirectory, recursePaths);

            zipStream.Finish();
            zipStream.Close();
        }

        private static void AddEntry(
            ZipOutputStream zipStream,
            DirectoryInfo root,
            DirectoryInfo target,
            bool recursePaths
        )
        {
            foreach (var file in target.GetFiles())
            {
                var entryPath = Path.GetRelativePath(root.FullName, file.FullName);
                // Windows との互換性のためにパス区切りを / に統一
                entryPath = ZipEntry.CleanName(entryPath);

                var newEntry = new ZipEntry(entryPath)
                {
                    DateTime = file.LastWriteTime,
                    Size = file.Length
                };

                zipStream.PutNextEntry(newEntry);

                var buffer = new byte[4096];
                using var fsIn = File.OpenRead(file.FullName);
                StreamUtils.Copy(fsIn, zipStream, buffer);
                zipStream.CloseEntry();
            }

            if (!recursePaths) return;

            foreach (var subDir in target.GetDirectories())
            {
                var entryPath = Path.GetRelativePath(root.FullName, subDir.FullName);
                entryPath = ZipEntry.CleanName(entryPath);

                // 空のディレクトリ、またはディレクトリ自体のエントリを作成する場合
                // SharpZipLib では末尾に / を付けることでディレクトリとして扱う
                if (!entryPath.EndsWith("/"))
                {
                    entryPath += "/";
                }

                var dirEntry = new ZipEntry(entryPath)
                {
                    DateTime = subDir.LastWriteTime,
                    Size = 0
                };
                zipStream.PutNextEntry(dirEntry);
                zipStream.CloseEntry();

                AddEntry(zipStream, root, subDir, recursePaths);
            }
        }
    }
}
