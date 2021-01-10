using System;
using System.IO;
using System.Threading;
using Serilog;

namespace ZipCompressor.App.Actions.Read
{
  public sealed class DecompressChunkReader : IReadAction
  {
    private readonly ChunkQueue _inputQueue;

    public DecompressChunkReader(ChunkQueue inputQueue)
    {
      _inputQueue = inputQueue;
    }

    public void Read(Stream inputStream, CancellationToken token)
    {
      _inputQueue.Connect();
      var lengthBuffer = new byte[4];
      var index = 0;

      try
      {
        int bytesRead;
        while (!token.IsCancellationRequested && (bytesRead = inputStream.Read(lengthBuffer, 0, lengthBuffer.Length)) > 0)
        {
          var chunkLength = BitConverter.ToInt32(lengthBuffer, 0);

          if (chunkLength < 0 || bytesRead < lengthBuffer.Length)
            throw new Exception("Read was Corrupted");

          var buffer = new byte[chunkLength];

          bytesRead = inputStream.Read(buffer, 0, chunkLength);

          if (bytesRead != chunkLength)
            throw new Exception("Read was Corrupted");

          var chunkBytes = new byte[chunkLength];
          Buffer.BlockCopy(buffer, 0, chunkBytes, 0, bytesRead);
          _inputQueue.Write(new Chunk { Bytes = chunkBytes, Index = index }, token);
          Log.Debug($"Reading chunk {index}");
          index++;
        }

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
