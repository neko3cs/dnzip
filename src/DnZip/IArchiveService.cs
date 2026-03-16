using System.IO;

namespace DnZip
{
    public interface IArchiveService
    {
        void CreateArchive(
          FileInfo archiveFile,
          DirectoryInfo sourceDirectory,
          bool recursePaths,
          string password
        );
    }
}
