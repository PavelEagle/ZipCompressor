using System;
using System.Collections.Generic;
using ZipCompressor.App.Actions;
using ZipCompressor.App.BaseCompressor;
using ZipCompressor.Common;

namespace ZipCompressor.App
{
  public class ZipApplication: IDisposable
  {
    private readonly IArchiver _compressor;
    private readonly CommandOptions _zipSettings;
    private readonly ActionPipeline _actionPipeline;

    public ZipApplication(CommandOptions zipSettings, IArchiver compressor)
    {
      _zipSettings = zipSettings;
      _compressor = compressor;
    }

    //public static ZipApplication Create(CommandOptions zipSettings)
    //{
      
    //}

    private ActionPipeline CreatePipeline(Commands commands)
    {
      if (commands != Commands.Compression || commands != Commands.Decompression)
        throw new ArgumentException("Unknown operation type");

      var result = new List<IAction>();

      //result.Add(new ReadAction());
      //result.Add(commands == Commands.Compression ? (IAction) new CompressAction() : new DecompressAction());
      //result.Add(new ReadAction());

      return new ActionPipeline(result);
    }
    public void StartProcess()
    {
   
    }

    public void StopProcess()
    {
    }

    public void Dispose()
    {
      _actionPipeline.Clear();
    }
  }
}
