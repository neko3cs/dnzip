using System;
using System.IO;
using System.Text;
using Cocona;
using ICSharpCode.SharpZipLib.Checksum;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

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
            [Option('r')] bool recurse,
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

            // FIXME: macOS, Windows 共に標準のアーカイバーで解凍が出来ない
            // FIXME: 1 エントリーしかアーカイブされない(macOS, unzip)
            // FIXME: アーカイブしても空のフォルダしか生成されない(Windows10, Expand-Archive)
            var archiveFile = new FileInfo(archiveFilePath);
            CreateArchive(archiveFile, sourceDirectory, recurse, password);

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
            bool recurse,
            string password
        )
        {
            using (FileStream outputFileStream = File.Create(archiveFile.FullName))
            using (var zipOutputStream = new ZipOutputStream(outputFileStream))
            {
                // zipOutputStream.UseZip64 = UseZip64.Off;
                zipOutputStream.SetLevel(9);
                if (!string.IsNullOrEmpty(password)) zipOutputStream.Password = password;
                var crc = new Crc32();
                CompressDirectory(zipOutputStream, sourceDirectory, crc, recurse);
            }
        }

        private void CompressDirectory(
            ZipOutputStream zipOutputStream,
            DirectoryInfo directory,
            Crc32 crc,
            bool recurse
        )
        {
            foreach (var file in directory.GetFiles())
            {
                var entry = new ZipEntry(
                    ZipEntry.CleanName(file.Name)
                )
                {
                    DateTime = file.LastWriteTime,
                    Size = file.Length
                };
                // var bufferForCrc = File.ReadAllBytes(file.FullName);
                // crc.Reset();
                // crc.Update(bufferForCrc);
                // entry.Crc = crc.Value;
                zipOutputStream.PutNextEntry(entry);
                // zipOutputStream.Write(bufferForCrc, 0, bufferForCrc.Length);

                var bufferForCopy = new byte[4096];
                using (FileStream inputFileStream = File.OpenRead(file.FullName))
                {
                    StreamUtils.Copy(inputFileStream, zipOutputStream, bufferForCopy);
                }
                zipOutputStream.CloseEntry();
            }

            if (recurse)
            {
                foreach (var subDirectory in directory.GetDirectories())
                {
                    CompressDirectory(zipOutputStream, subDirectory, crc, recurse);
                }
            }
        }
    }
}
