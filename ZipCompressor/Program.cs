using System;
using System.IO;
using System.Reflection;
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
      Console.WriteLine("Enter");

      var test = new [] {"compress", "input.txt", "output.gz"};

      //FileCreator.CreateDummyFile(path + @"\Test2.txt", 1000000);
      var zipCompressor = new ZipApplication(CommandOptions.Create(test), new GZipCompressor());


      SystemInfo.getOperatingSystemInfo(); //Call get operating system info method which will display operating system information.
      SystemInfo.getProcessorInfo(); //Call get  processor info method which will display processor info.

      Console.WriteLine("Start...");
      zipCompressor.StartProcess();
    }

    public void Run(ZipApplication _archiver)
    {
     bool isTerminated = false;

     var archiverThread =
        new Thread(
          _archiver.StartProcess)
        {
          Name = "GZipArchiver",
          Priority = ThreadPriority.AboveNormal
        };
      archiverThread.Start();

      OutputResult();
    }

    private void OutputResult()
    {
      Console.WriteLine("Done!");
    }
  }
}
