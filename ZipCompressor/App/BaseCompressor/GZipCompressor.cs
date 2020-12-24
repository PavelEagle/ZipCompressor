using System.IO;
using System.IO.Compression;

namespace ZipCompressor.App.BaseCompressor
{
  class GZipCompressor: IArchiver
  {
    public byte[] Compress(byte[] originalBytes)
    {
      using (var output = new MemoryStream())
      {
        using (var compressStream = new GZipStream(output, CompressionMode.Compress))
        {
          compressStream.Write(originalBytes, 0, originalBytes.Length);
        }

        return output.ToArray();
      }
    }

    public byte[] Decompress(byte[] compressedBytes)
    {
      using (var output = new MemoryStream())
      {
        using (var input = new MemoryStream(compressedBytes))
        {
          using (var decompressStream = new GZipStream(input, CompressionMode.Decompress))
          {
            decompressStream.CopyTo(output);
          }

          return output.ToArray();
        }
      }
    }
  }
}
