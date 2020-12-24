using ZipCompressor.App.Actions;

namespace ZipCompressor.App
{
  interface IActionPipeline
  {
    void Start();
    void AddAction(IAction action);
    void Clear();
  }
}
