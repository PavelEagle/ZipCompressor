using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Serilog;

namespace ZipCompressor.App.Actions.Write
{
  public sealed class ChunksWriter : IWriteAction
  {
    private readonly ChunkQueue _outputQueue;

    public ChunksWriter(ChunkQueue outputQueue)
    {
      _outputQueue = outputQueue;
    }

    public void Write(Stream outputStream, CancellationToken token, int expectedChunksCount, bool writeChunksLengths = false)
    {
      var index = 0;
      var unorderedChunks = new List<Chunk>();

      while (index < expectedChunksCount && !token.IsCancellationRequested)
      {
        try
        {
          if (_outputQueue.TryReadChunk(out var chunk, token))
          {
            if (chunk.Index == index)
            {
              WriteChunk(outputStream, chunk, token, writeChunksLengths);
              index++;

              if (unorderedChunks.Count != 0)
              {
                while ((chunk = unorderedChunks.FirstOrDefault(c => c.Index == index)) != null)
                {
                  unorderedChunks.Remove(chunk);
                  WriteChunk(outputStream, chunk, token, writeChunksLengths);
                  index++;
                }
              }

              continue;
            }

            unorderedChunks.Add(chunk);
          };

          Log.Debug("Writing complete");
        }
        catch (Exception e)
        {
          Log.Debug("Writer failed with error: " + e.Message);
          throw;
        }
      }

      outputStream.Flush();

      if (index != expectedChunksCount)
        throw new Exception("Some chunks were missing");
    }

    private void WriteChunk(Stream outputStream, Chunk chunk, CancellationToken token, bool writeChunksLengths)
    {
      if (token.IsCancellationRequested)
        return;

      if (writeChunksLengths)
        outputStream.Write(BitConverter.GetBytes(chunk.Bytes.Length), 0, 4);

      outputStream.Write(chunk.Bytes, 0, chunk.Bytes.Length);
    }
  }
}
