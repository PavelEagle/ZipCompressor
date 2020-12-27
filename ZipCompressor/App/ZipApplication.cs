using System;
using System.Collections.Generic;
using System.IO;
using ZipCompressor.App.Actions;
using ZipCompressor.App.BaseCompressor;
using ZipCompressor.Common;

namespace ZipCompressor.App
{
  public class ZipApplication : IDisposable
  {
    private readonly IArchiver _compressor;
    private readonly IChunkCollection _inputChunkCollection;
    private readonly IChunkCollection _outputChunkCollection;
    private readonly TaskQueue _taskQueue;

    public ZipApplication(CommandOptions zipSettings, IArchiver compressor)
    {
      _inputChunkCollection = new ChunkCollection();
      _outputChunkCollection = new ChunkCollection();
      _compressor = compressor;

      _taskQueue = CreateTaskQueue(zipSettings);
    }

    public void StartProcess()
    {
      _taskQueue.Start();
    }

    public void StopProcess()
    {
      _taskQueue.Stop();
    }

    public void Dispose()
    {
      StopProcess();
      _inputChunkCollection.Clear();
      _outputChunkCollection.Clear();
    }

    private TaskQueue CreateTaskQueue(CommandOptions options)
    {
      switch (options.Mode)
      {
        case Commands.Compress:
          return CreateCompressionTaskQueue(options.InputFileName, options.OutputFileName, Constants.DefaultByteBufferSize * 1024); //todo
        case Commands.Decompress:
          return CreateDecompressionTaskQueue(options.InputFileName, options.OutputFileName, Constants.DefaultByteBufferSize * 1024);
        default:
          throw new ArgumentException("Unknown archiver operation type.");
      }
    }

    /// <summary>
    /// Create Task Queue for compress
    /// </summary>
    /// <param name="inputFilePath"></param>
    /// <param name="outputFilePath"></param>
    /// <param name="bufferSize"></param>
    /// <returns></returns>
    private TaskQueue CreateCompressionTaskQueue(string inputFilePath, string outputFilePath, int bufferSize)
    {
      var taskQueue = new TaskQueue(Environment.ProcessorCount);
      var fileLength = new FileInfo(inputFilePath).Length;
      var availableBytes = fileLength;
      var chunkIndex = 0;
      while (availableBytes > 0)
      {
        var readCount = availableBytes < bufferSize
          ? (int)availableBytes
          : bufferSize;
        
        var actions = new List<IAction>
        {
          new ReadAction(fileLength - availableBytes, readCount, _inputChunkCollection, inputFilePath),
          new CompressAction(_inputChunkCollection, _outputChunkCollection, _compressor.Compress),
          new WriteAction(_inputChunkCollection, outputFilePath)
        };

        taskQueue.AddTask(new ActionPipeline(actions, chunkIndex));

        availableBytes -= readCount;
        chunkIndex++;
      }

      return taskQueue;
    }

    /// <summary>
    /// Create Task Queue for decompress
    /// </summary>
    /// <param name="inputFilePath"></param>
    /// <param name="outputFilePath"></param>
    /// <param name="bufferSize"></param>
    /// <returns></returns>
    private TaskQueue CreateDecompressionTaskQueue(string inputFilePath, string outputFilePath, int bufferSize)
    {
      var taskQueue = new TaskQueue(Environment.ProcessorCount);

      using (var reader = new BinaryReader(File.Open(inputFilePath, FileMode.Open, FileAccess.Read)))
      {
        var gzipHeader = Constants.GZipDefaultHeader;

        var fileLength = new FileInfo(inputFilePath).Length;
        var availableBytes = fileLength;
        var chunkIndex = 0;
        while (availableBytes > 0)
        {
          var gzipBlock = new List<byte>(bufferSize);

          // GZip header.
          if (chunkIndex == 0) // Get first GZip header in the file. All internal gzip blocks have the same one.
          {
            gzipHeader = reader.ReadBytes(gzipHeader.Length);
            availableBytes -= gzipHeader.Length;
          }
          gzipBlock.AddRange(gzipHeader);

          // GZipped data.
          var gzipHeaderMatchsCount = 0;
          while (availableBytes > 0)
          {
            var curByte = reader.ReadByte();
            gzipBlock.Add(curByte);
            availableBytes--;

            // Check a header of the next gzip block.
            if (curByte == gzipHeader[gzipHeaderMatchsCount])
            {
              gzipHeaderMatchsCount++;
              if (gzipHeaderMatchsCount != gzipHeader.Length)
                continue;

              gzipBlock.RemoveRange(gzipBlock.Count - gzipHeader.Length, gzipHeader.Length); // Remove gzip header of the next block from a rear of this one.
              break;
            }

            gzipHeaderMatchsCount = 0;
          }

          var gzipBlockStartPosition = 0L;
          var gzipBlockLength = gzipBlock.ToArray().Length;
          if (chunkIndex > 0)
          {
            gzipBlockStartPosition = fileLength - availableBytes - gzipHeader.Length - gzipBlockLength;
            if (gzipBlockStartPosition + gzipHeader.Length + gzipBlockLength == fileLength) // The last gzip block in a file.
              gzipBlockStartPosition += gzipHeader.Length;
          }

          var actions = new List<IAction>
          {
            new ReadAction(gzipBlockStartPosition, gzipBlockLength, _inputChunkCollection, inputFilePath),
            new DecompressAction(_outputChunkCollection, _inputChunkCollection, _compressor.Decompress),
            new WriteAction(_inputChunkCollection, outputFilePath)
          };

          taskQueue.AddTask(new ActionPipeline(actions, chunkIndex));

          chunkIndex++;
        }
      }

      return taskQueue;
    }
  }
}
