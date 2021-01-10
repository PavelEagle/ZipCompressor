using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using Serilog;

namespace ZipCompressor.App.Actions.Archive
{
  public class ChunkCompressor: IArchiveAction
  {
    private readonly ChunkQueue _inputQueue;
    private readonly ChunkQueue _outputQueue;

    public ChunkCompressor(ChunkQueue inputQueue, ChunkQueue outputQueue)
    {
      _inputQueue = inputQueue;
      _outputQueue = outputQueue;
    }

    public void StartZipAction(CancellationToken token)
    {
      _outputQueue.Connect();

      while (!token.IsCancellationRequested)
      {
        try
        {
          var chunk = _inputQueue.Read(token);
          using var processedStream = new MemoryStream();
          using var gzipStream = new GZipStream(processedStream, CompressionMode.Compress, true);
          gzipStream.Write(chunk.Bytes, 0, chunk.Bytes.Length);

          var processedBytes = processedStream.ToArray();
          _outputQueue.Write(new Chunk {Bytes = processedBytes, Index = chunk.Index}, token);
          processedStream.Position = 0;
          processedStream.SetLength(0);
          Log.Debug($"Compressed chunk #{chunk.Index} from {chunk.Bytes.Length} bytes to {processedBytes.Length}");
        }
        catch (PipeClosedException)
        {
          Log.Debug("Compressing complete");
          break;
        }
        catch (Exception e)
        {
          Log.Error("Compressing failed with error: " + e.Message);
          _outputQueue.Close();
          throw;
        }
      }

      _outputQueue.Close();
    }
  }
}
