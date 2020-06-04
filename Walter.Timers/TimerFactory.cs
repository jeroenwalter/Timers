using System;
using System.Runtime.InteropServices;
using Walter.Timers.Linux;
using Walter.Timers.Windows;

namespace Walter.Timers
{
  public static class TimerFactory
  {
    public static ITimer Create()
    {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        return new NanoSleepTimer();

      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        //return new WinMmTimer();
        return new TimerQueueTimer();

      throw new PlatformNotSupportedException($"Platform {RuntimeInformation.OSDescription} is not supported.");
    }
  }
}
