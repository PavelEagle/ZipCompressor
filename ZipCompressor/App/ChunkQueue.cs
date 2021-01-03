using System.Collections.Generic;
using System.Threading;

namespace ZipCompressor.App
{
  public class ChunkQueue
  {
    private readonly Queue<Chunk> _queue = new Queue<Chunk>();
    private readonly SemaphoreSlim _readSemaphore;
    private readonly SemaphoreSlim _writeSemaphore;
    private int _registeredWriters;
    private bool _wasEverOpened;

    public ChunkQueue(int maxElements)
    {
      _readSemaphore = new SemaphoreSlim(0, maxElements);
      _writeSemaphore = new SemaphoreSlim(maxElements, maxElements);
    }

    public Chunk Read(CancellationToken token)
    {
      AcquireReadLock(token);
      _writeSemaphore.Release();
      lock (_queue)
      {
        return _queue.Dequeue();
      }
    }

    public void Write(Chunk chunk, CancellationToken token)
    {
      _writeSemaphore.Wait(int.MaxValue, token);
      lock (_queue)
      {
        _queue.Enqueue(chunk);
      }

      _readSemaphore.Release();
    }

    public void Open()
    {
      Interlocked.Increment(ref _registeredWriters);
      _wasEverOpened = true;
    }

    public void Close()
    {
      Interlocked.Decrement(ref _registeredWriters);
    }

    private void AcquireReadLock(CancellationToken token)
    {
      while (true)
      {
        _readSemaphore.Wait(500, token);
        lock (_queue)
        {
          if (_queue.Count == 0)
          {
            if (_registeredWriters == 0 && _wasEverOpened)
            {
              throw new PipeClosedException();
            }

            continue;
          }
        }

        break;
      }
    }
  }
}
