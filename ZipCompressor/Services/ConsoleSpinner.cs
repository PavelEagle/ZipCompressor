using System;
using System.Threading;
using Serilog;

namespace ZipCompressor.Services
{
  public class ConsoleSpinner: IDisposable
  {
    private static readonly string[] _sequence = { "     ", "=>   ", "==>  ", "===> ", "====>" };
    private const int Delay = 200;
    private int _counter;

    public ConsoleSpinner()
    {
      Console.CursorVisible = false;
    }

    public void Turn()
    {
      _counter++;

      Thread.Sleep(Delay);

      var counterValue = _counter % 4;

      var fullMessage = "Processing " + _sequence[counterValue];
      var msgLength = fullMessage.Length;

      Console.Write(fullMessage);

      Console.SetCursorPosition(Console.CursorLeft - msgLength, Console.CursorTop);
    }

    public void Dispose()
    {
      Console.Write(_sequence[0]);
      Console.SetCursorPosition(Console.CursorLeft - _sequence[0].Length, Console.CursorTop);

      Console.CursorVisible = true;
    }
  }
}
