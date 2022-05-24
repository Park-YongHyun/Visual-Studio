using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GpuCoolerControl
{
	// v1.0.0.20220523
	public class Config
	{
		private static JsonRoot _jsonRoot;
		private static readonly string filePath = $"{Directory.GetParent(Environment.ProcessPath!)}/config.json";
		private static readonly JsonSerializerOptions jsonSerializerOptions = new()
		{
			WriteIndented = true
		};

		public static JsonRoot GetRoot
		{
			get
			{
				if (_jsonRoot == null) LoadJson();
				return _jsonRoot;
			}
		}

		public static void LoadJson()
		{
			_jsonRoot = JsonSerializer.Deserialize<JsonRoot>(File.ReadAllText(filePath));
		}

		public static void SaveJson()
		{
			File.WriteAllText(filePath, JsonSerializer.Serialize(_jsonRoot, jsonSerializerOptions));
		}

		// lv0(root)
		public class JsonRoot
		{
			public Control Control { get; set; }
			public Gpu Gpu { get; set; }
		}
		// lv1
		public class Control
		{
			public bool AutoStart { get; set; }
			public int TimerIntervalMilliseconds { get; set; }
		}
		public class Gpu
		{
			public Usage Usage { get; set; }
			public Temperature Temperature { get; set; }
		}
		// lv2
		public class Usage
		{
			public int MinUsage { get; set; }
			public int MaxUsage { get; set; }
			public int MinClockFrequencyMHz { get; set; }
			public int MaxClockFrequencyMHz { get; set; }
			public int MinFanSpeedLevel { get; set; }
			public int MaxFanSpeedLevel { get; set; }
		}
		public class Temperature
		{
			public int MinTemperature { get; set; }
			public int MaxTemperature { get; set; }
			public int MinFanSpeedLevel { get; set; }
			public int MaxFanSpeedLevel { get; set; }
		}
	}
}
