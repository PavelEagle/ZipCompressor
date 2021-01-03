using System;
using System.IO;
using System.Threading;
using Serilog;

namespace ZipCompressor.App.Actions
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
      _inputQueue.Open();
      var lengthBuffer = new byte[sizeof(int)];
      var buffer = new byte[0];
      var maxChunkLength = 0;
      var index = 0;

      try
      {
        int bytesRead;
        while (!token.IsCancellationRequested &&
               (bytesRead = inputStream.Read(lengthBuffer, 0, lengthBuffer.Length)) > 0)
        {
          if (bytesRead < lengthBuffer.Length)
          {
            throw new Exception(); //TODO exception
          }

          var chunkLength = BitConverter.ToInt32(lengthBuffer, 0);
          if (chunkLength < 0 || chunkLength > _chunkSize * 10)
          {
            throw new Exception(); //TODO exception
          }

          if (chunkLength > maxChunkLength)
          {
            Log.Information($"Increasing reading buffer to {chunkLength}");
            maxChunkLength = chunkLength;
            buffer = new byte[maxChunkLength];
          }

          bytesRead = inputStream.Read(buffer, 0, chunkLength);
          if (bytesRead != chunkLength)
          {
            throw new Exception(); //TODO exception
          }

          var chunkBytes = new byte[chunkLength];
          Buffer.BlockCopy(buffer, 0, chunkBytes, 0, bytesRead);
          _inputQueue.Write(new Chunk { Bytes = chunkBytes, Index = index }, token);
          Log.Information($"Read compressed chunk #{index} of {chunkLength} bytes");
          index++;
        }

        _inputQueue.Close();
      }
      catch (Exception e)
      {
        Log.Error("Archive reading failed with error: " + e.Message);
        _inputQueue.Close();
        throw;
      }
    }
  }
}
