using System;
using System.IO;
using System.Text;
using Cocona;
using Ionic.Zip;

namespace DnZip
{
    // HACK: 複数ファイル指定に対応
    class Program
    {
        static void Main(string[] args)
        {
            CoconaLiteApp.Run<Program>(args);
        }

        [PrimaryCommand]
        public int CompressZipFile(
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

            // FIXME: サブディレクトリに既出ファイル名のファイルがあるとエラーする
            var archiveFile = new FileInfo(archiveFilePath);
            try
            {
                CreateArchive(archiveFile, sourceDirectory, recursePaths, password);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return 0;
        }

        private bool TryGetPasswordFromConsole(out string password)
        {
            password = string.Empty;

            Console.Write("Enter password: ");
            var pw1 = ConsoleReadPassword();
            Console.Write("Verify password: ");
            var pw2 = ConsoleReadPassword();

            if (!pw1.Equals(pw2)) return false;

            password = pw1;
            return true;
        }

        private string ConsoleReadPassword()
        {
            var password = new StringBuilder();

            while (true)
            {
                var keyinfo = Console.ReadKey(intercept: true);

                if (keyinfo.Key.Equals(ConsoleKey.Enter))
                {
                    Console.WriteLine();
                    return password.ToString();
                }
                else if (keyinfo.Key.Equals(ConsoleKey.Backspace))
                {
                    if (password.Length > 0)
                    {
                        password.Length -= 1;
                        continue;
                    }
                }
                else if (Char.IsLetter(keyinfo.KeyChar))
                {
                    if ((keyinfo.Modifiers & ConsoleModifiers.Shift) == 0)
                    {
                        password.Append(keyinfo.KeyChar);
                        continue;
                    }
                    else
                    {
                        if (Console.CapsLock)
                        {
                            password.Append(Char.ToLower(keyinfo.KeyChar));
                            continue;
                        }
                        else
                        {
                            password.Append(Char.ToUpper(keyinfo.KeyChar));
                            continue;
                        }
                    }
                }
                else if (!Char.IsControl(keyinfo.KeyChar))
                {
                    password.Append(keyinfo.KeyChar);
                    continue;
                }

                Console.Beep();
            }
        }

        private void CreateArchive(
            FileInfo archiveFile,
            DirectoryInfo sourceDirectory,
            bool recursePaths,
            string password
        )
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (var zip = new ZipFile(Encoding.GetEncoding("Shift_JIS")))
            {
                zip.CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression;
                if (!string.IsNullOrEmpty(password)) zip.Password = password;
                AddEntry(zip, sourceDirectory, sourceDirectory, recursePaths);
                zip.Save(archiveFile.FullName);
            }
        }

        private void AddEntry(
            ZipFile zip,
            DirectoryInfo rootDir,
            DirectoryInfo targetDir,
            bool recursePaths
        )
        {
            foreach (var file in targetDir.GetFiles())
            {
                zip.AddFile(file.FullName, targetDir.FullName.Replace(rootDir.FullName, string.Empty));
            }
            if (recursePaths)
            {
                foreach (var subDir in targetDir.GetDirectories())
                {
                    zip.AddDirectory(subDir.FullName, subDir.Name);
                    AddEntry(zip, rootDir, subDir, recursePaths);
                }
            }
        }
    }
}
