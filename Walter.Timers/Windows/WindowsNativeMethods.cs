using System;
using System.Runtime.InteropServices;

namespace Walter.Timers.Windows
{
  internal static class WindowsNativeMethods
  {
    public delegate void MultimediaTimerCallback(uint id, uint msg, ref uint userCtx, uint rsv1, uint rsv2);
    public delegate void TimerDelegate(IntPtr parameter, bool timerOrWaitFired);

    public const int InvalidHandleValue = -1;
    public const int ErrorIoPending = 997;
    
    [DllImport("winmm.dll", SetLastError = true, EntryPoint = "timeSetEvent")]
    public static extern uint TimeSetEvent(uint msDelay, uint msResolution, MultimediaTimerCallback callback, ref uint userCtx, uint eventType);

    [DllImport("winmm.dll", SetLastError = true, EntryPoint = "timeKillEvent")]
    public static extern void TimeKillEvent(uint uTimerId);

    [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
    public static extern int TimeBeginPeriod(uint period);

    [DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
    public static extern int TimeEndPeriod(uint period);

    [DllImport("kernel32.dll", SetLastError= true)]
    public static extern bool CreateTimerQueueTimer(out IntPtr newTimer, IntPtr timerQueue, TimerDelegate callback, IntPtr parameter, uint dueTime, uint period, uint flags);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool DeleteTimerQueueTimer(IntPtr timerQueue, IntPtr timer, IntPtr completionEvent);
  }
}