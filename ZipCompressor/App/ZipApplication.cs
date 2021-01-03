using System;
using System.IO;
using System.Linq;
using System.Threading;
using Serilog;
using ZipCompressor.App.Archiver;
using ZipCompressor.Common;

namespace ZipCompressor.App
{
  public class ZipApplication : IDisposable
  {
    private readonly CommandOptions _options;
    private readonly IArchiver _archiver;
    private Executor _executor;
    private FileStream _inputStream;
    private FileStream _outputStream;
    public ZipApplication(CommandOptions options)
    {
      _options = options;

      var inputQueue = new ChunkQueue(Environment.ProcessorCount);
      var outputQueue = new ChunkQueue(Environment.ProcessorCount);

      _archiver = GipArchiver.Create(inputQueue, outputQueue, options);
    }

    public bool IsErrorOccured { get; private set; }

    public void Start()
    {
      var cancellationTokenSource = new CancellationTokenSource();
      _inputStream = File.OpenRead(_options.InputFilePath);
      _outputStream = File.Create(_options.OutputFilePath);
      var expectedChunksCount = GetExpectedChunksCount(_options, _inputStream, _outputStream);

      void ReadActionTask() => _archiver.Read(_inputStream, cancellationTokenSource.Token);
      void WriteActionTask() => _archiver.Write(_outputStream, cancellationTokenSource.Token,
        expectedChunksCount, _options.Mode == ArchiveModes.Compress);
      void ZipActionTask() => _archiver.StartZipAction(cancellationTokenSource.Token);

      var zipActionsTasks = Enumerable.Repeat<Action>(ZipActionTask, Environment.ProcessorCount);

      var actions = new Action[]
      {
        ReadActionTask,
        WriteActionTask,
      }.Concat(zipActionsTasks).ToArray();

      _executor = new Executor(actions, cancellationTokenSource);

      _executor.Start();
      Log.Information("Start...");
      _executor.Wait();
    }

    public void Stop()
    {
      _executor.Abort();
      IsErrorOccured = true;
    }

    public void Dispose()
    {
      _inputStream?.Dispose();
      _outputStream?.Dispose();
    }

    private static int GetExpectedChunksCount(CommandOptions options, Stream inputStream, Stream outputStream)
    {
      var headers = ApplicationConstants.Header;

      int expectedChunksCount;
      if (options.Mode == ArchiveModes.Compress)
      {
        expectedChunksCount = (int)Math.Ceiling(inputStream.Length * 1.0 / ApplicationConstants.DefaultByteBufferSize);
        outputStream.Write(headers, 0, headers.Length);
        outputStream.Write(BitConverter.GetBytes(expectedChunksCount), 0, 4);
      }
      else
      {
        byte[] buffer = new byte[4];
        inputStream.Read(buffer, 0, buffer.Length);
        if (!buffer.SequenceEqual(headers))
        {
          throw new Exception($"File {options.InputFilePath} is not an archive");
        }

        inputStream.Read(buffer, 0, 4);
        expectedChunksCount = BitConverter.ToInt32(buffer, 0);
      }

      return expectedChunksCount;
    }
  }
}
