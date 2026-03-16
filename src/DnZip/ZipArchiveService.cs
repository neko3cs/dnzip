using System.Collections.Generic;
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
          IReadOnlyList<ArchiveSource> sources,
          bool recursePaths,
          string password,
          bool noDirEntries
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

            foreach (var source in sources)
            {
                if (source.Source is FileInfo file)
                {
                    AddFileEntry(zipStream, file, source.EntryPath);
                    continue;
                }

                if (source.Source is DirectoryInfo directory)
                {
                    AddDirectoryEntry(zipStream, directory, source.EntryPath, recursePaths, noDirEntries);
                }
            }

            zipStream.Finish();
            zipStream.Close();
        }

        private static void AddDirectoryEntry(
          ZipOutputStream zipStream,
          DirectoryInfo directory,
          string entryPath,
          bool recursePaths,
          bool noDirEntries
        )
        {
            foreach (var file in directory.GetFiles())
            {
                var fileEntryPath = ZipEntry.CleanName(Path.Combine(entryPath, file.Name));
                AddFileEntry(zipStream, file, fileEntryPath);
            }

            if (!recursePaths) return;

            if (!noDirEntries)
            {
                AddDirectoryMarkerEntry(zipStream, directory, entryPath);
            }

            foreach (var subDirectory in directory.GetDirectories())
            {
                var subDirectoryEntryPath = ZipEntry.CleanName(Path.Combine(entryPath, subDirectory.Name));
                AddDirectoryEntry(zipStream, subDirectory, subDirectoryEntryPath, recursePaths, noDirEntries);
            }
        }

        private static void AddFileEntry(ZipOutputStream zipStream, FileInfo file, string entryPath)
        {
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

        private static void AddDirectoryMarkerEntry(ZipOutputStream zipStream, DirectoryInfo directory, string entryPath)
        {
            var directoryEntryPath = entryPath.EndsWith("/") ? entryPath : $"{entryPath}/";
            var dirEntry = new ZipEntry(directoryEntryPath)
            {
                DateTime = directory.LastWriteTime,
                Size = 0
            };

            zipStream.PutNextEntry(dirEntry);
            zipStream.CloseEntry();
        }
    }
}
