using System.IO;
using System.Text;
using Ionic.Zip;

namespace DnZip;

public class ZipArchiver
{
  static ZipArchiver()
  {
    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
  }

  public static void CreateArchive(
              FileInfo archiveFile,
              DirectoryInfo sourceDirectory,
              bool recursePaths,
              string password
          )
  {
    using var zip = new ZipFile(Encoding.GetEncoding("Shift_JIS"))
    {
      CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression
    };
    if (!string.IsNullOrEmpty(password)) zip.Password = password;
    AddEntry(zip, sourceDirectory, sourceDirectory, recursePaths);
    zip.Save(archiveFile.FullName);
  }

  private static void AddEntry(
      ZipFile zip,
      DirectoryInfo root,
      DirectoryInfo target,
      bool recursePaths
  )
  {
    foreach (var file in target.GetFiles())
    {
      var entryPath = Path.GetRelativePath(root.FullName, file.FullName);
      zip.AddFile(file.FullName, Path.GetDirectoryName(entryPath));
    }

    if (!recursePaths) return;

    foreach (var subDir in target.GetDirectories())
    {
      if (subDir.GetFiles().Length == 0)
        zip.AddDirectory(subDir.FullName, subDir.Name);

      AddEntry(zip, root, subDir, recursePaths);
    }
  }
}
