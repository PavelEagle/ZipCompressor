using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using Serilog;

namespace ZipCompressor.App.Actions.Archive
{
  public class ChunkDecompressor : IArchiveAction
  {
    private readonly ChunkQueue _inputQueue;
    private readonly ChunkQueue _outputQueue;

    public ChunkDecompressor(ChunkQueue inputQueue, ChunkQueue outputQueue)
    {
      _inputQueue = inputQueue;
      _outputQueue = outputQueue;
    }

    public void StartZipAction(CancellationToken token)
    {
      _outputQueue.Open();
      var processedStream = new MemoryStream();
      while (!token.IsCancellationRequested)
      {
        try
        {
          var chunk = _inputQueue.Read(token);
          using (var gzipStream = new GZipStream(new MemoryStream(chunk.Bytes), CompressionMode.Decompress))
          {
            gzipStream.CopyTo(processedStream);
          }

          var processedBytes = processedStream.ToArray();
          _outputQueue.Write(new Chunk { Bytes = processedBytes, Index = chunk.Index }, token);
          processedStream.Position = 0;
          processedStream.SetLength(0);
          Log.Debug($"Decompressed chunk #{chunk.Index} from {chunk.Bytes.Length} bytes to {processedBytes.Length}");
        }
        catch (PipeClosedException)
        {
          Log.Debug("Decompressing complete");
          break;
        }
        catch (Exception e)
        {
          Console.WriteLine(e);
          throw;
        }
      }

      _outputQueue.Close();
    }
  }
}
