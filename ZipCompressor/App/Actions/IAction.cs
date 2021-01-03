using System.Threading;

namespace ZipCompressor.App.Actions
{
  public interface IAction
  {
    void Process(CancellationToken token);
  }
}
