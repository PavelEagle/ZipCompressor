using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Serilog;
using ZipCompressor.App.Archiver;
using ZipCompressor.Common;

namespace ZipCompressor.App
{
  public class ZipApplication
  {
    private readonly Stopwatch _timer = new Stopwatch();
    private readonly Options _options;
    private readonly IArchiver _archiver;
    private Executor _executor;

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
      _timer.Start();
      var cancellationTokenSource = new CancellationTokenSource();
      using var inputStream = File.OpenRead(_options.InputFilePath);
      using var outputStream = File.Create(_options.OutputFilePath);

      var expectedChunksCount = GetExpectedChunksCount(_options, inputStream, outputStream);

      void ReadActionTask() => _archiver.Read(inputStream, cancellationTokenSource.Token);
      void WriteActionTask() => _archiver.Write(outputStream, cancellationTokenSource.Token,
        expectedChunksCount, _options.Mode == ArchiveActions.Compress);
      void ZipActionTask() => _archiver.StartZipAction(cancellationTokenSource.Token);

      var zipActionsTasks = Enumerable.Repeat<Action>(ZipActionTask, Environment.ProcessorCount);

      var actions = new Action[]
      {
        ReadActionTask,
        WriteActionTask,
      }.Concat(zipActionsTasks).ToArray();

      _executor = new Executor(actions, cancellationTokenSource);

      Log.Information(new StringBuilder(50).Insert(0, "=", 50).ToString());//Print line

      Log.Information("Application started...");
      _executor.Start();
      _executor.Wait();

      _timer.Stop();
      Log.Information($"Application finished in {_timer.Elapsed}");
    }

    public void Stop()
    {
      Log.Information($"Aborted....");
      _executor.Abort();
      IsAborted = true;
    }
    
    private static int GetExpectedChunksCount(Options options, Stream inputStream, Stream outputStream)
    {
      var headers = ApplicationConstants.Headers;

      int expectedChunksCount;
      if (options.Mode == ArchiveActions.Compress)
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
