using System;
using System.IO;
using DnZip;

namespace DnZip.Tests
{
    public sealed class FakeArchiveService : IArchiveService
    {
        public FileInfo? ArchiveFile { get; private set; }
        public DirectoryInfo? SourceDirectory { get; private set; }
        public bool RecursePaths { get; private set; }
        public string Password { get; private set; } = string.Empty;
        public int CallCount { get; private set; }
        public Exception? ExceptionToThrow { get; set; }

        public void CreateArchive(FileInfo archiveFile, DirectoryInfo sourceDirectory, bool recursePaths, string password)
        {
            CallCount++;
            ArchiveFile = archiveFile;
            SourceDirectory = sourceDirectory;
            RecursePaths = recursePaths;
            Password = password;

            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }
        }
    }
}
