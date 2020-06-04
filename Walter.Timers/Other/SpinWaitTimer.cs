using System;
using System.Threading;

namespace Walter.Timers.Other
{
  /// <summary>
  ///   SpinWaitTimer class
  /// </summary>
  /// <see cref="https://www.codeproject.com/Articles/98346/Microsecond-and-Millisecond-NET-Timer" />
  public class SpinWaitTimer : ITimer
  {
    private bool _disposed;
    private long _ignoreEventIfLateBy = long.MaxValue;
    private bool _stopTimer = true;

    private Thread _threadTimer;
    private long _timerIntervalInMicroSec;

    public SpinWaitTimer()
    {
      Interval = 1;
    }

    public long IgnoreEventIfLateBy
    {
      get => Interlocked.Read(ref _ignoreEventIfLateBy);
      set => Interlocked.Exchange(ref _ignoreEventIfLateBy, value <= 0 ? long.MaxValue : value);
    }


    public event EventHandler Elapsed;

    public uint Interval
    {
      get => (uint) Interlocked.Read(ref _timerIntervalInMicroSec) / 1000;
      set
      {
        ThrowIfDisposed();

        if (value == 0)
          throw new ArgumentOutOfRangeException(nameof(Interval), "Must be > 0");

        long intervalInMicroSec = value * 1000;
        Interlocked.Exchange(ref _timerIntervalInMicroSec, intervalInMicroSec);
      }
    }


    public bool IsRunning => _threadTimer != null && _threadTimer.IsAlive;

    public void Start()
    {
      ThrowIfDisposed();

      if (IsRunning)
        return;

      _stopTimer = false;

      _threadTimer = new Thread(() =>
        NotificationTimer(ref _timerIntervalInMicroSec, ref _ignoreEventIfLateBy, ref _stopTimer))
      {
        Priority = ThreadPriority.Highest,
        Name = "SpinWaitTimer",
        IsBackground = true
      };
      _threadTimer.Start();
    }

    public void Stop()
    {
      ThrowIfDisposed();
      _stopTimer = true;
    }

    public void StopAndWait()
    {
      ThrowIfDisposed();
      StopAndWait(Timeout.Infinite);
    }

    public bool StopAndWait(int timeoutInMilliSec)
    {
      ThrowIfDisposed();

      _stopTimer = true;

      if (!IsRunning || _threadTimer.ManagedThreadId == Thread.CurrentThread.ManagedThreadId)
        return true;

      return _threadTimer.Join(timeoutInMilliSec);
    }

    public void Abort()
    {
      ThrowIfDisposed();

      _stopTimer = true;

      if (IsRunning)
        _threadTimer.Abort();
    }

    public void Dispose()
    {
      Dispose(true);
    }

    private void NotificationTimer(ref long timerIntervalInMicroSec,
      ref long ignoreEventIfLateBy,
      ref bool stopTimer)
    {
      var timerCount = 0;
      long nextNotification = 0;

      var microStopwatch = new HiResStopwatch();
      microStopwatch.Start();

      while (!stopTimer)
      {
        var callbackFunctionExecutionTime =
          microStopwatch.ElapsedMicroseconds - nextNotification;

        var timerIntervalInMicroSecCurrent = Interlocked.Read(ref timerIntervalInMicroSec);
        var ignoreEventIfLateByCurrent = Interlocked.Read(ref ignoreEventIfLateBy);

        nextNotification += timerIntervalInMicroSecCurrent;
        timerCount++;
        long elapsedMicroseconds;

        while ((elapsedMicroseconds = microStopwatch.ElapsedMicroseconds)
               < nextNotification)
          Thread.SpinWait(10);

        var timerLateBy = elapsedMicroseconds - nextNotification;

        if (timerLateBy >= ignoreEventIfLateByCurrent)
          continue;

        var microTimerEventArgs = new SpinWaitTimerEventArgs(timerCount,
          elapsedMicroseconds,
          timerLateBy,
          callbackFunctionExecutionTime);

        Elapsed?.Invoke(this, microTimerEventArgs);
      }

      microStopwatch.Stop();
    }

    private void ThrowIfDisposed()
    {
      if (_disposed)
        throw new ObjectDisposedException(nameof(SpinWaitTimer));
    }

    private void Dispose(bool disposing)
    {
      if (_disposed)
        return;

      if (IsRunning)
        StopAndWait();

      _disposed = true;

      if (!disposing)
        return;

      Elapsed = null;
      GC.SuppressFinalize(this);
    }
  }
}