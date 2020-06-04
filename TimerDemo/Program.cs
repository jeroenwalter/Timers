using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Walter.Timers;
using Walter.Timers.Other;

namespace TimerDemo
{
  class Program
  {
    static void Main(string[] args)
    {
      var program = new Program();

      program.TimerTest(TimerFactory.Create());
      program.TimerTest(new SpinWaitTimer());

      program.TimerEventTest(TimerFactory.Create());
      program.TimerEventTest(new SpinWaitTimer());

      Console.WriteLine("Finished...");

      if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) 
        return;

      Console.WriteLine("Press Enter to quit.");
      Console.ReadLine();
    }

    private void TimerTest(ITimer timer)
    {
      Console.WriteLine($"TimerTest {timer.GetType().Name}");
      var stopwatch = new HiResStopwatch();
      stopwatch.Start();

      timer.Interval = 5;
      timer.Elapsed += (sender, args) => OnTimerTick(stopwatch, args);
      timer.Start();

      // Do something whilst events happening, for demo sleep 2000ms (2sec)
      System.Threading.Thread.Sleep(2000);

      Console.WriteLine("Stopping...");

      timer.Dispose();
    }

    private static void OnTimerTick(HiResStopwatch stopwatch, EventArgs eventArgs)
    {
      if (eventArgs is SpinWaitTimerEventArgs timerEventArgs)
      {
        Console.WriteLine(
          $"Count = {timerEventArgs.TimerCount:#,0}  Timer = {timerEventArgs.ElapsedMicroseconds:#,0} µs, " +
          $"LateBy = {timerEventArgs.TimerLateBy:#,0} µs, ExecutionTime = {timerEventArgs.CallbackFunctionExecutionTime:#,0} µs");
      }
      else
      {
        Console.WriteLine($"Count = {0:#,0}  Timer = {stopwatch.ElapsedMicroseconds:#,0} µs, " +
                          $"LateBy = {0:#,0} µs, ExecutionTime = {0:#,0} µs");
      }
    }

    private void TimerEventTest(ITimer timer)
    {
      Console.WriteLine($"TimerEventTest {timer.GetType().Name}");
      timer.Interval = 5;
      timer.Start();
      
      Task.Run(() =>
      {
        var stopwatch = new HiResStopwatch();
        stopwatch.Start();

        var timerEvent = new TimerEvent(timer);
        var count = 0;
        while (count++ < 400)
        {
          timerEvent.WaitOne();
          Console.WriteLine($"TimerEventTest TickCount {count}, Timer = {stopwatch.ElapsedMicroseconds:#,0} µs");
        }

        timerEvent.Dispose();
        stopwatch.Stop();
      }).Wait();
      
      Console.WriteLine("Stopping...");
      
      timer.Dispose();
    }
  }
}
