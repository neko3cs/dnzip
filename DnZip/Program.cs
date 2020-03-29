using System;
using System.IO;
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

            var password = string.Empty;
            if (encrypt)
            {
                Console.Write("Enter password: ");
                var pw1 = Console.ReadLine();
                Console.Write("Verify password: ");
                var pw2 = Console.ReadLine();
                if (!pw1.Equals(pw2))
                {
                    Console.WriteLine("Error: Password verification failed.");
                    return 1;
                }
                password = pw1;
            }

            var zip = new FastZip();
            if (encrypt) zip.Password = password;
            zip.CreateZip(
                zipFileName: Path.Combine(targetDirectory.Parent.FullName, $"{targetDirectory.Name}.zip"),
                targetDirectory.FullName,
                recurse: recursePaths,
                fileFilter: null,
                directoryFilter: null
            );

            return 0;
        }
    }
}
