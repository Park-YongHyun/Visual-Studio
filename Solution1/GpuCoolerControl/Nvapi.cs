using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GpuCoolerControl
{
	internal class Nvapi
	{
		public class CoolerControl
		{
			private static int currentFanSpeed = 0;

			private static readonly NvAPIWrapper.GPU.PhysicalGPU gpu = NvAPIWrapper.GPU.PhysicalGPU.GetPhysicalGPUs()[0];

			// 타뷸레이션
			private static readonly Dictionary<(int, int), int> fanSpeedLevelsOfUsage = new();
			private static readonly Dictionary<int, int> fanSpeedLevelsOfTemp = new();

			/*	쿨러 팬 속도
			 *	사용량 비례, 온도 비례 중 가장 높은 값 사용
			 */
			public static void SetFanSpeedLevel()
			{
				int fanSpeedLevel = Math.Max(ProportionalToUsage(), ProportionalToTemperature());

				if (fanSpeedLevel != gpu.CoolerInformation.CurrentFanSpeedLevel)
				{
					gpu.CoolerInformation.SetCoolerSettings(0, NvAPIWrapper.Native.GPU.CoolerPolicy.Manual, fanSpeedLevel);
				}
			}

			// 타뷸레이션 계산
			public static void CalculatingTabulation()
			{
				Config.Usage configGpuUsage = Config.GetRoot.Gpu.Usage;
				Config.Temperature configGpuTemp = Config.GetRoot.Gpu.Temperature;

				// 사용량 비례
				for (int usage = configGpuUsage.MinUsage; usage <= configGpuUsage.MaxUsage; usage++)
				{
					for (int clock = configGpuUsage.MinClockFrequencyMHz; clock <= configGpuUsage.MaxClockFrequencyMHz; clock++)
					{
						double usagePercentage = (double)(usage - configGpuUsage.MinUsage) / (configGpuUsage.MaxUsage - configGpuUsage.MinUsage);
						double clockPercentage = (double)(clock - configGpuUsage.MinClockFrequencyMHz) / (configGpuUsage.MaxClockFrequencyMHz - configGpuUsage.MinClockFrequencyMHz);
						int fanSpeedLevel = (int)Math.Round((usagePercentage * clockPercentage * (configGpuUsage.MaxFanSpeedLevel - configGpuUsage.MinFanSpeedLevel)) + configGpuUsage.MinFanSpeedLevel);
						fanSpeedLevelsOfUsage.Add((usage, clock), fanSpeedLevel);
					}
				}

				// 온도 비례
				for (int temperature = configGpuTemp.MinTemperature; temperature <= configGpuTemp.MaxTemperature; temperature++)
				{
					double temperaturePercentage = (double)(temperature - configGpuTemp.MinTemperature) / (configGpuTemp.MaxTemperature - configGpuTemp.MinTemperature);
					int fanSpeedLevel = (int)Math.Round((temperaturePercentage * (configGpuTemp.MaxFanSpeedLevel - configGpuTemp.MinFanSpeedLevel)) + configGpuTemp.MinFanSpeedLevel);
					fanSpeedLevelsOfTemp.Add(temperature, fanSpeedLevel);
				}
			}

			/*	사용량에 비례한 쿨러 팬 속도 계산
			 *		사용량, 클럭을 최저 최대 값 내로 보정
			 *		보정된 사용율(클럭율) = (보정된 사용량 - 최저 사용량) / (최대 사용량 - 최저 사용량)
			 *		팬 속도 = (사용율 * 클럭율 * (최대 속도 - 최저 속도)) + 최저 속도
			 */
			private static int ProportionalToUsage()
			{
				Config.Usage configGpu = Config.GetRoot.Gpu.Usage;

				int usage = gpu.UsageInformation.GPU.Percentage;
				Debug.WriteLine($"{nameof(usage)} = {usage}%");
				if (usage < configGpu.MinUsage) usage = configGpu.MinUsage;
				else if (usage > configGpu.MaxUsage) usage = configGpu.MaxUsage;

				int clock = (int)gpu.CurrentClockFrequencies.GraphicsClock.Frequency / 1000;
				Debug.WriteLine($"{nameof(clock)} = {clock}MHz");
				if (clock < configGpu.MinClockFrequencyMHz) clock = configGpu.MinClockFrequencyMHz;
				else if (clock > configGpu.MaxClockFrequencyMHz) clock = configGpu.MaxClockFrequencyMHz;

				//double usagePercentage = (double)(usage - configGpu.MinUsage) / (configGpu.MaxUsage - configGpu.MinUsage);
				//double clockPercentage = (double)(clock - configGpu.MinClockFrequencyMHz) / (configGpu.MaxClockFrequencyMHz - configGpu.MinClockFrequencyMHz);
				//int fanSpeedLevel = (int)Math.Round((usagePercentage * clockPercentage * (configGpu.MaxFanSpeedLevel - configGpu.MinFanSpeedLevel)) + configGpu.MinFanSpeedLevel);
				int fanSpeedLevel = fanSpeedLevelsOfUsage[(usage, clock)];
				Debug.WriteLine($"{nameof(fanSpeedLevel)} = {fanSpeedLevel}%\n");

				return fanSpeedLevel;
			}

			/*	온도에 비례한 쿨러 팬 속도 계산
			 *		온도를 최저 최대 값 내로 보정
			 *		보정된 온도율 = (보정된 온도 - 최저 온도) / (최대 온도 - 최저 온도)
			 *		팬 속도 = (온도율 * (최대 속도 - 최저 속도)) + 최저 속도
			 */
			private static int ProportionalToTemperature()
			{
				Config.Temperature configGpu = Config.GetRoot.Gpu.Temperature;

				int temperature = gpu.ThermalInformation.ThermalSensors.ToList()[0].CurrentTemperature;
				Debug.WriteLine($"{nameof(temperature)} = {temperature}°C");
				if (temperature < configGpu.MinTemperature) temperature = configGpu.MinTemperature;
				else if (temperature > configGpu.MaxTemperature) temperature = configGpu.MaxTemperature;

				//double temperaturePercentage = (double)(temperature - configGpu.MinTemperature) / (configGpu.MaxTemperature - configGpu.MinTemperature);
				//int fanSpeedLevel = (int)Math.Round((temperaturePercentage * (configGpu.MaxFanSpeedLevel - configGpu.MinFanSpeedLevel)) + configGpu.MinFanSpeedLevel);
				int fanSpeedLevel = fanSpeedLevelsOfTemp[temperature];
				Debug.WriteLine($"{nameof(fanSpeedLevel)} = {fanSpeedLevel}%");

				return fanSpeedLevel;
			}
		}
	}
}
