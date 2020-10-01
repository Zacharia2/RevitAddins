using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Reflection;

namespace ExportDAE
{
	internal class App : IExternalApplication
	{
		private string AssemblyFullName
		{
			get
			{
				return Assembly.GetExecutingAssembly().Location;
			}
		}

		public Result OnStartup(UIControlledApplication a)
		{
			RibbonPanel panel = a.CreateRibbonPanel("CALLADA");
			this.AddPushButton(panel);
			return 0;
		}

		private void AddPushButton(RibbonPanel panel)
		{
			
		}

		public Result OnShutdown(UIControlledApplication a)
		{
			return 0;
		}
	}
}
