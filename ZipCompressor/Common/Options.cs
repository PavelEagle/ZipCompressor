using System;
using System.IO;

namespace ZipCompressor.Common
{
  public struct Options
  {
    private Options(ArchiveActions mode, string inputFilePath, string outputFilePath)
    {
      Mode = mode;
      InputFilePath = inputFilePath;
      OutputFilePath = outputFilePath;
    }

    public ArchiveActions Mode { get; }
    public string InputFilePath { get; }
    public string OutputFilePath { get; }

    /// <summary>
    /// Parse input command line arguments.
    /// Expected: {compress/decompress} {inputFilePath} {outputFilePath}
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static Options Validation(string[] args)
    {
      if (args == null || args.Length != 3)
        throw new ArgumentException("You need to enter three commands like: compress/decompress inputFilePath outputFilePath");

      var operationString = args[ApplicationConstants.OperationArgNumber];
      var inputFilePath = args[ApplicationConstants.InputFilePathArgNumber]; 
      var outputFilePath = args[ApplicationConstants.OutputFilePathArgNumber];

      var operation = (operationString.Substring(0, 1).ToUpper() + operationString[1..].ToLower()) switch
      {
        nameof(ArchiveActions.Compress) => ArchiveActions.Compress,
        nameof(ArchiveActions.Decompress) => ArchiveActions.Decompress,
        _ => throw new ArgumentException($"Unsupported command: {operationString}. Supported operations list: compress, decompress."),
      };

      if (string.IsNullOrEmpty(inputFilePath) || inputFilePath.IndexOfAny(Path.GetInvalidPathChars()) != -1)
        throw new ArgumentException($"Incorrect input file path: {inputFilePath}");
      if (!File.Exists(inputFilePath))
        throw new ArgumentException($"Input file is not exists: {inputFilePath}");

      if (string.IsNullOrEmpty(inputFilePath) || outputFilePath.IndexOfAny(Path.GetInvalidPathChars()) != -1)
        throw new ArgumentException($"Incorrect input file path: {outputFilePath}");
      if (File.Exists(outputFilePath))
      {
        outputFilePath = GeneratePath(outputFilePath);
      }
      return new Options(operation, inputFilePath, outputFilePath);
    }

    private static string GeneratePath(string filePath)
    {
      var copyIndex = 1;
      var fileName = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath));
      var fileExtension = Path.GetExtension(filePath);

      while (true)
      {
        filePath = $"{fileName}({copyIndex}){fileExtension}";
        if (!File.Exists(filePath))
        {
          using (File.Create(filePath))
            return filePath;
        }

        copyIndex++;
      }
    }
  }
}
