using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using FluentAssertions;
using Xunit;
using ZipCompressor.App;
using ZipCompressor.App.Actions.Write;

namespace ZipCompressorTests
{
  public class WriteActionTest
  {
    private readonly CancellationToken _token;
    private readonly ChunkQueue _outputChunkQueue;

    public WriteActionTest()
    {
      _token = new CancellationToken();
      _outputChunkQueue = new ChunkQueue(Environment.ProcessorCount);
    }

    [Fact]
    public void CompressEmptyChunkReaderTest()
    {
      var maxElementsInChunk = 0;

      var compressor = new ChunksWriter(_outputChunkQueue);
      using var outputStream = new MemoryStream();

      compressor.Write(outputStream, _token, maxElementsInChunk);

      var result = outputStream.ToArray();

      result.Length.Should().Be(0);
    }


    [Fact]
    public void CompressChunkReaderTest()
    {
      var maxElementsInChunk = 3;
      var chunkData = new[]
      {
        new Chunk { Bytes = Encoding.ASCII.GetBytes("Hello! "), Index = 0 },
        new Chunk { Bytes = Encoding.ASCII.GetBytes("This is test data. "), Index = 1 },
        new Chunk { Bytes = Encoding.ASCII.GetBytes("Test read this chunks"), Index = 2 }
      };

      foreach (var chunk in chunkData)
      {
        _outputChunkQueue.Write(chunk, _token);
      }

      var compressor = new ChunksWriter(_outputChunkQueue);
      using var outputStream = new MemoryStream();

      compressor.Write(outputStream, _token, maxElementsInChunk);

      var result = outputStream.ToArray();

      var inputData = chunkData.Select(x => x.Bytes).Aggregate((a, b) => a.Concat(b).ToArray());

      result.Should().BeEquivalentTo(inputData);
    }
  }
}
