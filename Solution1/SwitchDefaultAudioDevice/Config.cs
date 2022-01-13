using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SwitchDefaultAudioDevice
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
		}

		public static void SaveJson()
		{
			File.WriteAllText(filePath, JsonSerializer.Serialize(_jsonRoot, jsonSerializerOptions));
		}

		public class JsonRoot
		{
			public Audio Audio { get; set; }
			public UnmuteSystemSounds UnmuteSystemSounds { get; set; }
			public SoundVolumeView SoundVolumeView { get; set; }
		}
		public class Audio
		{
			public string Device1 { get; set; }
			public string Device2 { get; set; }
			public string CurrentDevice { get; set; }
		}
		public class UnmuteSystemSounds
		{
			public bool IsEnabled { get; set; }
			public string RelativePath { get; set; }
			public string AbsolutePath { get; set; }
		}
		public class SoundVolumeView
		{
			public string RelativePath { get; set; }
			public string AbsolutePath { get; set; }
		}
	}
}
