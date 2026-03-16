using Sharprompt;

namespace DnZip
{
    public sealed class SharpromptPasswordPrompt : IPasswordPrompt
    {
        public string ReadPassword(string message)
        {
            return Prompt.Password(message);
        }
    }
}
