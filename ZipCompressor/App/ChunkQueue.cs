using System.Collections.Generic;
using System.Threading;

namespace ZipCompressor.App
{
  public class ChunkQueue
  {
    private readonly Queue<Chunk> _queue = new Queue<Chunk>();
    private readonly SemaphoreSlim _readSemaphore;
    private readonly SemaphoreSlim _writeSemaphore;
    private readonly object _lock = new object();
    private int _activeWriters;
    private bool _wasEverOpened;

    public ChunkQueue(int maxElements)
    {
      _readSemaphore = new SemaphoreSlim(0, maxElements);
      _writeSemaphore = new SemaphoreSlim(maxElements, maxElements);
    }

    public Chunk Read(CancellationToken token)
    {

      while (true)
      {
        if (_activeWriters == 0 && _wasEverOpened)
          throw new ChunkQueueCompleted();

        _readSemaphore.Wait(400, token);
        lock (_lock)
        {
          if (_queue.Count == 0)
            continue;
        }

        break;
      }
      _writeSemaphore.Release();
      lock (_lock)
      {
        return _queue.Dequeue();
      }
    }

    public void Write(Chunk chunk, CancellationToken token)
    {
      _writeSemaphore.Wait(30000, token);
      lock (_lock)
      {
        _queue.Enqueue(chunk);
      }

      _readSemaphore.Release();
    }

    /// <summary>
    /// Connect to queue
    /// </summary>
    public void Connect()
    {
      _wasEverOpened = true;
      Interlocked.Increment(ref _activeWriters);
    }

    /// <summary>
    /// Disconnect from queue
    /// </summary>
    public void Disconnect()
    {
      Interlocked.Decrement(ref _activeWriters);
    }

    public void Clear()
    {
      lock (_lock)
      {
        _queue.Clear();
      }
    }
  }
}
