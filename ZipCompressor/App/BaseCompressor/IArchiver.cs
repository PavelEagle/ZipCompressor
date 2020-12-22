namespace ZipCompressor.App.BaseCompressor
{
  public interface IArchiver
  {
    void StartCompress(string inputFilePath, string outputFilePath, int bufferSize);
    void StartDecompress(string inputFilePath, string outputFilePath, int bufferSize);
  }
}
