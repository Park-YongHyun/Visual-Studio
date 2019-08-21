using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace ConsoleApp1
{
	class Program
	{
		static void Main(string[] args)
		{
			c1 t1 = new c2();
			t1.a();
		}
	}

	public class c1
	{
		public virtual void a()
		{
			Console.Write("a");
		}
	}
	public class c2 : c1
	{
		public override void a()
		{
			Console.Write("b");
		}
	}
}
