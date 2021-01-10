using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Serilog;

namespace ZipCompressor.App.Actions.Write
{
  public class ChunksWriter: IWriteAction
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
      while (!token.IsCancellationRequested)
      {
        try
        {
          var chunk = _outputQueue.Read(token);
          if (chunk.Index != index)
          {
            unorderedChunks.Add(chunk);
            continue;
          }

          WriteChunk(outputStream, chunk, token, writeChunksLengths);
          index++;
          while ((chunk = unorderedChunks.FirstOrDefault(c => c.Index == index)) != null)
          {
            unorderedChunks.Remove(chunk);
            WriteChunk(outputStream, chunk, token, writeChunksLengths);
            index++;
          }
        }
        catch (PipeClosedException) //TODO exception
        {
          break;
        }
        catch (Exception e)
        {
          Log.Debug("Writer failed with error: " + e.Message);
          throw;
        }
      }
      Log.Debug("Writing complete");
      outputStream.Flush();
      if (unorderedChunks.Count > 0 || index != expectedChunksCount)
      {
        throw new Exception("Some chunks were missing");
      }
    }

    private void WriteChunk(Stream outputStream, Chunk chunk, CancellationToken token, bool writeChunksLengths)
    {
      if (token.IsCancellationRequested)
      {
        return;
      }

      Log.Debug($"Writing chunk #{chunk.Index}");
      if (writeChunksLengths)
      {
        outputStream.Write(BitConverter.GetBytes(chunk.Bytes.Length), 0, sizeof(int));
      }

      outputStream.Write(chunk.Bytes, 0, chunk.Bytes.Length);
    }
  }
}
