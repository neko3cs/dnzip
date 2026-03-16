using System.Collections.Generic;
using DnZip;

namespace DnZip.Tests
{
    public sealed class FakePasswordPrompt : IPasswordPrompt
    {
        private readonly Queue<string> _responses;

        public FakePasswordPrompt(params string[] responses)
        {
            _responses = new Queue<string>(responses);
        }

        public List<string> Messages { get; } = new();

        public string ReadPassword(string message)
        {
            Messages.Add(message);
            return _responses.Count > 0 ? _responses.Dequeue() : string.Empty;
        }
    }
}
