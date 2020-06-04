using System;

namespace Walter.Timers.Other
{
  /// <summary>
  ///   SpinWaitTimer Event Argument class
  /// </summary>
  /// <see cref="https://www.codeproject.com/Articles/98346/Microsecond-and-Millisecond-NET-Timer" />
  public class SpinWaitTimerEventArgs : EventArgs
  {
    public SpinWaitTimerEventArgs(int timerCount,
      long elapsedMicroseconds,
      long timerLateBy,
      long callbackFunctionExecutionTime)
    {
      TimerCount = timerCount;
      ElapsedMicroseconds = elapsedMicroseconds;
      TimerLateBy = timerLateBy;
      CallbackFunctionExecutionTime = callbackFunctionExecutionTime;
    }

    // Simple counter, number times timed event (callback function) executed
    public int TimerCount { get; }

    // Time when timed event was called since timer started
    public long ElapsedMicroseconds { get; }

    // How late the timer was compared to when it should have been called
    public long TimerLateBy { get; }

    // Time it took to execute previous call to callback function (OnTimedEvent)
    public long CallbackFunctionExecutionTime { get; }
  }
}