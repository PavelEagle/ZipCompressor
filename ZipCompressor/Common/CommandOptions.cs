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

    public Commands Mode { get;}
    public string InputFileName { get; }
    public string OutputFileName { get; }

    public static CommandOptions Create(string[] args)
    {
      if (args == null || args.Length != 3)
        throw new ArgumentException("Three command-line parameters are expected: operation type (compress or decompress), input file path, output file path.");

      var operationString = args[0]; //const
      var operation = (operationString.Substring(0,1).ToUpper()+ operationString.Substring(1).ToLower()) switch
      {
        nameof(Commands.Compression) => Commands.Compression,
        nameof(Commands.Decompression) => Commands.Decompression,
        nameof(Commands.Create) => Commands.Create,
        _ => throw new ArgumentException($"Unsupported operation: {operationString}. Supported operations list: compress, decompress."),
      };

      var inputFileName = args[1]; //const
      if (inputFileName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
        throw new ArgumentException($"Invalid input file parameter: {inputFileName}. It must be correct file path.");
      if (!File.Exists(inputFileName)) { }
      File.Create(inputFileName);
      //throw new ArgumentException($"Input file with path {inputFilePath} is not exists. Nothing to compress.");
      var outputFileName = args[2];
      if (outputFileName.IndexOfAny(Path.GetInvalidPathChars()) != -1)
        throw new ArgumentException($"Invalid output file parameter: {outputFileName}. It must be correct file path.");
      if (File.Exists(outputFileName))
        throw new ArgumentException($"Output file with path {outputFileName} is already exists. Remove it or choose another name for output file.");

      Console.WriteLine($"Yes! {operationString}, {inputFileName}, {outputFileName}");

      return new CommandOptions(operation, inputFileName, outputFileName);
    }
  }
}
