using System.IO;
using System.Threading;

namespace ZipCompressor.App.Actions.Read
{
  public interface IReadAction
  {
    /// <summary>
    /// Start reading chunks
    /// </summary>
    /// <param name="inputStream"></param>
    /// <param name="token"></param>
    void Read(Stream inputStream, CancellationToken token);
  }
}
