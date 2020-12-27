using System;
using System.Collections.Generic;
using System.Threading;
using ZipCompressor.App.Actions;
using ZipCompressor.App.BaseCompressor;

namespace ZipCompressor.App
{
  public class ActionPipeline: IActionPipeline
  {
    public event EventHandler<TaskEventArgs> TaskDone;
    private static List<IAction> _actionsPipeline;

    public ActionPipeline(List<IAction> actions)
    {
      _actionsPipeline = actions;
    }

    public void Execute()
    {
      foreach (var action in _actionsPipeline)
        action.Execute();

      Clear();
      TaskDone?.Invoke(this, new TaskEventArgs(Thread.CurrentThread));
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
