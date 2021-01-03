using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace ZipCompressor.App
{
  public class Executor
  {
    private WaitHandle[] _waitHandles;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Stopwatch _timer = new Stopwatch();

    private Executor(CancellationTokenSource cancellationTokenSource)
    {
      _cancellationTokenSource = cancellationTokenSource;
      _timer.Start();
    }

    public bool IsErrorOccured { get; private set; }

    public static Executor StartInParallel(Action[] actions, CancellationTokenSource cancellationTokenSource)
    {
      var task = new Executor(cancellationTokenSource);
      var handlesList = new List<WaitHandle>(actions.Length);
      foreach (var action in actions)
      {
        var handle = new EventWaitHandle(false, EventResetMode.ManualReset);
        var thread = new Thread(() =>
        {
          try
          {
            action();
          }
          //catch (Exception ex)
          //{
          //  Console.WriteLine(ex.Message);
          //  task.Abort();
          //}
          finally
          {
            handle.Set();
          }
        });

        handlesList.Add(handle);
        thread.Start();
      }

      task._waitHandles = handlesList.ToArray();
      return task;
    }

    public void Wait()
    {
      WaitHandle.WaitAll(_waitHandles);
      _timer.Stop();
      Log.Debug("");
      Log.Debug($"Task finished in {_timer.Elapsed}");
    }

    public void Abort()
    {
      Log.Error("Aborting ...");
      _cancellationTokenSource.Cancel();
      IsErrorOccured = true;
    }
  }
}
