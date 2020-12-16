using System;
using System.IO;
using System.IO.Compression;

namespace ZipCompressor.BaseCompressor
{
  class GZipCompressor: ICompressor
  {
    private readonly string _directoryPath;

    public GZipCompressor(string directoryPath)
    {
      _directoryPath = directoryPath;
    }

    public void StartCompress()
    {
      DirectoryInfo directorySelected = new DirectoryInfo(_directoryPath);
      Compress(directorySelected);
    }

    public void StartDecompress()
    {
      DirectoryInfo directorySelected = new DirectoryInfo(_directoryPath);

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
            FileInfo info = new FileInfo(_directoryPath + Path.DirectorySeparatorChar + fileToCompress.Name + ".gz");
            Console.WriteLine($"Compressed {fileToCompress.Name} from {fileToCompress.Length} to {info.Length} bytes.");
          }
        }
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
