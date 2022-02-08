using System.Windows;

[assembly: ThemeInfo(
	ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
									 //(used if a resource is not found in the page,
									 // or application resource dictionaries)
	ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
											  //(used if a resource is not found in the page,
											  // app, or any theme specific resource dictionaries)
)]

#if DEBUG
// 디버그용 xaml 조건부 컴파일러 지시문
[assembly: System.Windows.Markup.XmlnsDefinition( "debug-mode", "Namespace" )]
#endif
