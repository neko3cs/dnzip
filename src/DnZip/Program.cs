using System.Text;
using System.Threading.Tasks;
using ConsoleAppFramework;

namespace DnZip
{
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
