using System;
using ZipCompressor.App.BaseCompressor;

namespace ZipCompressor.App.Actions
{
  public class DecompressAction: IAction
  {
    private readonly int _chunkIndex;
    private readonly Func<byte[], byte[]> _decompressionFunction;
    private readonly IChunkCollection _inputChunkCollection;
    private readonly IChunkCollection _outputChunkCollection;

    public DecompressAction(IChunkCollection outputChunkCollection, IChunkCollection inputChunkCollection, int chunkIndex, Func<byte[], byte[]> decompressionFunction)
    {
      _outputChunkCollection = outputChunkCollection;
      _inputChunkCollection = inputChunkCollection;
      _chunkIndex = chunkIndex;
      _decompressionFunction = decompressionFunction;
    }

    public void Execute()
    {
      _outputChunkCollection.Add(_chunkIndex, _decompressionFunction(_inputChunkCollection.Get(_chunkIndex)));
    }
  }
}
