using System.Threading;

namespace ZipCompressor.App.Actions
{
  public interface IArchiverAction
  {
    void Start(CancellationToken token);
  }
}
