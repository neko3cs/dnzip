using System;
using System.Diagnostics;
using System.IO;
using Cocona;
using ICSharpCode.SharpZipLib.Zip;

namespace DnZip
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: 1: 無階層で zip 出来る機能の実装
            // TODO: 2: 1 に暗号化機能を実装
            // TODO: 3: フォルダ階層を維持して zip 出来る機能の実装
            CoconaLiteApp.Run<Program>(args);
        }

        [PrimaryCommand]
        public int CompressZipFile(
            [Argument]string path,
            [Option('e')]bool encrypt
        )
        {
            if (string.IsNullOrEmpty(path)) return 1;
            var zipDir = new DirectoryInfo(path);
            if (!zipDir.Exists)
            {
                Console.WriteLine("Error: Path not found.");
                return 1;
            }

            var password = string.Empty;
            if (encrypt)
            {
                Console.Write("Enter password: ");
                var p1 = Console.ReadLine();
                Console.Write("Verify password: ");
                var p2 = Console.ReadLine();
                if (!p1.Equals(p2))
                {
                    Console.WriteLine("Error: Password verification failed.");
                    return 1;
                }
            }

            using (var memoryStream = new MemoryStream())
            using (var zipOutputStream = new ZipOutputStream(memoryStream))
            {
                zipOutputStream.SetLevel(9);
                var buffer = new byte[4096];

                foreach (var file in zipDir.GetFiles())
                {
                    var entry = new ZipEntry(file.Name);
                    entry.DateTime = DateTime.Now;
                    zipOutputStream.PutNextEntry(entry);

                    using (var fileStream = File.OpenRead(file.FullName))
                    {
                        var sourceBytes = 0;
                        do
                        {
                            sourceBytes = fileStream.Read(buffer, 0, buffer.Length);
                            zipOutputStream.Write(buffer, 0, sourceBytes);
                        } while (sourceBytes > 0);
                    }
                }

                zipOutputStream.Finish();
                zipOutputStream.Close();

                var zipFileFullName = Path.Combine(zipDir.Parent.FullName, $"{zipDir.Name}.zip");
                File.WriteAllBytes(zipFileFullName, memoryStream.ToArray());
            }

            return 0;
        }
    }
}
