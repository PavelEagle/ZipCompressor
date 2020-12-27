using System.IO;
using ZipCompressor.App.BaseCompressor;

namespace ZipCompressor.App.Actions
{
  public class ReadAction : IAction
  {
    private readonly string _inputFileName;
    private readonly long _startPosition;
    private readonly int _bytesCount;
    private readonly IChunkCollection _inputHolder;

    public ReadAction(long startPosition, int bytesCount, IChunkCollection inputHolder, string inputFileName)
    {
      _startPosition = startPosition;
      _bytesCount = bytesCount;
      _inputHolder = inputHolder;
      _inputFileName = inputFileName;
    }

    public void Execute(int chunkIndex)
    {
      using (var reader = new BinaryReader(File.Open(_inputFileName, FileMode.Open, FileAccess.Read, FileShare.Read)))
      {
        reader.BaseStream.Seek(_startPosition, SeekOrigin.Begin);
        var bytes = reader.ReadBytes(_bytesCount);
        _inputHolder.Add(chunkIndex, bytes);
      }
    }
  }
}
