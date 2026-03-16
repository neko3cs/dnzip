using System.Text;
using System.Threading.Tasks;
using ConsoleAppFramework;

namespace DnZip
{
    // HACK: 複数ファイル指定に対応
    // HACK: --no-dir-entries(-D) に対応
    public class Program
    {
        static async Task Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var command = new DnZipCommand(
              new SharpromptPasswordPrompt(),
              new ZipArchiveService()
            );

            await ConsoleApp.RunAsync(args, command.Compress);
        }
    }
}
