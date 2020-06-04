using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Walter.Timers.Windows
{
  /// <summary>
  ///   This time is Windows specific and uses the Timer Queue API, e.g. CreateTimerQueueTimer etc.
  /// </summary>
  public class TimerQueueTimer : ITimer
  {
    // Hold the timer callback to prevent garbage collection.
    private readonly WindowsNativeMethods.TimerDelegate _callback;
    private bool _disposed;
    private uint _interval;
    private volatile IntPtr _timer;

    public TimerQueueTimer()
    {
      _callback = TimerCallbackMethod;
      Interval = 1;
      _timer = IntPtr.Zero;
    }

    public uint Interval
    {
      get => _interval;
      set
      {
        ThrowIfDisposed();

        if (value == 0)
          throw new ArgumentOutOfRangeException(nameof(Interval), "Must be > 0");

        _interval = value;
      }
    }


    public event EventHandler Elapsed;

    public bool IsRunning => _timer != IntPtr.Zero;

    public void Start()
    {
      ThrowIfDisposed();

      if (IsRunning)
        throw new InvalidOperationException("Timer is already running");

      var pParameter = IntPtr.Zero;

      WindowsNativeMethods.TimeBeginPeriod(1);
      if (WindowsNativeMethods.CreateTimerQueueTimer(out var phNewTimer, IntPtr.Zero, _callback, pParameter, Interval,
        Interval, 0))
      {
        _timer = phNewTimer;
        return;
      }

      var lastError = Marshal.GetLastWin32Error();
      WindowsNativeMethods.TimeEndPeriod(1);

      throw new Win32Exception(lastError, "Failed to create the timer");
    }

    public void Stop()
    {
      ThrowIfDisposed();

      if (!IsRunning)
        throw new InvalidOperationException("Timer has not been started");

      StopInternal();
    }

    public void StopAndWait()
    {
      Stop();
    }

    public bool StopAndWait(int timeoutInMilliSec)
    {
      Stop();
      return true;
    }

    public void Abort()
    {
      Stop();
    }


    public void Dispose()
    {
      Dispose(true);
    }


    private void StopInternal()
    {
      WindowsNativeMethods.TimeEndPeriod(1);
      if (!WindowsNativeMethods.DeleteTimerQueueTimer(IntPtr.Zero, _timer, IntPtr.Zero))
      {
        var lastError = Marshal.GetLastWin32Error();
        if (lastError != WindowsNativeMethods.ErrorIoPending)
        {
          Debug.WriteLine("DeleteTimerQueueTimer error {0} (0x{0:X}), retrying", lastError);

          // retry and wait for any running timer callback functions to complete
          if (!WindowsNativeMethods.DeleteTimerQueueTimer(IntPtr.Zero, _timer,
            (IntPtr) WindowsNativeMethods.InvalidHandleValue))
          {
            lastError = Marshal.GetLastWin32Error();
            throw new Win32Exception(lastError, "Failed to delete the timer");
          }
        }
      }

      _timer = IntPtr.Zero;
    }


    private void TimerCallbackMethod(IntPtr parameter, bool timerOrWaitFired)
    {
      Elapsed?.Invoke(this, EventArgs.Empty);
    }

    private void ThrowIfDisposed()
    {
      if (_disposed)
        throw new ObjectDisposedException(nameof(TimerQueueTimer));
    }

    private void Dispose(bool disposing)
    {
      if (_disposed)
        return;

      _disposed = true;

      if (IsRunning)
        StopInternal();

      if (!disposing)
        return;

      Elapsed = null;

      GC.SuppressFinalize(this);
    }

    ~TimerQueueTimer()
    {
      Dispose(false);
    }
  }
}