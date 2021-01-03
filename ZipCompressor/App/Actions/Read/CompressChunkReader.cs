using System;
using System.IO;
using System.Threading;
using Serilog;

namespace ZipCompressor.App.Actions.Read
{
  public class CompressChunkReader: IReadAction
  {
    private readonly ChunkQueue _pipe;
    private readonly int _inputQueue;

    public CompressChunkReader(ChunkQueue inputQueue, int chunkSize)
    {
      _pipe = inputQueue;
      _inputQueue = chunkSize;
    }

    public void Read(Stream inputStream, CancellationToken token)
    {
      _pipe.Open();
      var buffer = new byte[_inputQueue];
      var index = 0;

      try
      {
        int bytesRead;
        while (!token.IsCancellationRequested &&
               (bytesRead = inputStream.Read(buffer, 0, _inputQueue)) > 0)
        {
          var chunkBytes = new byte[bytesRead];
          Buffer.BlockCopy(buffer, 0, chunkBytes, 0, bytesRead);
          _pipe.Write(new Chunk { Bytes = chunkBytes, Index = index }, token);
          Log.Debug($"Read chunk #{index}");
          index++;
        }

        Log.Information("Reading complete");
        _pipe.Close();
      }
      catch (Exception e)
      {
        Log.Error("Reading failed with error: " + e.Message);
        _pipe.Close();
        throw;
      }
    }
  }
}
