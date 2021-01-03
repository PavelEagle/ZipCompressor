using System.IO;
using System.Threading;

namespace ZipCompressor.App.Actions
{
  public interface IReadAction
  {
    void Read(Stream inputStream, CancellationToken token);
  }
}
