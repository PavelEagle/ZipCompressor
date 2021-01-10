using System.IO;
using System.Threading;

namespace ZipCompressor.App.Archiver
{
  public interface IArchiver
  {
    void Read(Stream inputStream, CancellationToken token);
    void StartZipAction(CancellationToken token);
    void Write(Stream outputStream, CancellationToken token, int expectedChunksCount, bool writeChunksLengths);
  }
}
