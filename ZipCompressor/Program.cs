using System;
using System.IO;
using System.Reflection;
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
      var path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @"\data";
      Console.WriteLine("Enter");

      //FileCreator.CreateDummyFile(path + @"\Test2.txt", 1000000);
      var zipCompressor = new ZipApplication(new GZipCompressor(path), CommandOptions.Create(args));

      SystemInfo.getOperatingSystemInfo(); //Call get operating system info method which will display operating system information.
      SystemInfo.getProcessorInfo(); //Call get  processor info method which will display processor info.

      Console.WriteLine("Start...");
      zipCompressor.StartCompress();
    }
  }
}
