using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using Serilog;

namespace ZipCompressor.App.Actions.Archive
{
  public sealed class ChunkCompressor : IArchiveAction
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
      using var bufferedStream = new MemoryStream();

      while (_inputQueue.TryReadChunk(out var chunk, token) && !token.IsCancellationRequested)
      {
        try
        {
          using (var gzipStream = new GZipStream(bufferedStream, CompressionMode.Compress, true))
          {
            gzipStream.Write(chunk.Bytes, 0, chunk.Bytes.Length);
          }

          _outputQueue.WriteChunk(new Chunk { Bytes = bufferedStream.ToArray(), Index = chunk.Index }, token);
          bufferedStream.Position = 0;
          bufferedStream.SetLength(0);
          Log.Debug($"Compressed chunk {chunk.Index}");
        }
        catch (Exception e)
        {
          Log.Error("Compressing failed with error: " + e.Message);
          _outputQueue.Clear();
          throw;
        }

        Log.Debug("Compressing complete");
      }

      _outputQueue.Disconnect();
    }
  }
}