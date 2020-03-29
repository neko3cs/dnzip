using System;
using System.IO;
using System.Text;
using Cocona;
using ICSharpCode.SharpZipLib.Zip;

namespace DnZip
{
    class Program
    {
        static void Main(string[] args)
        {
            CoconaLiteApp.Run<Program>(args);
        }

        [PrimaryCommand]
        public int CompressZipFile(
            [Argument]string path,
            [Option('r')]bool recursePaths,
            [Option('e')]bool encrypt
        )
        {
            if (string.IsNullOrEmpty(path)) return 1;
            var targetDirectory = new DirectoryInfo(path);
            if (!targetDirectory.Exists)
            {
                Console.WriteLine("Error: Path not found.");
                return 1;
            }

            var zip = new FastZip();
            if (encrypt)
            {
                var password = GetPasswordFromConsole();
                if (string.IsNullOrEmpty(password))
                {
                    Console.WriteLine("Error: Password verification failed.");
                    return 1;
                }
                zip.Password = password;
            }
            zip.CreateZip(
                zipFileName: Path.Combine(targetDirectory.Parent.FullName, $"{targetDirectory.Name}.zip"),
                targetDirectory.FullName,
                recurse: recursePaths,
                fileFilter: null,
                directoryFilter: null
            );

            return 0;
        }

        private string GetPasswordFromConsole()
        {
            Console.Write("Enter password: ");
            var pw1 = ConsoleReadPassword();
            Console.Write("Verify password: ");
            var pw2 = ConsoleReadPassword();
            if (!pw1.Equals(pw2))
            {
                return string.Empty;
            }
            return pw1;
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
    }
}
