using System.Threading;

namespace ZipCompressor.App.Actions.Archive
{
  public interface IArchiveAction
  {
    void StartZipAction(CancellationToken token);
  }
}
