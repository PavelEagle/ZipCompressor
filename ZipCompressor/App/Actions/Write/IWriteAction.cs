using System.IO;
using System.Threading;

namespace ZipCompressor.App.Actions.Write
{
  public interface IWriteAction
  {
    void Write(Stream outputStream, CancellationToken token, int expectedChunksCount, bool writeChunksLengths);
  }
}
