using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundControl.Configuration
{
	public class ConfigData
	{
		public class JsonRoot
		{
			public Process Process { get; set; }
			public Volume Volume { get; set; }
			public Popup Popup { get; set; }
			public Audio Audio { get; set; }
		}

		public class Process
		{
			public string PriorityDescription { get; set; }
			public string Priority { get; set; }
			public int ProcessorAffinity { get; set; }
		}
		public class Volume
		{
			public int StepCount { get; set; }
			public Level Level { get; set; }
		}
		public class Level
		{
			public bool Calculated { get; set; }
			public int ExponentiationBase { get; set; }
			public double MinLevel { get; set; }
			public double MaxLevel { get; set; }
			public double[] List { get; set; }
		}

		public class Popup
		{
			public int TimeoutMillisec { get; set; }
			public double WindowOpacity { get; set; }
		}

		public class Audio
		{
			public string Device1Name { get; set; }
			public string Device2Name { get; set; }
			public bool UnmuteSystemSound { get; set; }
			public TestBeep TestBeep { get; set; }
		}
		public class TestBeep
		{
			public bool UseSystemBeep { get; set; }
			public int Frequency { get; set; }
			public int DurationMillisec { get; set; }
		}
	}
}
