using System;

namespace DnZip.Tests
{
    public sealed record ArchiveEntryInfo(
      string Name,
      long Size,
      DateTime DateTime,
      bool IsCrypted,
      bool IsDirectory,
      string? Content
    );
}
