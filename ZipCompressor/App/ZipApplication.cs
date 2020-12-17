using ZipCompressor.App.BaseCompressor;
using ZipCompressor.Common;

namespace ZipCompressor.App
{
  public class ZipApplication
  {
    private readonly ICompressor _compressor;
    private readonly ZipSettings _zipSettings;

    public ZipApplication(ICompressor compressor, ZipSettings zipSettings)
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
