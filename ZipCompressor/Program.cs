using System;
using Serilog;
using Serilog.Events;
using ZipCompressor.App;
using ZipCompressor.Common;

namespace ZipCompressor
{
  class Program
  {
    static int Main(string[] args)
    {

#if DEBUG
      args = new[]
      {
        "decompress", 
        @"C:\Users\Pavel\Documents\GitHub\ZipCompressor\files\test3.gz",
        @"C:\Users\Pavel\Documents\GitHub\ZipCompressor\files\test3-orig.txt"
      };
#endif

      Console.WriteLine($"Count of cores: {Environment.ProcessorCount}");

      Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.Console(LogEventLevel.Debug)
        .CreateLogger();

      var app = new Application(CommandOptions.Create(args));

      try
      {
        var task = app.Start();
        Console.CancelKeyPress += (_, cancelEventArgs) =>
        {
          cancelEventArgs.Cancel = true;
          task.Abort();
        };

        task.Wait();
        Log.Information("Execution finished " +
                        (task.IsErrorOccured ? "with errors or was aborted" : "successfully"));
        return task.IsErrorOccured ? ApplicationConstants.Error : ApplicationConstants.Success;
      }
      catch (Exception e)
      {
        Log.Error(e.Message);
        return ApplicationConstants.Error;
      }
    }
  }
}
