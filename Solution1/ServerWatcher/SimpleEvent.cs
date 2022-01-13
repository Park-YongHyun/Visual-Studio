using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerWatcher
{
	class SimpleEvent
	{
		public static event EventHandler Event1;

		public static void Event1Raise()
		{
			Event1?.Invoke(null, EventArgs.Empty);
		}
	}
}
