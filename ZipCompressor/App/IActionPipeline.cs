using ZipCompressor.App.Actions;

namespace ZipCompressor.App
{
  interface IActionPipeline
  {
    void Execute();
    void AddAction(IAction action);
    void Clear();
  }
}
