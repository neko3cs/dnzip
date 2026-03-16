using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace DnZip
{
    public sealed class ZipArchiveService : IArchiveService
    {
        public void CreateArchive(
          FileInfo archiveFile,
          DirectoryInfo sourceDirectory,
          bool recursePaths,
          string password
        )
        {
            using var fsOut = File.Create(archiveFile.FullName);
            var encoding = Encoding.GetEncoding("Shift_JIS");

            using var zipStream = new ZipOutputStream(fsOut, encoding.CodePage);

            if (!string.IsNullOrEmpty(password))
            {
                zipStream.Password = password;
            }

            zipStream.SetLevel(9);

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
