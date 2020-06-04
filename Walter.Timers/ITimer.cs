using System;

namespace Walter.Timers
{
  public interface ITimer : IDisposable
  {
    event EventHandler Elapsed;

    /// <summary>
    /// Interval in milliseconds
    /// </summary>
    uint Interval { get; set; }

    /// <summary>
    /// True if the timer is running
    /// </summary>
    bool IsRunning { get; }
    
    void Start();
    
    void Stop();
    void StopAndWait();
    bool StopAndWait(int timeoutInMilliSec);
    void Abort();
  }
}