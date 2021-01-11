using System;
using System.IO;
using System.Text;
using FluentAssertions;
using Xunit;
using ZipCompressor.App;
using ZipCompressor.Common;

namespace ZipCompressorTests
{
  public class ApplicationTest : IDisposable
  {
    private const string OriginFilename = "test-orig.txt";
    private const string CompressedFileName = "test.gz";
    private const string ResultFileName = "test-result.txt";

    [Fact]
    public void ShouldBeEqualOriginAndResultedFiles_WhenCompressAndDecompress()
    {
      // Given   
      var inputText = "Some test data";
      var bytes = Encoding.ASCII.GetBytes(inputText);
      File.WriteAllBytes(OriginFilename, bytes);
 
      var compressArgs = new[] { "compress", OriginFilename, CompressedFileName };
      var decompressArgs = new[] { "decompress", CompressedFileName, ResultFileName };

      // When
      var compressorApp = new ZipApplication(Options.Validation(compressArgs));
      compressorApp.Start();
      
      var deCompressorApp = new ZipApplication(Options.Validation(decompressArgs));
      deCompressorApp.Start();

      // Then
      var result = File.ReadAllText(ResultFileName);
      result.Should().BeEquivalentTo(inputText);
    }

    public void Dispose()
    {
      File.Delete(OriginFilename);
      File.Delete(CompressedFileName);
      File.Delete(ResultFileName);
    }
  }
}
