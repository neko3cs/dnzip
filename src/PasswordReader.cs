using System;
using System.Text;

namespace DnZip;

public class PasswordReader
{
  public bool TryGetPasswordFromConsole(out string password)
  {
    password = string.Empty;

    Console.Write("Enter password: ");
    var pw1 = ConsoleReadPassword();
    Console.Write("Verify password: ");
    var pw2 = ConsoleReadPassword();

    if (!pw1.Equals(pw2)) return false;

    password = pw1;
    return true;
  }

  private string ConsoleReadPassword()
  {
    var password = new StringBuilder();

    while (true)
    {
      var keyinfo = Console.ReadKey(intercept: true);

      if (keyinfo.Key.Equals(ConsoleKey.Enter))
      {
        Console.WriteLine();
        return password.ToString();
      }
      else if (keyinfo.Key.Equals(ConsoleKey.Backspace))
      {
        if (password.Length > 0)
        {
          password.Length -= 1;
          continue;
        }
      }
      else if (Char.IsLetter(keyinfo.KeyChar))
      {
        if ((keyinfo.Modifiers & ConsoleModifiers.Shift) == 0)
        {
          password.Append(keyinfo.KeyChar);
          continue;
        }
        else
        {
          if (Console.CapsLock)
          {
            password.Append(Char.ToLower(keyinfo.KeyChar));
            continue;
          }
          else
          {
            password.Append(Char.ToUpper(keyinfo.KeyChar));
            continue;
          }
        }
      }
      else if (!Char.IsControl(keyinfo.KeyChar))
      {
        password.Append(keyinfo.KeyChar);
        continue;
      }

      Console.Beep();
    }
  }
}
