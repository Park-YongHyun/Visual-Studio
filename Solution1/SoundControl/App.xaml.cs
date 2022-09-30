using SoundControl.Configuration;
using SoundControl.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace SoundControl
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private readonly SingleInstanceApp singleInstanceApp = new();

		public readonly View.VolumePopup volumePopup = new();

		protected override void OnStartup(StartupEventArgs e)
		{
			Debug.WriteLine($"{nameof(App)} OnStartup");

			singleInstanceApp.CheckAndShutdown(this);

			using (Process process = Process.GetCurrentProcess())
			{
				ConfigData.Process configProcess = Config.GetData.Process;
				process.PriorityClass = (ProcessPriorityClass)Enum.Parse(typeof(ProcessPriorityClass), configProcess.Priority);
				process.ProcessorAffinity = (IntPtr)configProcess.ProcessorAffinity;
			}

			base.OnStartup(e);
		}
	}
}
