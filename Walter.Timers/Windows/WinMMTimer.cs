using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Walter.Timers.Windows
{
  /// <summary>
  /// This time is Windows specific and uses the multimedia timer API, e.g. timerSetEvent etc.
  /// </summary>
  public class WinMmTimer : ITimer
  {
    private bool _disposed;
    private uint _interval;
    private uint _resolution;
    private uint _timerId;

    // Hold the timer callback to prevent garbage collection.
    private readonly WindowsNativeMethods.MultimediaTimerCallback _callback;

    public WinMmTimer()
    {
      _callback = TimerCallbackMethod;
      Resolution = 0;
      Interval = 1;
    }

    ~WinMmTimer()
    {
      Dispose(false);
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

        if (Resolution > Interval)
          Resolution = value;
      }
    }

    // Note minimum resolution is 0, meaning highest possible resolution.
    public uint Resolution
    {
      get => _resolution;
      set
      {
        ThrowIfDisposed();

        _resolution = value;
      }
    }

    public event EventHandler Elapsed;

    public bool IsRunning => _timerId != 0;

    public void Start()
    {
      ThrowIfDisposed();

      if (IsRunning)
        throw new InvalidOperationException("Timer is already running");

      WindowsNativeMethods.TimeBeginPeriod(1);

      uint userCtx = 0;
      _timerId = WindowsNativeMethods.TimeSetEvent(Interval, Resolution, _callback, ref userCtx, 1);
      if (_timerId != 0) 
        return;

      WindowsNativeMethods.TimeEndPeriod(1);
      var error = Marshal.GetLastWin32Error();
      throw new Win32Exception(error);
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


    private void StopInternal()
    {
      WindowsNativeMethods.TimeKillEvent(_timerId);
      WindowsNativeMethods.TimeEndPeriod(1);
      _timerId = 0;
    }


    private void TimerCallbackMethod(uint id, uint msg, ref uint userCtx, uint rsv1, uint rsv2)
    {
      Elapsed?.Invoke(this, EventArgs.Empty);
    }


    public void Dispose()
    {
      Dispose(true);
    }

    private void ThrowIfDisposed()
    {
      if (_disposed)
        throw new ObjectDisposedException(nameof(WinMmTimer));
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
  }
}
