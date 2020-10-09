using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Reflection;
using Collada141;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;
using System.Drawing.Imaging;

namespace ExportDAE
{
	//声明一个类，继承自RevitAPI的IExternalApplication（外部程序）接口。
	//IExternalApplication用两个成员函数OnStartup和OnShutdown来对应启动和关闭状态。
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
			RibbonPanel panel = a.CreateRibbonPanel("RevitAddins");
			this.AddPushButton(panel);
			return Result.Succeeded;
		}


		private static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
		{
			using (MemoryStream stream = new MemoryStream())
			{
				bitmap.Save(stream, ImageFormat.Png); // 坑点：格式选Bmp时，不带透明度

				stream.Position = 0;
				BitmapImage result = new BitmapImage();
				result.BeginInit();
				// According to MSDN, "The default OnDemand cache option retains access to the stream until the image is needed."
				// Force the bitmap to load right now so we can dispose the stream.
				result.CacheOption = BitmapCacheOption.OnLoad;
				result.StreamSource = stream;
				result.EndInit();
				result.Freeze();
				return result;
			}
		}

			
		private void AddPushButton(RibbonPanel panel)
		{
			/*实例化对象PushButtonData，并通过构造函数新建一个按钮数据。该构造函数有四个参数：
			 * 第一个是程序内部使用的按钮名称，这个只要注意不要重名就可以；
			 * 第二个是用户可见的按钮文字，这个可以自己随便写；
			 * 第三个是按钮对应的类库地址，即dll的路径，要按实际的填；
			 * 第四个是按钮命令实现代码的类名，这里要和28行对应。
			 * 其中第三个参数前面的@用来忽略后面的转义字符“\”，这是个很实用的小技巧。*/
			PushButtonData buttonData = new PushButtonData("导出DAE", "ExportDAE",this.AssemblyFullName,"ExportDAE.Command");
			PushButton pushButton = (PushButton)panel.AddItem(buttonData);
			pushButton.ToolTip = "导出为COLLADA（.dae）文件";

			//设置按钮图标

			Bitmap bitmap = ExportDAE.Properties.Resource1.blender_high;
			pushButton.LargeImage = BitmapToBitmapImage(bitmap); 
		}

		
		public Result OnShutdown(UIControlledApplication a)
		{
			return Result.Succeeded;
		}
	}
}
