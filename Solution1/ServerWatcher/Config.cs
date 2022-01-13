using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServerWatcher
{
	public class Config
	{
		private static JsonRoot _jsonRoot;
		private static readonly string filePath = $"{Directory.GetParent(Process.GetCurrentProcess().MainModule.FileName)}/config.json";
		private static readonly JsonSerializerOptions jsonSerializerOptions = new() {
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
			public SshServer SshServer { get; set; }
			public Server Server { get; set; }
		}
		public class SshServer
		{
			public string Host { get; set; }
			public int Port { get; set; }
			public string UserName { get; set; }
			public string Password { get; set; }
			public string DockerContainerName { get; set; }
		}
		public class Server
		{
			public string Url { get; set; }
			public Interval Interval { get; set; }
		}
		public class Interval
		{
			public int Hours { get; set; }
			public int Minutes { get; set; }
			public int Seconds { get; set; }
		}
	}
}
