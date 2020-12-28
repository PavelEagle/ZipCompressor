using System.IO;
using System.IO.Compression;

namespace ZipCompressor.App.BaseCompressor
{
  class GZipCompressor : IArchiver
  {
    public byte[] Compress(byte[] originalBytes)
    {
      using var outStream = new MemoryStream();
      using var gZipStream = new GZipStream(outStream, CompressionMode.Compress);

      gZipStream.Write(originalBytes, 0, originalBytes.Length);

      return outStream.ToArray();
    }

    public byte[] Decompress(byte[] compressedBytes)
    {
      using var inputStream = new MemoryStream(compressedBytes);
      using var gZipStream = new GZipStream(inputStream, CompressionMode.Decompress);
      using var outStream = new MemoryStream();

      gZipStream.CopyTo(outStream);

      return outStream.ToArray();
    }

  }
}

