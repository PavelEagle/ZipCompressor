using ZipCompressor.App.BaseCompressor;
using ZipCompressor.Common;

namespace ZipCompressor.App
{
  public class ZipApplication
  {
    private readonly ICompressor _compressor;
    private readonly CommandOptions _zipSettings;

    public ZipApplication(ICompressor compressor, CommandOptions zipSettings)
    {
      _compressor = compressor;
      _zipSettings = zipSettings;
    }

    public void StartCompress()
    {
      _compressor.StartCompress();
    }

    public void StartDecompress()
    {
      _compressor.StartDecompress();
    }
  }
}
