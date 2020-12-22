using System.IO;

namespace ZipCompressor.App.BaseCompressor
{
  public class Chunk
  {
    public int Index { get; set; }
    public long StartPosition { get; set; }
    public int Length { get; set; }

    public static byte[] CreateChunk(string fileName, long startPosition, int bytesCount, int chunkIndex)
    {
      using (var reader = new BinaryReader(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)))
      {
        reader.BaseStream.Seek(startPosition, SeekOrigin.Begin);
        var bytes = reader.ReadBytes(bytesCount);
        return bytes;
      }
    }
  }
}
