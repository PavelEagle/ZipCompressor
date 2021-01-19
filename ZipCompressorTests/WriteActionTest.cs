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
    public void ShouldBeEmpty_WhenZeroChunksWasWritten()
    {
      // Given
      var maxElementsInChunk = 0;

      // When
      var chunkWriter = new ChunksWriter(_outputChunkQueue);
      using var outputStream = new MemoryStream();
      chunkWriter.Write(outputStream, _token, maxElementsInChunk);

      //Then
      var result = outputStream.ToArray();

      result.Length.Should().Be(0);
    }

    [Theory]
    [InlineData("Hello!")]
    [InlineData("This is test data")]
    [InlineData("Test read this chunks")]
    public void ShouldBeEqualBytes_WhenChunkWasWritten(string inputString)
    {
      // Given  
      var maxElementsInChunk = 1;
      var bytes = Encoding.ASCII.GetBytes(inputString);
      _outputChunkQueue.WriteChunk(new Chunk() { Bytes = bytes }, _token);

      // When
      var chunkWriter = new ChunksWriter(_outputChunkQueue);
      using var outputStream = new MemoryStream();
      chunkWriter.Write(outputStream, _token, maxElementsInChunk);

      // Then
      var result = outputStream.ToArray();

      result.Should().BeEquivalentTo(bytes);
    }


    [Fact]
    public void ShouldBeEqualBytes_WhenChunksWasWritten()
    {
      // Given  
      var maxElementsInChunk = 3;
      var chunkData = new[]
      {
        new Chunk { Bytes = Encoding.ASCII.GetBytes("Hello! "), Index = 0 },
        new Chunk { Bytes = Encoding.ASCII.GetBytes("This is test data. "), Index = 1 },
        new Chunk { Bytes = Encoding.ASCII.GetBytes("Test read this chunks"), Index = 2 }
      };

      foreach (var chunk in chunkData)
        _outputChunkQueue.WriteChunk(chunk, _token);

      // When
      var chunkWriter = new ChunksWriter(_outputChunkQueue);
      using var outputStream = new MemoryStream();
      chunkWriter.Write(outputStream, _token, maxElementsInChunk);

      // Then
      var result = outputStream.ToArray();
      var inputData = chunkData.Select(x => x.Bytes).Aggregate((a, b) => a.Concat(b).ToArray());

      result.Should().BeEquivalentTo(inputData);
    }
  }
}
