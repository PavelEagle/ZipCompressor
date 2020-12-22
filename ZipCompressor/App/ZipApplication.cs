using System;
using ZipCompressor.App.BaseCompressor;
using ZipCompressor.Common;

namespace ZipCompressor.App
{
  public class ZipApplication
  {
    private readonly IArchiver _compressor;
    private readonly CommandOptions _zipSettings;

    public ZipApplication(CommandOptions zipSettings, IArchiver compressor)
    {
      _zipSettings = zipSettings;
      _compressor = compressor;
    }

    public void StartProcess()
    {
      switch (_zipSettings.Mode)
      {
        case Commands.Compression:
          _compressor.StartCompress(_zipSettings.InputFileName, _zipSettings.OutputFileName, Constants.DefaultByteBufferSize*1024);
          break;
        case Commands.Decompression:
          _compressor.StartDecompress(_zipSettings.InputFileName, _zipSettings.OutputFileName, Constants.DefaultByteBufferSize * 1024);
          break;
        case Commands.Create:
          break;
        default:
          throw new ArgumentException("Unknown operation type");
      }
    }

    public void StopProcess()
    {
    }
  }
}
