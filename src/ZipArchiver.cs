using System;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;

namespace DnZip;

public class ZipArchiver
{
  public void CreateArchive(
              FileInfo archiveFile,
              DirectoryInfo sourceDirectory,
              bool recursePaths,
              string password
          )
  {
    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    using (var zip = new ZipFile(Encoding.GetEncoding("Shift_JIS")))
    {
      zip.CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression;
      if (!string.IsNullOrEmpty(password)) zip.Password = password;
      AddEntry(zip, sourceDirectory, sourceDirectory, recursePaths);
      zip.Save(archiveFile.FullName);
    }
  }

  private void AddEntry(
      ZipFile zip,
      DirectoryInfo rootDir,
      DirectoryInfo targetDir,
      bool recursePaths
  )
  {
    foreach (var file in targetDir.GetFiles())
    {
      zip.AddFile(
          fileName: file.FullName,
          directoryPathInArchive: targetDir.FullName.Substring(rootDir.FullName.Length)
      );
    }
    if (recursePaths)
    {
      foreach (var subDir in targetDir.GetDirectories())
      {
        if (!subDir.GetFiles().Any())
        {
          zip.AddDirectory(subDir.FullName, subDir.Name);
        }
        AddEntry(zip, rootDir, subDir, recursePaths);
      }
    }
  }
}
