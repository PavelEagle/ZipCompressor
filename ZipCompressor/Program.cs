using System;
using System.Diagnostics;
using Serilog;
using Serilog.Events;
using ZipCompressor.App;
using ZipCompressor.Common;

namespace ZipCompressor
{
  class Program
  {
    static void Main(string[] args)
    {

#if DEBUG
      args = new[]
      {
        "compress",
        @"C:\Users\Pavel\Documents\GitHub\ZipCompressor\files\test3.txt",
        @"C:\Users\Pavel\Documents\GitHub\ZipCompressor\files\test3.gz"
      };
#endif

      Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.Console(Debugger.IsAttached ? LogEventLevel.Debug : LogEventLevel.Information)
        .CreateLogger();

      Log.Information($"Count of cores: {Environment.ProcessorCount}");
      
      using (var app = new ZipApplication(CommandOptions.Create(args)))
      {
        try
        {
          app.Start();
          Console.CancelKeyPress += (obj, e) =>
          {
            app.Stop();
            e.Cancel = true;
          };

          Log.Information("Execution finished " +
                          (app.IsErrorOccured ? "with errors or was aborted" : "successfully"));
          Environment.Exit(app.IsErrorOccured ? ApplicationConstants.Error : ApplicationConstants.Success);
        }
        catch (Exception e)
        {
          Log.Error(e.Message);
          Environment.Exit(ApplicationConstants.Error); 
        }
      }
    }
  }
}
