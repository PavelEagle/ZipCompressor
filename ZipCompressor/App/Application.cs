using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ZipCompressor.App.Actions;
using ZipCompressor.Common;

namespace ZipCompressor.App
{
  public class Application
  {
    private readonly CommandOptions _options;
    public Application(CommandOptions options)
    {
      _options = options;
    }

    public Executor Start()
    {
      var cancellationTokenSource = new CancellationTokenSource();
      var inputQueue = new ChunkQueue(Environment.ProcessorCount);
      var outputQueue = new ChunkQueue(Environment.ProcessorCount);
      var inputStream = File.OpenRead(_options.InputFilePath);
      var outputStream = File.Create(_options.OutputFilePath);

      var expectedChunksCount = GetExpectedChunksCount(_options, inputStream, outputStream);

      var writer = new ChunksWriter(outputQueue);
      IReadAction reader = null;
      IEnumerable<IArchiverAction> archiverActions = null;

      switch (_options.Mode)
      {
        case ArchiveModes.Compress:
          reader = new CompressChunkReader(inputQueue, ApplicationConstants.DefaultByteBufferSize);
          archiverActions = Enumerable.Range(0, Environment.ProcessorCount*2).Select( //TODO ProcessorCount*2
            _ => new ChunkCompressor(inputQueue, outputQueue));
          break;

        case ArchiveModes.Decompress:
          reader = new DecompressChunkReader(inputQueue, ApplicationConstants.DefaultByteBufferSize);
          archiverActions = Enumerable.Range(0, Environment.ProcessorCount * 2).Select(
            _ => new ChunkDecompressor(inputQueue, outputQueue));
          break;
      }

      void ReadAction()
      {
        (reader ?? throw new ArgumentNullException()).Read(inputStream, cancellationTokenSource.Token);
        inputStream.Close();
      }

      void WriteAction()
      {
        writer.WriteToStream(outputStream, cancellationTokenSource.Token, expectedChunksCount, _options.Mode == ArchiveModes.Compress);
        outputStream.Close();
      }

      var actions = new Action[]
      {
        ReadAction,
        WriteAction,
      }.Concat((archiverActions ?? throw new ArgumentNullException())
        .Select<IArchiverAction, Action>(processor => () => processor.Start(cancellationTokenSource.Token))).ToArray();

      return Executor.StartInParallel(actions, cancellationTokenSource);
    }

    private static int GetExpectedChunksCount(CommandOptions options, Stream inputFile, Stream outputFile)
    {
      var headers = ApplicationConstants.MagicHeader;

      int expectedChunksCount;
      if (options.Mode == ArchiveModes.Compress)
      {
        expectedChunksCount = (int)Math.Ceiling(inputFile.Length * 1.0 / ApplicationConstants.DefaultByteBufferSize);
        outputFile.Write(headers, 0, headers.Length);
        outputFile.Write(BitConverter.GetBytes(expectedChunksCount), 0, sizeof(int));
      }
      else
      {
        byte[] buffer = new byte[sizeof(int)];
        inputFile.Read(buffer, 0, buffer.Length);
        if (!buffer.SequenceEqual(headers))
        {
          throw new Exception($"File {options.InputFilePath} is not an archive");
        }

        inputFile.Read(buffer, 0, sizeof(int));
        expectedChunksCount = BitConverter.ToInt32(buffer, 0);
      }

      return expectedChunksCount;
    }
  }
}
