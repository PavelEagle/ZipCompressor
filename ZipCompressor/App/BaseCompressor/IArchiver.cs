namespace ZipCompressor.App.BaseCompressor
{
  public interface IArchiver
  {
    byte[] Compress(byte[] bytes);
    byte[] Decompress(byte[] bytes);
  }
}
