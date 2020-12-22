namespace ZipCompressor.Common
{
  public class Constants
  {
    public const int MemoryPageSize = 4096; // Default system value.
    public const int DefaultByteBufferSize = 81920; // The largest multiple of 4096 that is still smaller than the large object heap threshold (85K).
  }
}
