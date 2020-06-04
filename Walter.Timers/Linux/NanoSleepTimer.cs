using System;
using System.Diagnostics;
using System.Threading;
using Mono.Unix.Native;

namespace Walter.Timers.Linux
{
  /// <summary>
  ///   Timer class for Linux
  ///   source: https://stackoverflow.com/questions/37814505/mono-high-resolution-timer-on-linux/37882723#37882723
  /// </summary>
  public class NanoSleepTimer : ITimer
  {
    private const uint SafeDelay = 0; // millisecond (for slightly early wakeup)
    private readonly object _lockObject = new object();
    private readonly Stopwatch _watch = new Stopwatch(); // High resolution time
    private Timespec _pendingNanosleepParams;
    private Timespec _threadNanosleepParams;
    private Thread _threadTimer;

    public event EventHandler Elapsed;

    public bool IsRunning { get; private set; }

    public void Start()
    {
      _watch.Start();
      IsRunning = true;

      _threadTimer = new Thread(TickGenerator)
      {
        Priority = ThreadPriority.Highest,
        Name = nameof(NanoSleepTimer),
        IsBackground = true
      };
      _threadTimer.Start();
    }

    public void Stop()
    {
      if (!IsRunning)
        return;

      lock (_lockObject)
      {
        IsRunning = false;
      }

      _threadTimer.Join(1000);
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

    public uint Interval
    {
      get
      {
        double totalNanoseconds;
        lock (_lockObject)
        {
          totalNanoseconds = 1e9 * _pendingNanosleepParams.tv_sec
                             + _pendingNanosleepParams.tv_nsec;
        }

        return (uint) (totalNanoseconds * 1e-6); //return value in ms
      }
      set
      {
        lock (_lockObject)
        {
          _pendingNanosleepParams.tv_sec = value / 1000;
          _pendingNanosleepParams.tv_nsec = (long) (value % 1000 * 1e6); //set value in ns
        }
      }
    }

    public void Dispose()
    {
      if (IsRunning)
        StopAndWait();
      Elapsed = null;
      GC.SuppressFinalize(this);
    }

    private void TickGenerator()
    {
      bool bNotPendingStop;
      lock (_lockObject)
      {
        bNotPendingStop = IsRunning;
      }

      while (bNotPendingStop)
      {
        // Check if thread has been told to halt

        lock (_lockObject)
        {
          bNotPendingStop = IsRunning;
        }

        var curTime = _watch.ElapsedMilliseconds;
        if (curTime >= Interval)
        {
          _watch.Restart();
          Elapsed?.Invoke(this, EventArgs.Empty);
        }
        else
        {
          var iTimeLeft = Interval - curTime; // How long to delay for 
          if (iTimeLeft >= SafeDelay)
          {
            // Task.Delay has resolution 15ms//await Task.Delay(TimeSpan.FromMilliseconds(iTimeLeft - safeDelay));
            _threadNanosleepParams.tv_nsec = (int) ((iTimeLeft - SafeDelay) * 1e6);
            _threadNanosleepParams.tv_sec = 0;
            Syscall.nanosleep(ref _threadNanosleepParams, ref _threadNanosleepParams);
          }
        }
      }

      _watch.Stop();
    }
  }
}