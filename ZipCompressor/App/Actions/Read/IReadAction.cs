using System.IO;
using System.Threading;

namespace ZipCompressor.App.Actions.Read
{
  public interface IReadAction
  {
    void Read(Stream inputStream, CancellationToken token);
  }
}
