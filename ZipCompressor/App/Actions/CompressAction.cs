using System;
using ZipCompressor.App.BaseCompressor;

namespace ZipCompressor.App.Actions
{
  public class CompressAction: IAction
  {
    private readonly int _chunkIndex;
    private readonly string _inputFilePath;
    private readonly Func<byte[], byte[]> _compressionFunction;
    private readonly IChunkCollection _inputChunkCollection;
    private readonly IChunkCollection _outputChunkCollection;

    public CompressAction(IChunkCollection inputChunkCollection, IChunkCollection outputChunkCollection, int chunkIndex, string inputFilePath, Func<byte[], byte[]> compressionFunction)
    {
      _inputChunkCollection = inputChunkCollection;
      _outputChunkCollection = outputChunkCollection;
      _chunkIndex = chunkIndex;
      _inputFilePath = inputFilePath;
      _compressionFunction = compressionFunction;
    }

    public void Execute()
    {
      _outputChunkCollection.Add(_chunkIndex, _compressionFunction(_inputChunkCollection.Get(_chunkIndex)));
    }
  }
}
