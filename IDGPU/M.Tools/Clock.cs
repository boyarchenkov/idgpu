using System;
using System.Runtime.InteropServices;

namespace M.Tools
{
	public class Clock
	{
		public float ElapsedTime
		{
			get
			{
				long tick_count = 0;
				QueryPerformanceCounter(out tick_count);
				return (tick_count - last_tick_count) * freq;
			}
		}

		public Clock()
		{
			long f;
			QueryPerformanceFrequency(out f);
			freq = 1.0f / f;
			QueryPerformanceCounter(out last_tick_count);
		}

		[DllImport("Kernel32.dll")]
		private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

		[DllImport("Kernel32.dll")]
		private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        [DllImport("Winmm.dll", EntryPoint = "timeBeginPeriod")]
        public static extern uint TimeBeginPeriod(uint period);

        [DllImport("Winmm.dll", EntryPoint = "timeEndPeriod")]
        public static extern uint TimeEndPeriod(uint period);

        private long last_tick_count;
		private float freq;
	}

}
