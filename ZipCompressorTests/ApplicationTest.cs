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
    [Fact]
    public void FullAppTest()
    {
      var originFilename = "test-orig.txt";
      var resultFileName = "test-result.txt";
      var inputText = "Some test data";
      var bytes = Encoding.ASCII.GetBytes(inputText);

      File.WriteAllBytes(originFilename, bytes);
 
      var compressArgs = new[] { "compress", "test-orig.txt", "test.gz" };
      var decompressArgs = new[] { "decompress", "test.gz", "test-result.txt" };

      var compressorApp = new ZipApplication(Options.Validation(compressArgs));
      compressorApp.Start();
      
      var deCompressorApp = new ZipApplication(Options.Validation(decompressArgs));
      deCompressorApp.Start();

      var result = File.ReadAllText(resultFileName);

      result.Should().BeEquivalentTo(inputText);
    }

    public void Dispose()
    {
      File.Delete("test-orig.txt");
      File.Delete("test-result.txt");
      File.Delete("test.gz");
    }
  }
}
