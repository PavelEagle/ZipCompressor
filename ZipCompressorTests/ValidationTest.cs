using System;
using System.IO;
using FluentAssertions;
using Xunit;
using ZipCompressor.Common;

namespace ZipCompressorTests
{
  public class ValidationTest : IDisposable
  {
    private const string FileName = "testFile";

    public ValidationTest()
    {
      using var stream = File.Create(FileName);
    }

    [Theory]
    [InlineData("compress", "testFile", "testFile-result")]
    [InlineData("decompress", "testFile", "testFile-result")]
    public void ShouldBeSuccess_WhenMethodCompressOrDecompress(params string[] args)
    {
      // When
      Action validation = () => Options.Validation(args);

      // Then
      validation.Should().NotThrow();
    }

    [Theory]
    [InlineData("test", "testFile", "testFile-result")]
    [InlineData("some-test", "testFile", "testFile-result")]
    public void ShouldBeException_WhenMethodNoCompressOrDecompress(params string[] args)
    {
      // When
      Action validation = () => Options.Validation(args);

      // Then
      validation.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("one")]
    [InlineData("one", "two")]
    [InlineData("one", "two", "three", "four")]

    public void ShouldBeException_WhenNoThreeArguments(params string[] args)
    {
      // When
      Action validation = () => Options.Validation(args);

      // Then
      validation.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("compress", "testFile:dsd%", "testFile-result")]
    [InlineData("decompress", "testFile", "")]
    public void ShouldBeException_WhenIncorrectPath(params string[] args)
    {
      // When
      Action validation = () => Options.Validation(args);

      // Then
      validation.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ShouldBeException_WhenFileIsNotExists()
    {
      //Given 
      var args = new[] {"compress", "someTest", "testFile-result"};

      // When
      Action validation = () => Options.Validation(args);

      // Then
      validation.Should().Throw<ArgumentException>();
    }

    public void Dispose()
    {
      File.Delete(FileName);
    }
  }
}