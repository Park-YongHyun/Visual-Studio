using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundControl.Configuration
{
	public class ConfigData
	{
		// lv0(root)
		public class JsonRoot
		{
			public Volume Volume { get; set; }
			public Popup Popup { get; set; }
			public Audio Audio { get; set; }
		}

		// lv1
		public class Volume
		{
			public int StepCount { get; set; }
			public Level Level { get; set; }
		}
		public class Popup
		{
			public int TimeoutMilliseconds { get; set; }
			public double WindowOpacity { get; set; }
		}
		public class Audio
		{
			public string Device1Name { get; set; }
			public string Device2Name { get; set; }
			public bool UnmuteSystemSound { get; set; }
		}

		// lv2
		public class Level
		{
			public bool Calculated { get; set; }
			public int ExponentiationBase { get; set; }
			public double MinLevel { get; set; }
			public double MaxLevel { get; set; }
			public double[] List { get; set; }
		}
	}
}
