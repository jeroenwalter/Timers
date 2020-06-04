using System;
using System.Threading;

namespace Walter.Timers
{
  /// <summary>
  ///   TimerEvent will subscribe on a timer and sets an AutoResetEvent each timer tick.
  /// </summary>
  public class TimerEvent : ITimerEvent, IDisposable
  {
    public TimerEvent(ITimer timer)
    {
      Timer = timer;
      WaitHandle = new AutoResetEvent(false);
      Timer.Elapsed += TimerOnElapsed;
    }

    public void Dispose()
    {
      Timer.Elapsed -= TimerOnElapsed;
      WaitHandle?.Dispose();
    }

    public ITimer Timer { get; }

    public AutoResetEvent WaitHandle { get; }

    public void WaitOne()
    {
      if (!Timer.IsRunning)
        throw new InvalidOperationException("Timer is not running");

      WaitHandle.WaitOne();
    }

    public bool WaitOne(int millisecondsTimeout)
    {
      if (!Timer.IsRunning)
        throw new InvalidOperationException("Timer is not running");

      return WaitHandle.WaitOne(millisecondsTimeout);
    }

    private void TimerOnElapsed(object sender, EventArgs e)
    {
      WaitHandle.Set();
    }
  }
}