using System;
using ZipCompressor.App.BaseCompressor;

namespace ZipCompressor.App.Actions
{
  public class CompressAction: IAction
  {
    private readonly Func<byte[], byte[]> _compressionFunction;
    private readonly IChunkCollection _inputChunkCollection;
    private readonly IChunkCollection _outputChunkCollection;

    public CompressAction(IChunkCollection inputChunkCollection, IChunkCollection outputChunkCollection, Func<byte[], byte[]> compressionFunction)
    {
      _inputChunkCollection = inputChunkCollection;
      _outputChunkCollection = outputChunkCollection;
      _compressionFunction = compressionFunction;
    }

    public void Execute(int chunkIndex)
    {
      _outputChunkCollection.Add(chunkIndex, _compressionFunction(_inputChunkCollection.Get(chunkIndex)));
    }
  }
}
