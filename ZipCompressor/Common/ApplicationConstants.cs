namespace ZipCompressor.Common
{
  internal static class ApplicationConstants
  {
    public const int DefaultByteBufferSize = 1024 * 1024;
    public static readonly byte[] MagicHeader = { 0x1a, 0x2b, 0x3c, 0x4d };

    #region Programm result
    public const int Success = 0;
    public const int Error = 1;
    #endregion

    #region MyRegion
    public const int OperationArgNumber = 0;
    public const int InputFilePathArgNumber = 1;
    public const int OutputFilePathArgNumber = 2;
    #endregion
  }
}
