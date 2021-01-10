using System;
using System.Management;
using Serilog;

namespace ZipCompressor.Services
{
  public class SystemInfo
  {
    public static void GetSystemInfo()
    {
      Log.Information("Displaying operating system info....");
      var managementObjects = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
      foreach (var managementObject in managementObjects.Get())
      {
        if (managementObject["Caption"] != null)
        {
          Log.Information("Operating System Name  :  " + managementObject["Caption"]);   //Display operating system caption
        }
        if (managementObject["OSArchitecture"] != null)
        {
          Log.Information("Operating System Architecture  :  " + managementObject["OSArchitecture"]);   //Display operating system architecture.
        }
        if (managementObject["CSDVersion"] != null)
        {
          Log.Information("Operating System Service Pack   :  " + managementObject["CSDVersion"]);     //Display operating system version.
        }
      }
    }

    public static void GetCountOfCores()
    {
      Log.Information($"Count of cores: {Environment.ProcessorCount}");
    }
  }
}
