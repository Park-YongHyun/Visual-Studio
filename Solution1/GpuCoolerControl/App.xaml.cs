using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GpuCoolerControl
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			Debug.WriteLine("OnStartup");

			SingleInstanceApp.CheckAndShutdown(this);

			using (Process process = Process.GetCurrentProcess())
			{
				Config.Process configProcess = Config.GetRoot.Process;
				process.PriorityClass = (ProcessPriorityClass)Enum.Parse(typeof(ProcessPriorityClass), configProcess.Priority);
				process.ProcessorAffinity = (IntPtr)configProcess.ProcessorAffinity;
			}

			base.OnStartup(e);
		}
	}
}
