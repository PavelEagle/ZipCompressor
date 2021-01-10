using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Serilog;
using ZipCompressor.App.Archiver;
using ZipCompressor.Common;

namespace ZipCompressor.App
{
  public class ZipApplication : IDisposable
  {
    private readonly Options _options;
    private readonly IArchiver _archiver;
    private Executor _executor;
    private FileStream _inputStream;
    private FileStream _outputStream;
    public ZipApplication(Options options)
    {
      _options = options;

      var inputQueue = new ChunkQueue(Environment.ProcessorCount);
      var outputQueue = new ChunkQueue(Environment.ProcessorCount);

      _archiver = GipArchiver.Create(inputQueue, outputQueue, options.Mode);
    }

    public bool IsAborted { get; private set; }

    public void Start()
    {
      var cancellationTokenSource = new CancellationTokenSource();
      using var read = _inputStream = File.OpenRead(_options.InputFilePath);
      using var stream = _outputStream = File.Create(_options.OutputFilePath);

      var expectedChunksCount = GetExpectedChunksCount(_options, _inputStream, _outputStream);

      void ReadActionTask() => _archiver.Read(_inputStream, cancellationTokenSource.Token);
      void WriteActionTask() => _archiver.Write(_outputStream, cancellationTokenSource.Token,
        expectedChunksCount, _options.Mode == ArchiveActions.Compress);
      void ZipActionTask() => _archiver.StartZipAction(cancellationTokenSource.Token);

      var zipActionsTasks = Enumerable.Repeat<Action>(ZipActionTask, Environment.ProcessorCount);

      var actions = new Action[]
      {
        ReadActionTask,
        WriteActionTask,
      }.Concat(zipActionsTasks).ToArray();

      _executor = new Executor(actions, cancellationTokenSource);

      Log.Information(new StringBuilder(50).Insert(0, "=", 50).ToString());//TODO
      Log.Information("Application started...");
      _executor.Start();
      _executor.Wait();
    }

    public void Stop()
    {
      _executor.Abort();
      IsAborted = true;
    }

    public void Dispose()
    {
      _inputStream?.Dispose();
      _outputStream?.Dispose();
    }

    private static int GetExpectedChunksCount(Options options, Stream inputStream, Stream outputStream)
    {
      var headers = ApplicationConstants.Headers;

      int expectedChunksCount;
      if (options.Mode == ArchiveActions.Compress)
      {
        expectedChunksCount = (int)Math.Ceiling(inputStream.Length * 1.0 / ApplicationConstants.DefaultByteBufferSize);
        outputStream.Write(headers, 0, headers.Length);
        outputStream.Write(BitConverter.GetBytes(expectedChunksCount), 0, sizeof(int));
      }
      else
      {
        byte[] buffer = new byte[sizeof(int)];
        inputStream.Read(buffer, 0, buffer.Length);
        if (!buffer.SequenceEqual(headers))
        {
          throw new Exception($"File {options.InputFilePath} is not an archive");
        }

        inputStream.Read(buffer, 0, sizeof(int));
        expectedChunksCount = BitConverter.ToInt32(buffer, 0);
      }

      return expectedChunksCount;
    }
  }
}
