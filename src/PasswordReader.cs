using System;
using System.Text;

namespace DnZip;

public class PasswordReader
{
  public bool TryGetPasswordFromConsole(out string password)
  {
    password = string.Empty;
    Console.Write("Enter password: ");
    var pw1 = ReadPassword();
    Console.Write("Verify password: ");
    var pw2 = ReadPassword();

    if (!pw1.Equals(pw2)) return false;

    password = pw1;
    return true;
  }

  private static string ReadPassword()
  {
    var password = new StringBuilder();
    ConsoleKeyInfo key;
    while ((key = Console.ReadKey(true)).Key != ConsoleKey.Enter)
    {
      if (key.Key == ConsoleKey.Backspace && password.Length > 0)
        password.Length--;
      else if (!char.IsControl(key.KeyChar))
        password.Append(key.KeyChar);
    }
    Console.WriteLine();
    return password.ToString();
  }
}
