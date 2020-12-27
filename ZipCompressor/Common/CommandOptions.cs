using System;
using System.IO;

namespace ZipCompressor.Common
{
  public struct CommandOptions
  {
    public CommandOptions(Commands mode, string inputFileName, string outputFileName)
    {
      Mode = mode;
      InputFileName = inputFileName;
      OutputFileName = outputFileName;
    }

    public Commands Mode { get; }
    public string InputFileName { get; }
    public string OutputFileName { get; }

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

      var operationString = args[Constants.OperationArgNumber];
      var inputFileName = args[Constants.InputFilePathArgNumber]; 
      var outputFileName = args[Constants.OutputFilePathArgNumber];

      var operation = (operationString.Substring(0, 1).ToUpper() + operationString[1..].ToLower()) switch
      {
        nameof(Commands.Compress) => Commands.Compress,
        nameof(Commands.Decompress) => Commands.Decompress,
        _ => throw new ArgumentException($"Unsupported command: {operationString}. Supported operations list: compress, decompress."),
      };

      if (string.IsNullOrEmpty(inputFileName) || inputFileName.IndexOfAny(Path.GetInvalidPathChars()) != -1)
        throw new ArgumentException($"Incorrect input file path: {inputFileName}");
      if (!File.Exists(inputFileName))
        throw new ArgumentException($"Input file is not exists: {inputFileName}");

      if (string.IsNullOrEmpty(inputFileName) || outputFileName.IndexOfAny(Path.GetInvalidPathChars()) != -1)
        throw new ArgumentException($"Incorrect input file path: {outputFileName}");
      if (File.Exists(outputFileName))
      {
        outputFileName = GeneratePath(outputFileName);
      }
      return new CommandOptions(operation, inputFileName, outputFileName);
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
