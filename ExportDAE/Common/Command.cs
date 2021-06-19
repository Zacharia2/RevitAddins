using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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
				return Result.Cancelled;
			}
			Document document = activeUIDocument.Document;
			if (document == null)
			{
				return Result.Cancelled;
			}
			if (document.ActiveView is View3D)//是否在三维视图下？
			{
				//创建面板，用于选择导出文件设置
				ExporterDialog exporterDialog = new ExporterDialog();
				exporterDialog.SaveFileDialog.FileName = document.Title;//设置文件名
				exporterDialog.SaveFileDialog.FileName = exporterDialog.SaveFileDialog.FileName.Replace(".rvt", ".dae");//替换文件后缀
																														//判断选择面板是否创建成功与导出的文件名非为空
				if (exporterDialog.ShowDialog() == DialogResult.OK && exporterDialog.SaveFileDialog.FileName != "")
				{
					ExportingOptions userSetting = new ExportingOptions();//创建选项设置

					//保存用户的设置
					userSetting.FilePath = exporterDialog.SaveFileDialog.FileName;//将设置的文件名赋予对象的路径变量中。
					userSetting.SkipSmallerThan = (double)exporterDialog.SkipSmallerThan.Value;//设置跳过小于某一精度的物体。
					userSetting.InsertionPoint = exporterDialog.InsertionPoint.SelectedIndex;//设置插入点
					userSetting.SkipInteriorDetails = exporterDialog.SkipInteriorDetails.Checked;//设置是否跳过较小的几何体
					userSetting.CollectTextures = exporterDialog.CollectTextures.Checked;//设置是否保护贴图
					userSetting.UnicodeSupport = exporterDialog.UnicodeSupport.Checked;//设置Unicode支持
					userSetting.GeometryOptimization = exporterDialog.GeometryOptimization.Checked;//设置是否优化几何体
					userSetting.MainView3D = (document.ActiveView as View3D);//as 判断类型是否兼容，是强制转换，不是返回null。
					userSetting.LevelOfDetail = exporterDialog.levelOfDetail.Value;// 设置导出模型精度等级。

					//导出DAE模型,  文档、3D视图、用户设置
					ExportView3D(document, document.ActiveView, userSetting);
				}
			}
			else
			{
				MessageBox.Show("请在三维视图中导出模型。");
			}
			return Result.Succeeded;
		}



		/// <summary>
		/// //导出DAE模型,  文档、3D视图、用户设置
		/// </summary>
		/// <param name="document">Revit活动视图的文档</param>
		/// <param name="view">视图</param>
		/// <param name="userSetting">用户设置</param>
		internal void ExportView3D(Document document, Autodesk.Revit.DB.View view3D, ExportingOptions userSetting)
		{
			//将文档和导出模型设置提交给 导出上下文对象。
			MExportContext myExportContext = new MExportContext(document, userSetting);
			//TestExportContext testExportContext = new TestExportContext(document);
			//将文档和导出上下文对象提交给 autodesk默认导出对象。
			CustomExporter customExporter = new CustomExporter(document, myExportContext)
			{
				IncludeGeometricObjects = false,//当通过导出上下文处理模型时，此标志将导出器设置为包括或排除几何对象（例如面和曲线）的输出。
				ShouldStopOnError = false   //如果在任何一种导出方法中发生错误，此标志将指示导出过程停止或继续。
			};

			//使用CustomExporter导出模型。
			customExporter.Export(view3D);

		}
	}
}