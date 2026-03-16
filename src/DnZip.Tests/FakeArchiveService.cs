using System;
using System.Collections.Generic;
using System.IO;
using DnZip;

namespace DnZip.Tests
{
    public sealed class FakeArchiveService : IArchiveService
    {
        public FileInfo? ArchiveFile { get; private set; }
        public IReadOnlyList<ArchiveSource>? Sources { get; private set; }
        public bool RecursePaths { get; private set; }
        public string Password { get; private set; } = string.Empty;
        public bool NoDirEntries { get; private set; }
        public int CallCount { get; private set; }
        public Exception? ExceptionToThrow { get; set; }

        public void CreateArchive(
          FileInfo archiveFile,
          IReadOnlyList<ArchiveSource> sources,
          bool recursePaths,
          string password,
          bool noDirEntries)
        {
            CallCount++;
            ArchiveFile = archiveFile;
            Sources = sources;
            RecursePaths = recursePaths;
            Password = password;
            NoDirEntries = noDirEntries;

            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }
        }
    }
}
