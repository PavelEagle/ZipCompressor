using System.IO;
using ZipCompressor.App.BaseCompressor;

namespace ZipCompressor.App.Actions
{
  public class WriteAction: IAction
  {
    private readonly IChunkCollection _inputChunkCollection;
    private static volatile int _nextWriteChunk;
    private readonly int _chunkIndex;
    private readonly string _outputFilePath;

    public WriteAction(IChunkCollection inputChunkCollection, int chunkIndex, string outputFilePath)
    {
      _chunkIndex = chunkIndex;
      _inputChunkCollection = inputChunkCollection;
      _outputFilePath = outputFilePath;

      if (_chunkIndex == 0)
        _nextWriteChunk = 0; // Reset on a new file processing.
    }

    public void Execute()
    {
      while (_chunkIndex != _nextWriteChunk) { } // The file must be written sequentially, because the size of the following blocks is unknown.

      using (var writer = new BinaryWriter(File.Open(_outputFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None)))
      {
        writer.BaseStream.Seek(0, SeekOrigin.End);
        var bytes = _inputChunkCollection.Get(_chunkIndex);
        writer.Write(bytes);
      }

      _nextWriteChunk++;
    }
  }
}

