namespace ZipCompressor.Common
{
  internal class Constants
  {
    public const int DefaultByteBufferSize = 1000000; // The largest multiple of 4096 that is still smaller than the large object heap threshold (85K).

    #region Exit Codes
    public const int SuccessExitCode = 0;
    public const int ErrorExitCode = 1;
    #endregion

    #region Programm result
    public const int SuccessfulExecution = 0;
    public const int UnsuccessfulExecution = 1;
    #endregion

    #region MyRegion
    public const int OperationArgNumber = 0;
    public const int InputFilePathArgNumber = 1;
    public const int OutputFilePathArgNumber = 2;
    #endregion
  }
}
