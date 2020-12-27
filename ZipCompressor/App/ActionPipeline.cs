using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ZipCompressor.App.Actions;

namespace ZipCompressor.App
{
  public class ActionPipeline : IActionPipeline
  {
    public event EventHandler<TaskEventArgs> TaskDone;
    private static IList<IAction> _actionsPipeline;
    private readonly int _chunkIndex;

    public ActionPipeline(IList<IAction> actions, int chunkIndex)
    {
      _actionsPipeline = actions;
      _chunkIndex = chunkIndex;
    }
    public void Execute()
    {
      foreach (var action in _actionsPipeline.ToList())
      {
        action.Execute(_chunkIndex);
      }
      
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
