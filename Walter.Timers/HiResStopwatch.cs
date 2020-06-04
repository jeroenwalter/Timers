using System;
using System.Diagnostics;

namespace Walter.Timers
{
  /// <summary>
  ///   HiResStopwatch class
  /// </summary>
  /// <see cref="https://www.codeproject.com/Articles/98346/Microsecond-and-Millisecond-NET-Timer" />
  public class HiResStopwatch : Stopwatch
  {
    private readonly double _microSecPerTick = 1000000D / Frequency;

    public HiResStopwatch()
    {
      if (!IsHighResolution)
        throw new Exception("On this system the high-resolution performance counter is not available");
    }

    public long ElapsedMicroseconds => (long) (ElapsedTicks * _microSecPerTick);
  }
}