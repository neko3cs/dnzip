using System;
using System.IO;

namespace DnZip.Tests
{
    public sealed class TestWorkspace : IDisposable
    {
        public TestWorkspace()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            RootPath = Path.Combine(Path.GetTempPath(), "DnZipTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(RootPath);
        }

        public string RootPath { get; }

        public DirectoryInfo CreateSourceDirectory(string directoryName)
        {
            return Directory.CreateDirectory(Path.Combine(RootPath, directoryName));
        }

        public void Dispose()
        {
            if (Directory.Exists(RootPath))
            {
                Directory.Delete(RootPath, true);
            }
        }
    }
}
