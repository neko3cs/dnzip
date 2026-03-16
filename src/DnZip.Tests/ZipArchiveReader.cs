using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace DnZip.Tests
{
    public static class ZipArchiveReader
    {
        public static ArchiveEntryInfo[] GetEntries(string archivePath, string? password = null)
        {
            using var fileStream = File.OpenRead(archivePath);
            using var zipFile = new ZipFile(fileStream);

            if (!string.IsNullOrEmpty(password))
            {
                zipFile.Password = password;
            }

            var entries = new List<ArchiveEntryInfo>();
            foreach (ZipEntry entry in zipFile)
            {
                string? content = null;
                if (!entry.IsDirectory)
                {
                    using var entryStream = zipFile.GetInputStream(entry);
                    using var reader = new StreamReader(entryStream);
                    content = reader.ReadToEnd();
                }

                entries.Add(new ArchiveEntryInfo(entry.Name, entry.Size, entry.DateTime, entry.IsCrypted, entry.IsDirectory, content));
            }

            return entries.ToArray();
        }
    }
}
