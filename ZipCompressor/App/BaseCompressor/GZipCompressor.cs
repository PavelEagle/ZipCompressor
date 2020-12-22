using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace ZipCompressor.App.BaseCompressor
{
  class GZipCompressor: IArchiver
  {
    private readonly int _threadsCount;
    private readonly int _bufferSize;

    public GZipCompressor(int threadsCount, int bufferSize)
    {
      _threadsCount = threadsCount;
      _bufferSize = bufferSize;
    }

    public void StartCompress(string inputFilePath, string outputFilePath, int bufferSize)
    {
      var chunks = new List<byte[]>(0);
      var fileLength = new FileInfo(inputFilePath).Length;
      var availableBytes = fileLength;
      var chunkIndex = 0;
      while (availableBytes > 0)
      {
        var readCount = availableBytes < bufferSize
          ? (int)availableBytes
          : bufferSize;

        chunks.Add(Chunk.CreateChunk(inputFilePath, fileLength - availableBytes, readCount, chunkIndex));

        availableBytes -= readCount;
        chunkIndex++;
      }

      foreach (var chunk in chunks)
      {
        Compress(chunk);
      }

      //DirectoryInfo directorySelected = new DirectoryInfo(inputFilePath);
      //Compress(directorySelected);
    }

    public void StartDecompress(string inputFilePath, string outputFilePath, int bufferSize)
    {
      DirectoryInfo directorySelected = new DirectoryInfo(inputFilePath);

      foreach (FileInfo fileToDecompress in directorySelected.GetFiles("*.gz"))
      {
        Decompress(fileToDecompress);
      }
    }

    private void Compress(DirectoryInfo directorySelected)
    {
      foreach (FileInfo fileToCompress in directorySelected.GetFiles())
      {
        using (FileStream originalFileStream = fileToCompress.OpenRead())
        {
          if ((File.GetAttributes(fileToCompress.FullName) &
               FileAttributes.Hidden) != FileAttributes.Hidden & fileToCompress.Extension != ".gz")
          {
            using (FileStream compressedFileStream = File.Create(fileToCompress.FullName + ".gz"))
            {
              using (GZipStream compressionStream = new GZipStream(compressedFileStream,
                CompressionMode.Compress))
              {
                originalFileStream.CopyTo(compressionStream);
              }
            }
            FileInfo info = new FileInfo("" + Path.DirectorySeparatorChar + fileToCompress.Name + ".gz"); //change
            Console.WriteLine($"Compressed {fileToCompress.Name} from {fileToCompress.Length} to {info.Length} bytes.");
          }
        }
      }
    }

    private byte[] Compress(byte[] originalBytes)
    {
      using (var output = new MemoryStream())
      {
        using (var compressStream = new GZipStream(output, CompressionMode.Compress))
        {
          compressStream.Write(originalBytes, 0, originalBytes.Length);
        }

        return output.ToArray();
      }
    }

    private void Decompress(FileInfo fileToDecompress)
    {
      using FileStream originalFileStream = fileToDecompress.OpenRead();
      var currentFileName = fileToDecompress.FullName;
      var newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

      using FileStream decompressedFileStream = File.Create(newFileName);
      using GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress);
      decompressionStream.CopyTo(decompressedFileStream);
      Console.WriteLine($"Decompressed: {fileToDecompress.Name}");
    }
  }
}
