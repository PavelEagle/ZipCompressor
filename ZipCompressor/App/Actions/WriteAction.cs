using System.IO;
using ZipCompressor.App.BaseCompressor;

namespace ZipCompressor.App.Actions
{
  public class WriteAction: IAction
  {
    private readonly IChunkCollection _inputChunkCollection;
    private static volatile int _nextWriteChunk;
    private readonly string _outputFilePath;

    public WriteAction(IChunkCollection inputChunkCollection, string outputFilePath)
    {
      _inputChunkCollection = inputChunkCollection;
      _outputFilePath = outputFilePath;
    }

    public void Execute(int chunkIndex)
    {

      if (chunkIndex == 0)
        _nextWriteChunk = 0; // TODO Reset on a new file processing.

      while (chunkIndex != _nextWriteChunk) { } // TODO The file must be written sequentially, because the size of the following blocks is unknown.

      using (var writer = new BinaryWriter(File.Open(_outputFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None)))
      {
        writer.BaseStream.Seek(0, SeekOrigin.End);
        var bytes = _inputChunkCollection.Get(chunkIndex);
        writer.Write(bytes);
      }

      _nextWriteChunk++;
    }
  }
}

