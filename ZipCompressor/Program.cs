using System;
using ZipCompressor.BaseCompressor;

namespace ZipCompressor
{
  class Program
  {
    static void Main(string[] args)
    {
      var path = @".\data";
      var zipCompressor = new GZipCompressor(path);

      SystemInfo.getOperatingSystemInfo(); //Call get operating system info method which will display operating system information.
      SystemInfo.getProcessorInfo(); //Call get  processor info method which will display processor info.
      Console.ReadLine();

      Console.WriteLine("Start...");
      zipCompressor.StartCompress();
    }
  }
}
