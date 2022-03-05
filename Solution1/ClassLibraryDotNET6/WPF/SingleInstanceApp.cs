using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace 
{
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
}
