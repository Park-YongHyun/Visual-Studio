using System;
using System.Diagnostics;
using System.IO;

namespace SwitchDefaultAudioDevice
{
	class Program
	{
		static void Main(string[] args)
		{
			string absolutePath;

			// UnmuteSystemSounds 실행
			if (Config.GetRoot.UnmuteSystemSounds.IsEnabled)
			{
				absolutePath = Config.GetRoot.UnmuteSystemSounds.AbsolutePath;
				if (!processStartWait(absolutePath))
				{
					absolutePath = findAbsolutePath(Config.GetRoot.UnmuteSystemSounds.RelativePath);
					if (absolutePath != "" && processStartWait(absolutePath))
					{
						Config.GetRoot.UnmuteSystemSounds.AbsolutePath = absolutePath;
						Config.SaveJson();
					}
				}
			}

			// SoundVolumeView 실행
			absolutePath = Config.GetRoot.SoundVolumeView.AbsolutePath;
			Config.GetRoot.Audio.CurrentDevice = Config.GetRoot.Audio.CurrentDevice != Config.GetRoot.Audio.Device1 ? Config.GetRoot.Audio.Device1 : Config.GetRoot.Audio.Device2;
			string processArgs = $"/SetDefault {Config.GetRoot.Audio.CurrentDevice} all";

			if (!processStartWait(absolutePath, processArgs))
			{
				absolutePath = findAbsolutePath(Config.GetRoot.SoundVolumeView.RelativePath);
				if (absolutePath != "" && processStartWait(absolutePath, processArgs))
				{
					Config.GetRoot.SoundVolumeView.AbsolutePath = absolutePath;
					Config.SaveJson();
				}
			}
			else
			{
				Config.SaveJson();
			}

#pragma warning disable CA1416 // 플랫폼 호환성 유효성 검사
			System.Media.SystemSounds.Beep.Play();
#pragma warning restore CA1416 // 플랫폼 호환성 유효성 검사


			string findAbsolutePath(string relativePath)
			{
				string dirPath = Directory.GetParent(Process.GetCurrentProcess().MainModule.FileName).FullName;
				DirectoryInfo dirInfo;
				while ((dirInfo = Directory.GetParent(dirPath)) != null)
				{
					if (File.Exists($"{dirPath}\\{relativePath}"))
						return $"{dirPath}\\{relativePath}";
					dirPath = dirInfo.FullName;
				}
				return "";
			}

			bool processStartWait(string path, string args = "")
			{
				if (File.Exists(path))
				{
					Process unmuteSystemSoundsExe = new() { StartInfo = new ProcessStartInfo() { FileName = path, Arguments = args } };
					unmuteSystemSoundsExe.Start();
					unmuteSystemSoundsExe.WaitForExit();
					return true;
				}
				return false;
			}
		}
	}
}
