using System.IO;
using ZipCompressor.App.BaseCompressor;

namespace ZipCompressor.App.Actions
{
  public class WriteAction: IAction
  {
    private readonly IChunkCollection _inputChunkCollection;
    private static volatile int _nextWriteChunk;
    private readonly int _chunkIndex;
    private readonly string _outputFileName;

    public WriteAction(IChunkCollection inputChunkCollection, string inputFilePath, int chunkIndex, string outputFilePath)
    {
      _inputChunkCollection = inputChunkCollection;
      var fileLength = new FileInfo(inputFilePath).Length;
      var availableBytes = fileLength;
      var chunkIndex = 0;
      while (availableBytes > 0)
      {
        var readCount = availableBytes < bufferSize
          ? (int)availableBytes
          : bufferSize;

        availableBytes -= readCount;
        chunkIndex++;
      }

      if (_chunkIndex == 0)
        _nextWriteChunk = 0;
    }

    public void Execute()
    {
      while (_chunkIndex != _nextWriteChunk) { }

      using (var writer = new BinaryWriter(File.Open(_outputFileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None)))
      {
        writer.BaseStream.Seek(0, SeekOrigin.End);
        var bytes = _inputChunkCollection.Get(_chunkIndex);
        writer.Write(bytes);
      }

      _nextWriteChunk++;
    }
  }
}
