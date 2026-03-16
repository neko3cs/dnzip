using System.IO;

namespace DnZip
{
    public sealed class ArchiveSource(FileSystemInfo source, string entryPath)
    {
        public FileSystemInfo Source { get; } = source;
        public string EntryPath { get; } = entryPath;
    }
}
