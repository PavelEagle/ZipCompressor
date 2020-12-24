using System.Collections.Generic;
using ZipCompressor.App.Actions;

namespace ZipCompressor.App
{
  public class ActionPipeline: IActionPipeline
  {
    private static List<IAction> _actionsPipeline;

    public ActionPipeline(List<IAction> actions)
    {
      _actionsPipeline = actions;
    }
    public ActionPipeline() { }

    public void Start()
    {
      foreach (var action in _actionsPipeline)
        action.Execute();

      Clear();
    }

    public void AddAction(IAction action)
    {
      _actionsPipeline.Add(action);
    }

    public void Clear()
    {
      _actionsPipeline.Clear();
    }
  }
}
