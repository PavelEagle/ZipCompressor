using System;
using ZipCompressor.App.BaseCompressor;

namespace ZipCompressor.App.Actions
{
  public class DecompressAction: IAction
  {
    private readonly Func<byte[], byte[]> _decompressionFunction;
    private readonly IChunkCollection _inputChunkCollection;
    private readonly IChunkCollection _outputChunkCollection;

    public DecompressAction(IChunkCollection outputChunkCollection, IChunkCollection inputChunkCollection, Func<byte[], byte[]> decompressionFunction)
    {
      _outputChunkCollection = outputChunkCollection;
      _inputChunkCollection = inputChunkCollection;
      _decompressionFunction = decompressionFunction;
    }

    public void Execute(int chunkIndex)
    {
      _outputChunkCollection.Add(chunkIndex, _decompressionFunction(_inputChunkCollection.Get(chunkIndex)));
    }
  }
}
