using System;
using System.Diagnostics;
using System.Threading;
using Serilog;
using Serilog.Events;
using ZipCompressor.App;
using ZipCompressor.Common;
using ZipCompressor.Services;

namespace ZipCompressor
{
  class Program
  {
    static void Main(string[] args)
    {
#if DEBUG
      args = new[]
      {
        "decompress",
        @"C:\Users\Pavel\Documents\GitHub\ZipCompressor\files\test3.gz",
        @"C:\Users\Pavel\Documents\GitHub\ZipCompressor\files\test3-orig.txt"
        //"compress",
        //@"C:\Users\Pavel\Documents\GitHub\ZipCompressor\files\test3.txt",
        //@"C:\Users\Pavel\Documents\GitHub\ZipCompressor\files\test3.gz"
      };
#endif

      Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.Console(Debugger.IsAttached ? LogEventLevel.Debug : LogEventLevel.Information)
        .CreateLogger();

      SystemInfo.GetSystemInfo();
      SystemInfo.GetCountOfCores();

      var app = new ZipApplication(Options.Validation(args));
      try
      {
        var appThread = new Thread(() => app.Start());
        appThread.Start();

        Console.CancelKeyPress += (obj, e) =>
        {
          app.Stop();
          e.Cancel = true;
        };

        using (var spinner = new ConsoleSpinner())
        {
          while (appThread.IsAlive)
            spinner.Turn();
        }

        appThread.Join();

        Log.Information(app.IsAborted ? "Application has failed" : "Application completed successfully");
        Log.Information(app.IsAborted ? ApplicationConstants.Error.ToString() : ApplicationConstants.Success.ToString());
        Environment.Exit(app.IsAborted ? ApplicationConstants.Error : ApplicationConstants.Success);
      }
      catch (Exception e)
      {
        Log.Error(e.Message);
        Environment.Exit(ApplicationConstants.Error);
      }
    }
  }
}
