using System.Threading;

namespace ZipCompressor.App.Actions.Archive
{
  public interface IArchiveAction
  {
    /// <summary>
    /// Start compress or decompress
    /// </summary>
    /// <param name="token"></param>
    void StartZipAction(CancellationToken token);
  }
}
