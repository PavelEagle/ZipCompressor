using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Serilog;

namespace ZipCompressor.App.Actions
{
  public class ChunksWriter
  {
    private readonly ChunkQueue _outputQueue;

    public ChunksWriter(ChunkQueue outputQueue)
    {
      _outputQueue = outputQueue;
    }

    public void WriteToStream(Stream outputStream,
        CancellationToken token,
        int expectedChunksCount,
        bool writeChunksLengths = false)
    {
      var index = 0;
      var unorderedChunks = new List<Chunk>();
      while (!token.IsCancellationRequested)
      {
        try
        {
          var chunk = _outputQueue.Read(token);
          Log.Information($"Requested to write chunk #{chunk.Index}, expecting #{index}");
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
        catch (Exception)//TODO exception
        {
          Log.Information("Writing complete");
          outputStream.Flush();
          if (unorderedChunks.Count > 0)
          {
            throw new Exception("Some chunks were missing and some are left");
          }

          if (index != expectedChunksCount)
          {
            throw new Exception(); //TODO exception
          }

          break;
        }
        //catch (Exception e)
        //{
        //  _logger.WriteError("Writer failed with error: " + e.Message);
        //  throw;
        //}
      }
    }

    private void WriteChunk(
        Stream outputStream, Chunk chunk, CancellationToken token, bool writeChunksLengths)
    {
      if (token.IsCancellationRequested)
      {
        return;
      }

      Log.Information($"Writing chunk #{chunk.Index} of {chunk.Bytes.Length} bytes");
      if (writeChunksLengths)
      {
        outputStream.Write(BitConverter.GetBytes(chunk.Bytes.Length), 0, sizeof(int));
      }

      outputStream.Write(chunk.Bytes, 0, chunk.Bytes.Length);
    }
  }
}
