using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Visual;
using System;
using System.Windows.Forms;

namespace ExportDAE
{
	[Journaling(default), Regeneration(default), Transaction(default)]
	public class Command : IExternalCommand
	{
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			UIApplication application = commandData.Application;
			UIDocument activeUIDocument = application.ActiveUIDocument;
			if (activeUIDocument == null)
			{
				return (Result)1;
			}
			Document document = activeUIDocument.Document;
			if (document == null)
			{
				return (Result)1;//不同返回值具有不同结果，1为取消。
			}
			if (document.ActiveView is View3D)//is 用来判断是否兼容
			{
				//创建面板，用于选择导出文件设置
				ExporterDialog exporterDialog = new ExporterDialog();
				exporterDialog.SaveFileDialog.FileName = document.Title;//设置文件名
				exporterDialog.SaveFileDialog.FileName = exporterDialog.SaveFileDialog.FileName.Replace(".rvt", ".dae");//替换文件后缀
				//判断选择面板是否创建成功与导出的文件名非为空
				if (exporterDialog.ShowDialog() == DialogResult.OK && exporterDialog.SaveFileDialog.FileName != "")
				{
					ExportingOptions exportingOptions = new ExportingOptions();//创建选项设置
					exportingOptions.FilePath = exporterDialog.SaveFileDialog.FileName;//将设置的文件名赋予对象的路径变量中。
					exportingOptions.SkipSmallerThan = (double)exporterDialog.SkipSmallerThan.Value;//设置跳过小于某一精度的物体。
					exportingOptions.InsertionPoint = exporterDialog.InsertionPoint.SelectedIndex;//设置插入点
					exportingOptions.SkipInteriorDetails = exporterDialog.SkipInteriorDetails.Checked;//
					exportingOptions.CollectTextures = exporterDialog.CollectTextures.Checked;
					exportingOptions.UnicodeSupport = exporterDialog.UnicodeSupport.Checked;
					exportingOptions.GeometryOptimization = exporterDialog.GeometryOptimization.Checked;
					exportingOptions.MainView3D = (document.ActiveView as View3D);//as 判断类型是否兼容，是强制转换，不是返回null。
					exportingOptions.LevelOfDetail = exporterDialog.levelOfDetail.Value;




					//导出DAE模型,提交函数条件  文档、3D视图、导出选线
					this.ExportView3D(document, document.ActiveView as View3D, exportingOptions);
				}
			}
			else
			{
				MessageBox.Show("请在三维视图中导出模型。");
			}
			return 0;
		}

		internal void ExportView3D(Document document, View3D view3D, ExportingOptions exportingOptions)
		{
			//将文档和导出模型设置提交给 导出上下文对象。
			MyExportContext myExportContext = new MyExportContext(document, exportingOptions);
			//将文档和导出上下文对象提交给 autodesk默认导出对象。
			CustomExporter customExporter = new CustomExporter(document, myExportContext);
			customExporter.IncludeGeometricObjects = false;
			customExporter.ShouldStopOnError = false;
			//使用CustomExporter导出模型。
			customExporter.Export(view3D);
		}

		private string GetAssetDescription(Asset asset)
		{
			string text = "";
			for (int i = 0; i < asset.Size; i++)
			{
				AssetProperty assetProperty = asset.Get(i);
				text += this.GetPropertyDescription(assetProperty);
			}
			return text;
		}


		//获取属性说明
		private string GetPropertyDescription(AssetProperty assetProperty)
		{
			//text = assetProperty.Name + ": " + assetProperty.Type.ToString() + "  " + 属性值 + "CONNECTED START\n" +      多个assetProperty + "CONNECTED END\n"
			string text = assetProperty.Name + ": " + assetProperty.Type.ToString() + "  ";

			//数值代表变量类型。获取属性值并加入字符串末尾。
			switch (assetProperty.Type)
			{
				case (AssetPropertyType)2:
					text += (assetProperty as AssetPropertyBoolean).Value.ToString();
					break;
				case (AssetPropertyType)4:
					text += (assetProperty as AssetPropertyInteger).Value.ToString();
					break;
				case (AssetPropertyType)5:
					text += (assetProperty as AssetPropertyFloat).Value.ToString();
					break;
				case (AssetPropertyType)6:
					text += (assetProperty as AssetPropertyDouble).Value.ToString();
					break;
				case (AssetPropertyType)7:
					text += (assetProperty as AssetPropertyDoubleArray2d).Value.ToString();
					break;
				case (AssetPropertyType)8:
					text += (assetProperty as AssetPropertyDoubleArray3d).GetValueAsDoubles().ToString();
					break;
				case (AssetPropertyType)9:
					text += (assetProperty as AssetPropertyDoubleArray4d).GetValueAsDoubles().ToString();
					break;
				case (AssetPropertyType)10:
					text += (assetProperty as AssetPropertyDoubleMatrix44).Value.ToString();
					break;
				case (AssetPropertyType)11:
					text += (assetProperty as AssetPropertyString).Value.ToString();
					break;
				case (AssetPropertyType)14:
					text += (assetProperty as AssetPropertyDistance).Value.ToString();
					break;
				case (AssetPropertyType)15:
					text += this.GetAssetDescription(assetProperty as Asset);
					break;
				case (AssetPropertyType)17:
					text += (assetProperty as AssetPropertyInt64).Value.ToString();
					break;
				case (AssetPropertyType)18:
					text += (assetProperty as AssetPropertyUInt64).Value.ToString();
					break;
				case (AssetPropertyType)20:
					text += (assetProperty as AssetPropertyFloatArray).GetValue().ToString();
					break;
			}
			//添加换行。
			text += "\n";

			//当前连接的属性数。
			if (assetProperty.NumberOfConnectedProperties > 0)
			{
				text += "CONNECTED START\n";
			}
			for (int i = 0; i < assetProperty.NumberOfConnectedProperties; i++)
			{
				text = text + "       " + this.GetPropertyDescription(assetProperty.GetConnectedProperty(i));//根据索引值返回assetProperty，并加入到字符串中。
			}
			if (assetProperty.NumberOfConnectedProperties > 0)
			{
				text += "CONNECTED END\n";
			}
			return text;
		}
		
		
		
		
		
		//在资源中查找纹理路径
		private string FindTexturePathInAsset(Asset asset)
		{
			for (int i = 0; i < asset.Size; i++)
			{
				AssetProperty assetProperty = asset.Get(i);//获取给定索引处的属性。
				string text = this.FindTexturePathInAssetProperty(assetProperty);
				if (text.Length > 0)
				{
					return text;
				}
			}
			return "";
		}

		private string FindTexturePathInAssetProperty(AssetProperty assetProperty)
		{
			AssetPropertyType type = assetProperty.Type;
			if (!type.Equals(11)) //String = 11,非字符串类型，判定为Asset类型。
			{
				if (type.Equals(15))//Asset = 15
				{
					string text = this.FindTexturePathInAsset(assetProperty as Asset);
					if (text.Length > 0)
					{
						return text;
					}
				}
			}
			else if (assetProperty.Name == "unifiedbitmap_Bitmap")
			{
				return (assetProperty as AssetPropertyString).Value.ToString();
			}
			for (int i = 0; i < assetProperty.NumberOfConnectedProperties; i++)
			{
				string text2 = this.FindTexturePathInAssetProperty(assetProperty.GetConnectedProperty(i));
				if (text2.Length > 0)
				{
					return text2;
				}
			}
			return "";
		}
	}
}
