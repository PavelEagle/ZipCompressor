using System;
using System.IO;
using System.Threading;
using Serilog;

namespace ZipCompressor.App.Actions.Read
{
  public sealed class CompressChunkReader: IReadAction
  {
    private readonly ChunkQueue _inputQueue; 
    private readonly int _chunkSize;

    public CompressChunkReader(ChunkQueue inputQueue, int chunkSize)
    {
      _inputQueue = inputQueue;
      _chunkSize = chunkSize;
    }

    public void Read(Stream inputStream, CancellationToken token)
    {
      _inputQueue.Connect();
      var buffer = new byte[_chunkSize];
      var index = 0;

      try
      {
        int bytesRead;
        while (!token.IsCancellationRequested && (bytesRead = inputStream.Read(buffer, 0, _chunkSize)) > 0)
        {
          var chunkBytes = new byte[bytesRead];
          Buffer.BlockCopy(buffer, 0, chunkBytes, 0, bytesRead);
          _inputQueue.WriteChunk(new Chunk { Bytes = chunkBytes, Index = index }, token);
          Log.Debug($"Reading chunk {index}");
          index++;
        }

        Log.Debug("Reading complete");
        _inputQueue.Disconnect();
      }
      catch (Exception e)
      {
        Log.Error("Reading failed: " + e.Message);
        _inputQueue.Clear();
        throw;
      }
    }
  }
}
