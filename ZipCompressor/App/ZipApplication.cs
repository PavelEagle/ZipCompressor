using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ZipCompressor.App.Actions;
using ZipCompressor.App.BaseCompressor;
using ZipCompressor.Common;

namespace ZipCompressor.App
{
  public class ZipApplication: IDisposable
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
          return CreateCompressionTaskQueue(options.InputFileName, options.OutputFileName, Constants.DefaultByteBufferSize * 1024);
        case Commands.Decompress:
          return CreateDecompressionTaskQueue(options.InputFileName, options.OutputFileName, Constants.DefaultByteBufferSize * 1024);
        default:
          throw new ArgumentException("Unknown archiver operation type.");
      }
    }

    private TaskQueue CreateCompressionTaskQueue(string inputFilePath, string outputFilePath, int bufferSize)
    {
      var taskQueue = new TaskQueue(Environment.ProcessorCount);
      var fileLength = new FileInfo(inputFilePath).Length;
      var availableBytes = fileLength;
      var chunkIndex = 0;
      while (availableBytes > 0)
      {
        var actions = new List<IAction>();
        var readCount = availableBytes < bufferSize
          ? (int)availableBytes
          : bufferSize;

        actions.Add(new ReadAction(fileLength - availableBytes, readCount, chunkIndex, _outputChunkCollection, inputFilePath));
        actions.Add(new CompressAction(_inputChunkCollection, _outputChunkCollection, chunkIndex, inputFilePath, _compressor.Compress));
        actions.Add(new WriteAction(_inputChunkCollection, chunkIndex, outputFilePath));

        taskQueue.AddTask(new ActionPipeline(actions));

        availableBytes -= readCount;
        chunkIndex++;
      }

      return taskQueue;
    }

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
          var actions = new List<IAction>();

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
          actions.Add(new ReadAction(gzipBlockStartPosition, gzipBlockLength,chunkIndex, _outputChunkCollection, inputFilePath));
          actions.Add(new DecompressAction(_outputChunkCollection,_inputChunkCollection, chunkIndex, _compressor.Decompress));
          actions.Add(new WriteAction(_inputChunkCollection, chunkIndex, outputFilePath));

          taskQueue.AddTask(new ActionPipeline(actions));

          chunkIndex++;
        }
      }

      return taskQueue;
    }
  }
}
