using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ZipCompressor.App
{
  public class TaskQueue
  {
    private readonly Queue<ActionPipeline> _queue;
    private readonly int _maxThreadsCount;
    private readonly List<Thread> _threadPool; 
    private readonly object _lock = new object();

    private volatile bool _isExecuting;

    private int _totalTasks;

    public TaskQueue(int maxThreadsCount)
    {
      _maxThreadsCount = maxThreadsCount;
      _threadPool = new List<Thread>(_maxThreadsCount);
      _queue = new Queue<ActionPipeline>();
    }

    public void AddTask(ActionPipeline task)
    {
      _queue.Enqueue(task);
    }

    public void AddTasks(IEnumerable<ActionPipeline> tasks)
    {
      foreach (var task in tasks)
        _queue.Enqueue(task);
    }

    public void Start()
    {
      _isExecuting = true;

      while (true)
      {
        lock (_lock)
        {
          if (!_isExecuting || (!_queue.Any() && _threadPool.Count == 0)) // Stop execution or all tasks done.
            break;
        }

        if (_threadPool.Count == _maxThreadsCount) // The whole pool is busy with threads.
          continue;

        var task = _queue.Dequeue();
        if (task == null) // All tasks from the queue are pulled out.
          continue;

        task.TaskDone += (sender, args) => { lock (_lock) _threadPool.Remove(args.Thread); };
        _totalTasks++;

        var thread = new Thread(() => task.Execute()) { Name = "GZipTask" + _totalTasks, Priority = ThreadPriority.AboveNormal };
        lock (_lock) _threadPool.Add(thread);
        thread.Start();
      }

      _isExecuting = false;
      _totalTasks = 0;
    }

    public void Stop()
    {
      _isExecuting = false;
      lock (_lock)
      {
        foreach (var thread in _threadPool)
          thread.Abort();
        _threadPool.Clear();

        _queue.Clear();
      }
    }
  }
}
