﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace 
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
			public int Config1 { get; set; }
		}
	}
}
