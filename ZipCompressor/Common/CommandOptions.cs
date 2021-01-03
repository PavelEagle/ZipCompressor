using System;
using System.IO;

namespace ZipCompressor.Common
{
  public struct CommandOptions
  {
    public CommandOptions(ArchiveModes mode, string inputFilePath, string outputFilePath)
    {
      Mode = mode;
      InputFilePath = inputFilePath;
      OutputFilePath = outputFilePath;
    }

    public ArchiveModes Mode { get; }
    public string InputFilePath { get; }
    public string OutputFilePath { get; }

    /// <summary>
    /// Parse input command line arguments.
    /// Expected: {compress/decompress} {inputFilePath} {outputFilePath}
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static CommandOptions Create(string[] args)
    {
      if (args == null || args.Length != 3)
        throw new ArgumentException("You need to enter three commands like: compress/decompress inputFilePath outputFilePath");

      var operationString = args[ApplicationConstants.OperationArgNumber];
      var inputFilePath = args[ApplicationConstants.InputFilePathArgNumber]; 
      var outputFilePath = args[ApplicationConstants.OutputFilePathArgNumber];

      var operation = (operationString.Substring(0, 1).ToUpper() + operationString[1..].ToLower()) switch
      {
        nameof(ArchiveModes.Compress) => ArchiveModes.Compress,
        nameof(ArchiveModes.Decompress) => ArchiveModes.Decompress,
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
      return new CommandOptions(operation, inputFilePath, outputFilePath);
    }

    private static string GeneratePath(string filePath)
    {
      var outputFilePath = filePath;
      var copyIndex = 1;

      while (true)
      {
        filePath = $"{outputFilePath.Substring(0, outputFilePath.Length - 3)}({copyIndex}).gz";
        if (!File.Exists(filePath))
        {
          File.Create(filePath);
          return filePath;
        }

        copyIndex++;
      }
    }
  }
}
