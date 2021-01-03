using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using Serilog;

namespace ZipCompressor.App.Actions
{
  public class ChunkCompressor: IArchiverAction
  {
    private readonly ChunkQueue _inputQueue;
    private readonly ChunkQueue _outputQueue;

    public ChunkCompressor(ChunkQueue inputQueue, ChunkQueue outputQueue)
    {
      _inputQueue = inputQueue;
      _outputQueue = outputQueue;
    }

    public void Start(CancellationToken token)
    {
      _outputQueue.Open();
      var processedStream = new MemoryStream();
      while (!token.IsCancellationRequested)
      {
        try
        {
          var chunk = _inputQueue.Read(token);
          using (var gzipStream = new GZipStream(processedStream, CompressionMode.Compress, leaveOpen: true))
          {
            gzipStream.Write(chunk.Bytes, 0, chunk.Bytes.Length);
          }

          var processedBytes = processedStream.ToArray();
          _outputQueue.Write(new Chunk { Bytes = processedBytes, Index = chunk.Index }, token);
          processedStream.Position = 0;
          processedStream.SetLength(0);
          Log.Information(
            $"Compressed chunk #{chunk.Index} from {chunk.Bytes.Length} bytes to {processedBytes.Length}");
        }
        catch (PipeClosedException)
        {
          Log.Information("Compressing complete");
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
