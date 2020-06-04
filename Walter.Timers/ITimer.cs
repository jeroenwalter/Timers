using System;

namespace Walter.Timers
{
  public interface ITimer : IDisposable
  {
    /// <summary>
    ///   Interval in milliseconds
    /// </summary>
    uint Interval { get; set; }

    /// <summary>
    ///   True if the timer is running
    /// </summary>
    bool IsRunning { get; }

    event EventHandler Elapsed;

    void Start();

    void Stop();
    void StopAndWait();
    bool StopAndWait(int timeoutInMilliSec);
    void Abort();
  }
}