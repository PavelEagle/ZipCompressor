﻿namespace ZipCompressor.Common
{
  internal class Constants
  {
    public const int MemoryPageSize = 4096; // Default system value.
    public const int DefaultByteBufferSize = 81920; // The largest multiple of 4096 that is still smaller than the large object heap threshold (85K).
    public const int MinimalAvailableMemoryInPercentages = 25; // If available physical memory is less than 25 % of total RAM, then force a full garbage collection.
    public static readonly byte[] GZipDefaultHeader = { 0x1f, 0x8b, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00 };

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
