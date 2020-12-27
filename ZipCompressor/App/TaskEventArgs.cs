using System.Threading;

namespace ZipCompressor.App
{
  public class TaskEventArgs
  {
    public readonly Thread Thread;

    public TaskEventArgs(Thread thread)
    {
      Thread = thread;
    }
  }
}