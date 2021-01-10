using System;
using System.IO;
using System.Text;
using System.Threading;
using FluentAssertions;
using Xunit;
using ZipCompressor.App;
using ZipCompressor.App.Actions.Read;
using ZipCompressor.Common;

namespace ZipCompressorTests
{
  public class ReadActionTest : IDisposable
  {
    private readonly CancellationToken _token;
    private readonly ChunkQueue _inputChunkQueue;

    public ReadActionTest()
    {
      _token = new CancellationToken();
      _inputChunkQueue = new ChunkQueue(Environment.ProcessorCount);
    }

    [Theory]
    [InlineData("Hello!")]
    [InlineData("This is test data")]
    [InlineData("Test read this chunks")]
    public void CompressChunkReaderTest(string inputString)
    {
      var chunkReader = new CompressChunkReader(_inputChunkQueue, ApplicationConstants.DefaultByteBufferSize);
      var bytes = Encoding.ASCII.GetBytes(inputString);
      using var inputStream = new MemoryStream(bytes);

      chunkReader.Read(inputStream, _token);
      var result = _inputChunkQueue.Read(_token);

      result.Should().BeEquivalentTo(new { Bytes = bytes, Index = 0 });
    }

    //[Theory]
    //[InlineData("Hello!")]
    //[InlineData("This is test data")]
    //[InlineData("Test read this chunks")]
    //public void DeCompressChunkReaderTest(string inputString)
    //{
    //  var compressor = new DecompressChunkReader(_inputChunkQueue, ApplicationConstants.DefaultByteBufferSize);
    //  var bytes = Encoding.ASCII.GetBytes(inputString);

    //  using var inputStream = new MemoryStream(bytes);

    //  compressor.Read(inputStream, _token);
    //  var result = _inputChunkQueue.Read(_token);

    //  result.Should().BeEquivalentTo(new { Bytes = bytes, Index = 0 });
    //}

    public void Dispose()
    {
    }
  }
}
