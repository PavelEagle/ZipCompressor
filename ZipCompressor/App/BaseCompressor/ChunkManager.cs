using System;
using System.Collections.Generic;

namespace ZipCompressor.App.BaseCompressor
{
  public class ChunkManager
  {
    private readonly Dictionary<int, byte[]> _chunks;
    private readonly object _lock = new object();

    public ChunkManager()
    {
      _chunks = new Dictionary<int, byte[]>(0);
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

    public byte[] Get(int index, bool remove = true)
    {
      lock (_lock)
      {
        if (!_chunks.ContainsKey(index))
          throw new ArgumentException("Chunk is not exist");

        var chunk = _chunks[index];
        if (remove)
          _chunks.Remove(index);

        return chunk;
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
