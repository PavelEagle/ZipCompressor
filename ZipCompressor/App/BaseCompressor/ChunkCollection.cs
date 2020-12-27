using System;
using System.Collections.Generic;

namespace ZipCompressor.App.BaseCompressor
{
  public class ChunkCollection: IChunkCollection
  {
    private readonly Dictionary<int, byte[]> _chunks;
    private readonly object _lock = new object();

    public ChunkCollection()
    {
      _chunks = new Dictionary<int, byte[]>();
    }

    public void Add(int index, byte[] bytes)
    {
      lock (_lock)
      {
        if (_chunks.ContainsKey(index))
          throw new ArgumentException("Already exists");

        _chunks.Add(index, bytes);
      }
    }

    public void Remove(int index)
    {
      lock (_lock)
      {
        if (!_chunks.ContainsKey(index))
          throw new ArgumentException("Chunk is not exist");

        _chunks.Remove(index);
      }
    }

    public byte[] Get(int index)
    {
      lock (_lock)
      {
        if (_chunks.TryGetValue(index, out var result))
          return result;

        throw new ArgumentException("Chunk is not exist");
      }
    }

    public int Count()
    {
      lock (_lock)
      {
        return _chunks.Count;
      }
    }

    public void Clear()
    {
      lock (_lock)
      {
        _chunks.Clear();
      }
    }
  }
}
