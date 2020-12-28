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
      Func<byte[], byte[]> zipFunc;

      switch (options.Mode)
      {
        case Commands.Compress:
          zipFunc = _compressor.Compress;
          break;
        case Commands.Decompress:
          zipFunc = _compressor.Decompress;
          break;
        default:
          throw new ArgumentException("Unknown archiver operation type.");
      }


      var taskQueue = new TaskQueue(Environment.ProcessorCount);
      var fileLength = new FileInfo(options.InputFilePath).Length;
      var availableBytes = fileLength;
      
      var chunkIndex = 0;

      while (availableBytes > 0)
      {
        var readCount = availableBytes < Constants.DefaultByteBufferSize
          ? (int)availableBytes
          : Constants.DefaultByteBufferSize;

        var actions = new List<IAction>
        {
          new ReadAction(fileLength - availableBytes, readCount, _inputChunkCollection, options.InputFilePath),
          new CompressAction(_inputChunkCollection, _outputChunkCollection, zipFunc),
          new WriteAction(_inputChunkCollection, options.OutputFilePath)
        };

        taskQueue.AddTask(new ActionPipeline(actions, chunkIndex));

        availableBytes -= readCount;
        chunkIndex++;
      }

      return taskQueue;
    }
  }
}
