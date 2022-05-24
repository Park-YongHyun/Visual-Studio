using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GpuCoolerControl
{
	// v1.0.0.20220522.0
	class SingleInstanceApp
	{
		private static Mutex mutex;

		public static void CheckAndShutdown(System.Windows.Application app)
		{
			string appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
			bool createdNew;

			mutex = new(true, appName, out createdNew);

			if (!createdNew)
			{
				Debug.WriteLine("Shutdown");

				app.Shutdown();
			}
		}
	}

	/* 사용 예시
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			Debug.WriteLine("OnStartup");

			SingleInstanceApp.CheckAndShutdown(this);

			base.OnStartup(e);
		}
	}
	*/
}
