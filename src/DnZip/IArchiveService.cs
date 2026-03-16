using System.Collections.Generic;
using System.IO;

namespace DnZip
{
    public interface IArchiveService
    {
        void CreateArchive(
          FileInfo archiveFile,
          IReadOnlyList<ArchiveSource> sources,
          bool recursePaths,
          string password
        );
    }
}
