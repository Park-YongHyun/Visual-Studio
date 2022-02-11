using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SoundControl
{
	public class Config
	{
		private static JsonRoot _jsonRoot;
		private static readonly string filePath = $"{Directory.GetParent(Process.GetCurrentProcess().MainModule.FileName)}/config.json";
		private static readonly JsonSerializerOptions jsonSerializerOptions = new()
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
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
			_jsonRoot = JsonSerializer.Deserialize<JsonRoot>(File.ReadAllText(filePath), jsonSerializerOptions);

			// 볼륨 계산
			if (CalculateVolumeLevel()) SaveJson();
		}

		public static void SaveJson()
		{
			File.WriteAllText(filePath, JsonSerializer.Serialize(_jsonRoot, jsonSerializerOptions));
		}

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
			public int ExponentiationBase { get; set; }
			public bool Calculated { get; set; }
			public double[] List { get; set; }
		}


		/*	볼륨 계산
		 *		데시벨(dB)인 로그함수의 역함수인 지수함수로 계산
		 *		pow(base, x / (step - 1)) / base
		 *			최소값과 최대값의 차이: {base}배
		 */
		public static bool CalculateVolumeLevel()
		{
			Volume volume = GetRoot.Volume;
			if (!volume.Level.Calculated)
			{
				volume.Level.List = new double[volume.StepCount];

				for (int i = 0; i < volume.StepCount; i++)
				{
					volume.Level.List[i] = Math.Round(Math.Pow(volume.Level.ExponentiationBase, (double)i / (volume.StepCount - 1)) / volume.Level.ExponentiationBase, 5);
					//volume.LevelList[i] = Math.Pow(volume.ExponentiationBase, (double)i / (volume.StepCount - 1)) / volume.ExponentiationBase;
				}

				return volume.Level.Calculated = true;
			}
			return false;
		}
	}
}
