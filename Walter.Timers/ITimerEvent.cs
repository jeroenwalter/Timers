using System;
using System.Threading;

namespace Walter.Timers
{
  /// <summary>
  /// Subscribe on a timer and set an AutoResetEvent each timer tick.
  /// </summary>
  public interface ITimerEvent
  {
    ITimer Timer { get; }

    AutoResetEvent WaitHandle { get; }

    /// <summary>
    /// Wait for the next timer tick.
    /// </summary>
    /// <remarks>Wrapper for AutoResetEvent.WaitOne</remarks>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, or Infinite (-1) to wait indefinitely.</param>
    /// <returns>true if the current instance receives a signal; otherwise, false.</returns>
    bool WaitOne(int millisecondsTimeout);

    /// <summary>
    /// Wait for the next timer tick.
    /// </summary>
    /// <remarks>Wrapper for AutoResetEvent.WaitOne</remarks>
    void WaitOne();
  }
}