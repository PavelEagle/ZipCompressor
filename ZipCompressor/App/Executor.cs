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
    private readonly Action[] _actions;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Stopwatch _timer = new Stopwatch();

    public Executor(Action[] actions, CancellationTokenSource cancellationTokenSource)
    {
      _actions = actions;
      _cancellationTokenSource = cancellationTokenSource;
      _timer.Start();
    }
    
    public void Start()
    {
      var handlesList = new List<WaitHandle>(_actions.Length);

      foreach (var action in _actions)
      {
        CreateNewThread(action, handlesList);
      }

      _waitHandles = handlesList.ToArray();
    }

    private void CreateNewThread(Action action, ICollection<WaitHandle> manualResetEvents)
    {
      var handle = new ManualResetEvent(false);
      var thread = new Thread(() =>
      {
        try
        {
          action();
        }
        catch (Exception ex)
        {
          Log.Error(ex.Message);
          Abort();
        }
        finally
        {
          handle.Set();
        }
      });

      manualResetEvents.Add(handle);
      thread.Start();
    }

    public void Wait()
    {
      WaitHandle.WaitAll(_waitHandles);
      _timer.Stop();
      Log.Information($"Task finished in {_timer.Elapsed}");
    }

    public void Abort()
    {
      Log.Error("Aborting ...");
      _cancellationTokenSource.Cancel();
    }
  }
}
