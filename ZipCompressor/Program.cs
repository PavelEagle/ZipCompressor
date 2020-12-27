using System;
using System.Threading;
using ZipCompressor.App;
using ZipCompressor.App.BaseCompressor;
using ZipCompressor.Common;
using ZipCompressor.Services;

namespace ZipCompressor
{
  class Program
  {
    static void Main(string[] args)
    {
      //FileCreator.CreateDummyFile(@"C:\Users\Pavel\Documents\GitHub\ZipCompressor\files\test.txt", 100000000);
#if DEBUG
      var test = new [] { "decompress", @"C:\Users\Pavel\Documents\GitHub\ZipCompressor\files\test.gz", @"C:\Users\Pavel\Documents\GitHub\ZipCompressor\files\111.txt" };
      //var test = new [] { "compress", @"C:\Users\Pavel\Documents\GitHub\ZipCompressor\files\test.txt", @"C:\Users\Pavel\Documents\GitHub\ZipCompressor\files\test.gz" };
#endif

      var zipCompressor = new ZipApplication(CommandOptions.Create(test), new GZipCompressor());

      Console.WriteLine($"Count of cores: {Environment.ProcessorCount}");
      Console.WriteLine("Start...");
      try
      {
        Run(zipCompressor);
      }
      catch 
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine(Constants.UnsuccessfulExecution);
        Environment.Exit(Constants.ErrorExitCode);
      }
#if DEBUG
      finally
      {
        Console.ReadLine();
      }
#endif

      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine(Constants.SuccessfulExecution);
      Console.ForegroundColor = ConsoleColor.White;

      Environment.Exit(Constants.SuccessExitCode);
    }

    public static void Run(ZipApplication archiver)
    {
      var zipAppThread = new Thread(archiver.StartProcess);

      zipAppThread.Start();

      using (var spinner = new ConsoleSpinner())
      {
        while (zipAppThread.IsAlive)
          spinner.Turn();
      }
    }
  }
}
