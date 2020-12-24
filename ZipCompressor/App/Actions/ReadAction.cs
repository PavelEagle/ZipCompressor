using System.IO;
using ZipCompressor.App.BaseCompressor;

namespace ZipCompressor.App.Actions
{
  public class ReadAction : IAction
  {
    private readonly string _inputFileName;
    private readonly long _startPosition;
    private readonly int _bytesCount;
    private readonly int _chunkIndex;
    private readonly IChunkCollection _outputHolder;

    public ReadAction(long startPosition, int bytesCount, int chunkIndex, IChunkCollection outputHolder, string inputFileName)
    {
      _startPosition = startPosition;
      _bytesCount = bytesCount;
      _chunkIndex = chunkIndex;
      _outputHolder = outputHolder;
      _inputFileName = inputFileName;
    }

    public void Execute()
    {
      using (var reader = new BinaryReader(File.Open(_inputFileName, FileMode.Open, FileAccess.Read, FileShare.Read)))
      {
        reader.BaseStream.Seek(_startPosition, SeekOrigin.Begin);
        var bytes = reader.ReadBytes(_bytesCount);
        _outputHolder.Add(_chunkIndex, bytes);
      }
    }
  }
}
