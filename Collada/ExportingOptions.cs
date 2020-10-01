using System;
using Autodesk.Revit.DB;

namespace ExportDAE
{
	internal class ExportingOptions
	{
		public string FilePath
		{
			get;
			set;
		}

		public View3D MainView3D
		{
			get;
			set;
		}

		public double SkipSmallerThan
		{
			get;
			set;
		}

		public int InsertionPoint
		{
			get;
			set;
		}

		public bool SkipInteriorDetails
		{
			get;
			set;
		}

		public bool CollectTextures
		{
			get;
			set;
		}

		public bool UnicodeSupport
		{
			get;
			set;
		}

		public int LevelOfDetail
		{
			get;
			set;
		}

		public bool GeometryOptimization
		{
			get;
			set;
		}
	}
}
