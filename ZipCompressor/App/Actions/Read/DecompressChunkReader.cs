using System;
using System.IO;
using System.Threading;
using Serilog;

namespace ZipCompressor.App.Actions.Read
{
  public class DecompressChunkReader: IReadAction
  {
    private readonly ChunkQueue _inputQueue;
    private readonly int _chunkSize;

    public DecompressChunkReader(ChunkQueue inputQueue, int chunkSize)
    {
      _inputQueue = inputQueue;
      _chunkSize = chunkSize;
    }

    public void Read(Stream inputStream, CancellationToken token)
    {
      _inputQueue.Connect();
      var lengthBuffer = new byte[4];
      var buffer = new byte[0];
      var maxChunkLength = 0;
      var index = 0;

      try
      {
        int bytesRead;
        while (!token.IsCancellationRequested && (bytesRead = inputStream.Read(lengthBuffer, 0, lengthBuffer.Length)) > 0)
        {
          if (bytesRead < lengthBuffer.Length)
          {
            throw new Exception("Read was Corrupted");
          }

          var chunkLength = BitConverter.ToInt32(lengthBuffer, 0);
          if (chunkLength < 0 || chunkLength > _chunkSize * 10)
          {
            throw new Exception("Read was Corrupted");
          }

          if (chunkLength > maxChunkLength)
          {
            maxChunkLength = chunkLength;
            buffer = new byte[maxChunkLength];
          }

          bytesRead = inputStream.Read(buffer, 0, chunkLength);
          if (bytesRead != chunkLength)
          {
            throw new Exception("Read was Corrupted");
          }

          var chunkBytes = new byte[chunkLength];
          Buffer.BlockCopy(buffer, 0, chunkBytes, 0, bytesRead);
          _inputQueue.Write(new Chunk { Bytes = chunkBytes, Index = index }, token);
          Log.Debug($"Reading chunk #{index}");
          index++;
        }

        _inputQueue.Close();
      }
      catch (Exception e)
      {
        Log.Error("Reading failed: " + e.Message);
        _inputQueue.Close();
        throw;
      }
    }
  }
}
