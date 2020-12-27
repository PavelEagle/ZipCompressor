namespace ZipCompressor.App.BaseCompressor
{
  public interface IChunkCollection
  {
    void Add(int index, byte[] bytes);
    void Remove(int index);
    byte[] Get(int index);
    int Count();
    void Clear();
  }
}
