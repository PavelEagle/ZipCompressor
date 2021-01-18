using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using Serilog;

namespace ZipCompressor.App.Actions.Archive
{
  public sealed class ChunkDecompressor : IArchiveAction
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
      _outputQueue.Connect();
      using var bufferedStream = new MemoryStream();

      while (_inputQueue.TryReadChunk(out var chunk, token) && !token.IsCancellationRequested)
      {
        try
        {
          using (var gzipStream = new GZipStream(new MemoryStream(chunk.Bytes), CompressionMode.Decompress))
          {
            gzipStream.CopyTo(bufferedStream);
          }

          _outputQueue.Write(new Chunk { Bytes = bufferedStream.ToArray(), Index = chunk.Index }, token);
          bufferedStream.Position = 0;
          bufferedStream.SetLength(0);
          Log.Debug($"Decompressed chunk {chunk.Index}");
        }
        catch (Exception e)
        {
          Log.Error("Decompressing failed with error: " + e.Message);
          _outputQueue.Clear();
          throw;
        }
      }

      Log.Debug("Decompressing complete");
      _outputQueue.Disconnect();
    }
  }
}