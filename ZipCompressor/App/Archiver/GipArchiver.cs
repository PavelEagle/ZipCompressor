using System;
using System.IO;
using System.Threading;
using ZipCompressor.App.Actions.Archive;
using ZipCompressor.App.Actions.Read;
using ZipCompressor.App.Actions.Write;
using ZipCompressor.Common;

namespace ZipCompressor.App.Archiver
{
  public sealed class GipArchiver: IArchiver
  {
    private readonly IReadAction _readAction;
    private readonly IArchiveAction _archiveAction;
    private readonly IWriteAction _writeAction;

    private GipArchiver(IReadAction readAction, IArchiveAction archiveAction, IWriteAction writeAction)
    {
      _readAction = readAction;
      _archiveAction = archiveAction;
      _writeAction = writeAction;
    }

    /// <summary>
    /// Create compressor or decompressor
    /// </summary>
    /// <param name="inputQueue"></param>
    /// <param name="outputQueue"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static GipArchiver Create(ChunkQueue inputQueue, ChunkQueue outputQueue, ArchiveActions action)
    {
      IWriteAction writer = new ChunksWriter(outputQueue);
      IReadAction reader;
      IArchiveAction archiverAction;

      switch (action)
      {
        case ArchiveActions.Compress:
          reader = new CompressChunkReader(inputQueue, ApplicationConstants.DefaultByteBufferSize);
          archiverAction = new ChunkCompressor(inputQueue, outputQueue);
          break;
        case ArchiveActions.Decompress:
          reader = new DecompressChunkReader(inputQueue);
          archiverAction = new ChunkDecompressor(inputQueue, outputQueue);
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }

      return new GipArchiver(reader, archiverAction, writer);
    }

    public void Read(Stream inputStream, CancellationToken token)
    {
      _readAction.Read(inputStream, token);
    }

    public void StartZipAction(CancellationToken token)
    {
      _archiveAction.StartZipAction(token);
    }

    public void Write(Stream outputStream, CancellationToken token, int expectedChunksCount, bool writeChunksLengths)
    {
      _writeAction.Write(outputStream, token, expectedChunksCount, writeChunksLengths);
    }
  }
}
