using System.IO;
using System.Threading;

namespace ZipCompressor.App.Actions.Write
{
  public interface IWriteAction
  {
    /// <summary>
    /// Start writing chunks
    /// </summary>
    /// <param name="outputStream"></param>
    /// <param name="token"></param>
    /// <param name="expectedChunksCount"></param>
    /// <param name="writeChunksLengths"></param>
    void Write(Stream outputStream, CancellationToken token, int expectedChunksCount, bool writeChunksLengths);
  }
}
