using System.IO;

namespace ZipCompressor.Services
{
  public class FileCreator
  {
    public static void CreateDummyFile(string filePath, long length)
    {
      using (StreamWriter file = new StreamWriter(File.Create(filePath)))
      {
        for (var i = 0; i < length; i++)
        {
          file.WriteLine("TestData");
        }
      }
    }
  }
}
