using System;
using System.IO;
using System.Threading;
using Serilog;

namespace ZipCompressor.App.Actions.Write
{
  public sealed class ChunksWriter: IWriteAction
  {
    private readonly ChunkQueue _outputQueue;

    public ChunksWriter(ChunkQueue outputQueue)
    {
      _outputQueue = outputQueue;
    }

    public void Write(Stream outputStream, CancellationToken token, int expectedChunksCount, bool writeChunksLengths = false)
    {
      var index = 0;
      while (!token.IsCancellationRequested)
      {
        try
        {
          var chunk = _outputQueue.Read(token);
          WriteChunk(outputStream, chunk, token, writeChunksLengths);
          index++;

        }
        catch (ChunkQueueCompleted)
        {
          Log.Debug("Writing complete");
          break;
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
