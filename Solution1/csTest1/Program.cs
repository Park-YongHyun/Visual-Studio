// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System.Globalization;

//Console.WriteLine("Hello, World!");

Debug.WriteLine("test start");

// 유효숫자 표기
{
	
	List<double> doubleList = new();
	doubleList.Add(0.012345);
	doubleList.Add(0.12345);
	doubleList.Add(1.2345);
	doubleList.Add(12.345);
	doubleList.Add(123.45);
	doubleList.Add(1234.5);
	doubleList.Add(12345);
	doubleList.Add(10);
	doubleList.Add(0);
	doubleList.Add(1);
	doubleList.Add(-1);
	doubleList.Add(0.1);

	Debug.WriteLine("");
	foreach (var item in doubleList)
	{
		Debug.WriteLine(decimal.Parse(item.ToString("E2"), NumberStyles.Float));

		// 0.0123
		// 0.123
		// 1.23
		// 12.3
		// 123
		// 1230
		// 12300
		// 10.0
		// 0.00
		// 1.00
		// -1.00
		// 0.100
	}

	Debug.WriteLine("");
	foreach (var item in doubleList)
	{
		Debug.WriteLine(decimal.Parse(item.ToString("E3"), NumberStyles.Float));

		// 0.01235
		// 0.1235
		// 1.234
		// 12.35
		// 123.5
		// 1234
		// 12340
		// 10.00
		// 0.000
		// 1.000
		// -1.000
		// 0.1000
	}
}


//using NvAPIWrapper;
//var t1 = NvAPIWrapper.GPU.PhysicalGPU.GetPhysicalGPUs();
//t1[0].CoolerInformation.SetCoolerSettings(0, NvAPIWrapper.Native.GPU.CoolerPolicy.Manual, 31);


Debug.WriteLine("test end");
