using System.IO;
using System.Threading;

namespace ZipCompressor.App.Actions
{
  public interface IWriteAction
  {
    void WriteToStream(Stream outputStream, int expectedChunksCount, bool writeChunksLengths, CancellationToken token);
  }
}
