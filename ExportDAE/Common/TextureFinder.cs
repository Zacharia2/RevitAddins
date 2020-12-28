using Autodesk.Revit.DB;
using Microsoft.Win32;
using Autodesk.Revit.DB.Visual;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Linq;
using System.Security;

namespace ExportDAE
{
	internal class TextureFinder    //纹理查找器
	{
		#region 内部类

		[CompilerGenerated]
		[Serializable]
		private sealed class Inner
		{
			public static readonly Inner tools = new Inner();
			public static Func<ModelMaterial, bool> mIsValidTexturePath;
			public static Func<ModelMaterial, string> TexturePath1;
			public static Func<ModelMaterial, string> TexturePath2;
			public static Func<char, bool> IsChar1;
			public static Func<char, bool> IsChar2;


			/// <summary>
			/// IsValidTexturePath 判断贴图路径是否为空。
			/// </summary>
			internal bool IsValidTexturePath(ModelMaterial m)
			{
				return m.TexturePath != string.Empty;

			}

			/// <summary>
			/// GetTexturePath2 获取贴图路径
			/// </summary>
			internal string GetTexturePath1(ModelMaterial o) //GetTexturePath1
			{
				return o.TexturePath;
			}

			/// <summary>
			/// GetTexturePath2 获取贴图路径
			/// </summary>
			internal string GetTexturePath2(ModelMaterial o)
			{
				return o.TexturePath;
			}

			/// <summary>
			/// IsSADM	 指示指定的 Unicode 字符是否属于空格类别、字母或十进制数字类别、标点符号类别。中的一种。
			/// </summary>
			internal bool IsSADM(char c)
			{
				return char.IsWhiteSpace(c) || char.IsLetterOrDigit(c) || char.IsPunctuation(c);
			}

			/// <summary>
			/// Is2	 指示指定的 Unicode 字符是否属于空格类别、字母或十进制数字类别、'.'、'-'之中的一种。
			/// </summary>
			internal bool IsSADS(char c)
			{
				return char.IsWhiteSpace(c) || char.IsLetterOrDigit(c) || c == '.' || c == '-';
			}

		}

		#endregion

		private Document document;
		private ICollection<string> textureFolders;
		private static Encoding usAsciiEncoder = Encoding.GetEncoding("us-ascii", new EncoderReplacementFallback(string.Empty), new DecoderExceptionFallback());
		private static Encoding Utf16Encoder = Encoding.GetEncoding("unicode", new EncoderReplacementFallback(string.Empty), new DecoderExceptionFallback());
		

		public void Init(Document document)
		{
			this.document = document;
			CollectTexturesFolders();
		}


		public void Clear()
		{
			textureFolders.Clear();
		}


		/// <summary>
		/// 收集材质纹理文件夹
		/// </summary>
		private void CollectTexturesFolders()
		{
			// 收集纹理文件夹
			textureFolders = new List<string>(); //创建List文件列表
			try
			{
				// 查找文档磁盘文件的标准路径。
				string directoryName = Path.GetDirectoryName(document.PathName);
				// 判断是否为空
				if (!textureFolders.Contains(directoryName))
				{
					// 不为空则加入到纹理文件夹列表中
					textureFolders.Add(directoryName);
				}
			}
			catch (Exception)
			{
			}

			// 通过注册表值查找指定文件夹。


			//打开一个注册表项
			RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
			//打开注册表值
			registryKey = registryKey.OpenSubKey("SOFTWARE\\Wow6432Node\\Autodesk\\ADSKAssetLibrary");
			
			/*
			 Windows Registry Editor Version 5.00

			[HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Autodesk\ADSKAssetLibrary]

			[HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Autodesk\ADSKAssetLibrary\2020]
			"LibraryPaths"="C:\\Program Files (x86)\\Common Files\\Autodesk Shared\\Materials\\2020\\assetlibrary_base.adsklib"
			"ProductLine"="PLC0000013"
			"Master"="{DC5C74DA-C2B8-4D80-BAB0-FA8006D1E4C5}"
			"Release"="2020"
			"Build"="2018.11.1.0"
			 
			 */

			if (registryKey == null)
			{
				return;//return; 直接作为一条语句表示当前函数（也可以叫做方法）结束
			}


			string[] subKeyNames = registryKey.GetSubKeyNames();
			for (int i = 0; i < registryKey.SubKeyCount; i++)
			{
				RegistryKey subKey = registryKey.OpenSubKey(subKeyNames[i]);
				string text = (string)subKey.GetValue("LibraryPaths");
				if (text != null)
				{
					string item = Path.GetDirectoryName(text) + "\\assetlibrary_base.fbm";
					if (!textureFolders.Contains(item))
					{

						//搜索结果为文件夹C:\Program Files (x86)\Common Files\Autodesk Shared\Materials\2020\assetlibrary_base.fbm
						textureFolders.Add(item); //加入到纹理文件夹列表中
					}
				}
				subKey.Close();
			}
			registryKey.Close();



			registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
			registryKey = registryKey.OpenSubKey("SOFTWARE\\Wow6432Node\\Autodesk\\ADSKTextureLibrary");

			/*
			 Windows Registry Editor Version 5.00

			[HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Autodesk\ADSKTextureLibraryNew]

			[HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Autodesk\ADSKTextureLibraryNew\1]

			[HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Autodesk\ADSKTextureLibraryNew\1\2020]
			"LibraryPaths"="C:\\Program Files (x86)\\Common Files\\Autodesk Shared\\Materials\\Textures\\"
			"ProductLine"="PLC0000014"
			"Master"="{9D7FF3B8-4613-4295-8026-638F43CF8E99}"
			"Release"="2020"
			"Build"="2018.11.1.0"

			[HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Autodesk\ADSKTextureLibraryNew\2]

			[HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Autodesk\ADSKTextureLibraryNew\2\2020]
			"LibraryPaths"="C:\\Program Files (x86)\\Common Files\\Autodesk Shared\\Materials\\Textures\\"
			"ProductLine"="PLC0000015"
			"Master"="{E9FF3018-5696-4B85-810F-3C3AC844B033}"
			"Release"="2020"
			"Build"="2018.11.1.0"

			[HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Autodesk\ADSKTextureLibraryNew\3]

			[HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Autodesk\ADSKTextureLibraryNew\3\2020]
			"LibraryPaths"="C:\\Program Files (x86)\\Common Files\\Autodesk Shared\\Materials\\Textures\\"
			"ProductLine"="PLC0000016"
			"Master"="{6E3D6503-EF40-4FB2-884F-ABB68CCA318A}"
			"Release"="2020"
			"Build"="2018.11.1.0"

			 */
			if (registryKey == null)
			{
				return;
			}
			string[] subKeyNames2 = registryKey.GetSubKeyNames();
			for (int j = 0; j < registryKey.SubKeyCount; j++)
			{
				RegistryKey subKey2 = registryKey.OpenSubKey(subKeyNames2[j]);
				string text2 = (string)subKey2.GetValue("LibraryPaths");
				if (text2 != null)
				{
					string item2 = Path.GetDirectoryName(text2) + "\\" + subKeyNames2[j] + "\\Mats";
					if (!textureFolders.Contains(item2))
					{
						//查找结果为贴图所在的文件夹C:\Program Files (x86)\Common Files\Autodesk Shared\Materials\Textures\2（item2）\Mats
						textureFolders.Add(item2);
					}
				}
				subKey2.Close();
			}
			registryKey.Close();
		}


		/// <summary>
		/// 从Asset资源中查找漫反射纹理路径
		/// </summary>
		/// <param name="exportedMaterial">被导出的材质</param>
		/// <param name="asset">Asset资源</param>
		public void FindDiffuseTexturePathFromAsset(ModelMaterial exportedMaterial, Asset asset)
		{
			//判读asset是否为空
			if (asset == null)
			{
				//为空结束函数。
				return;
			}
			try
			{
				//不为空 创建新的变量asset2，并调用本类中的查找贴图资源，条件为asset变量。
				Asset assetTexture = FindTextureAsset(asset);
				//判断 assetTexture 是否为空
				if (assetTexture != null)
				{
					//非空则查找资源中的"unifiedbitmap_Bitmap"文件并修复文件路径为绝对路径。
					exportedMaterial.TexturePath = (assetTexture.FindByName("unifiedbitmap_Bitmap") as AssetPropertyString).Value;
					exportedMaterial.TexturePath = FixTexturePath(exportedMaterial.TexturePath);
					//定义资源属性距离
					AssetPropertyDistance assetPropertyDistance;
					//判断资产中是否包含“texture_RealWorldScaleX”
					if (assetTexture.FindByName("texture_RealWorldScaleX") != null)
					{
						//"包含texture_RealWorldScaleX"则将值赋值给assetPropertyDistance
						assetPropertyDistance = (assetTexture.FindByName("texture_RealWorldScaleX") as AssetPropertyDistance);
					}
					else
					{
						//否则 "unifiedbitmap_RealWorldScaleX  真实世界尺度" 将未定义赋值给assetPropertyDistance
						assetPropertyDistance = (assetTexture.FindByName("unifiedbitmap_RealWorldScaleX") as AssetPropertyDistance);
					}
					//判断距离属性非空，转换后的值（资源距离属性附加单位距离）除以1 得到小数，赋值给纹理U比例尺
					if (assetPropertyDistance != null)
					{
						exportedMaterial.TextureScaleU = 1.0 / UnitUtils.ConvertToInternalUnits(assetPropertyDistance.Value, assetPropertyDistance.DisplayUnitType);
					}

					//创建新的资源距离属性2重复上述步骤，将纹理缩放赋值给exportedMaterial.TextureScaleV
					AssetPropertyDistance assetPropertyDistance2;
					if (assetTexture.FindByName("texture_RealWorldScaleY") != null)
					{
						assetPropertyDistance2 = (assetTexture.FindByName("texture_RealWorldScaleY") as AssetPropertyDistance);
					}
					else
					{
						assetPropertyDistance2 = (assetTexture.FindByName("unifiedbitmap_RealWorldScaleY") as AssetPropertyDistance);
					}
					if (assetPropertyDistance2 != null)
					{
						exportedMaterial.TextureScaleV = 1.0 / UnitUtils.ConvertToInternalUnits(assetPropertyDistance2.Value, assetPropertyDistance.DisplayUnitType);
					}
					//判断是否存在texture_RealWorldOffsetX 存在  创建assetPropertyDistance3，不存在默认使用未定义。
					//将纹理贴图偏移量加上单位赋值给exportedMaterial.TextureOffsetU
					AssetPropertyDistance assetPropertyDistance3;
					if (assetTexture.FindByName("texture_RealWorldOffsetX") != null)
					{
						assetPropertyDistance3 = (assetTexture.FindByName("texture_RealWorldOffsetX") as AssetPropertyDistance);
					}
					else
					{
						assetPropertyDistance3 = (assetTexture.FindByName("unifiedbitmap_RealWorldOffsetX") as AssetPropertyDistance);
					}
					if (assetPropertyDistance3 != null)
					{
						exportedMaterial.TextureOffsetU = UnitUtils.ConvertToInternalUnits(assetPropertyDistance3.Value, assetPropertyDistance3.DisplayUnitType);
					}
					AssetPropertyDistance assetPropertyDistance4;
					if (assetTexture.FindByName("texture_RealWorldOffsetY") != null)
					{
						assetPropertyDistance4 = (assetTexture.FindByName("texture_RealWorldOffsetY") as AssetPropertyDistance);
					}
					else
					{
						assetPropertyDistance4 = (assetTexture.FindByName("unifiedbitmap_RealWorldOffsetY") as AssetPropertyDistance);
					}
					if (assetPropertyDistance4 != null)
					{
						exportedMaterial.TextureOffsetV = UnitUtils.ConvertToInternalUnits(assetPropertyDistance4.Value, assetPropertyDistance4.DisplayUnitType);
					}
					//查找texture_WAngle 并赋值给assetPropertyDoubl不存在则使用默认未定义。之后赋值给exportedMaterial.TextureRotationAngle加上某个数值。
					AssetPropertyDouble assetPropertyDouble;
					if (assetTexture.FindByName("texture_WAngle") != null)
					{
						assetPropertyDouble = (assetTexture.FindByName("texture_WAngle") as AssetPropertyDouble);
					}
					else
					{
						assetPropertyDouble = (assetTexture.FindByName("unifiedbitmap_WAngle") as AssetPropertyDouble);
					}
					if (assetPropertyDouble != null)
					{
						exportedMaterial.TextureRotationAngle = assetPropertyDouble.Value;
						exportedMaterial.TextureRotationAngle *= 0.017453292519943295;
					}
				}
			}
			catch (Exception)
			{
			}
		}


		private Asset FindTextureAsset(AssetProperty assetProperty)
		{
			//查找纹理贴图资源
			//获取资源属性的数据类型。
			AssetPropertyType type = assetProperty.Type;
			if (type.Equals(AssetPropertyType.Asset))
			{
				try
				{
					Asset asset = assetProperty as Asset;
					if (IsTextureAsset(asset))
					{
						Asset result = asset;
						return result;
					}
					for (int i = 0; i < asset.Size; i++)
					{
						Asset asset2 = FindTextureAsset(asset.Get(i));
						if (asset2 != null)
						{
							Asset result = asset2;
							return result;
						}
					}
				}
				catch (Exception)
				{
				}
			}
			if (assetProperty.Name.IndexOf("bump", StringComparison.OrdinalIgnoreCase) < 0 && assetProperty.Name.IndexOf("opacity", StringComparison.OrdinalIgnoreCase) < 0 && assetProperty.Name.IndexOf("shader", StringComparison.OrdinalIgnoreCase) < 0 && assetProperty.Name.IndexOf("pattern", StringComparison.OrdinalIgnoreCase) < 0 && assetProperty.Name.IndexOf("_map", StringComparison.OrdinalIgnoreCase) < 0 && assetProperty.Name.IndexOf("transparency", StringComparison.OrdinalIgnoreCase) < 0)
			{
				for (int j = 0; j < assetProperty.NumberOfConnectedProperties; j++)
				{
					Asset asset3 = FindTextureAsset(assetProperty.GetConnectedProperty(j));
					if (asset3 != null)
					{
						return asset3;
					}
				}
			}
			return null;
		}


		private bool IsTextureAsset(Asset asset)
		{
			AssetProperty assetProprty = GetAssetProprty(asset, "assettype");
			return (assetProprty != null && (assetProprty as AssetPropertyString).Value == "texture") || this.GetAssetProprty(asset, "unifiedbitmap_Bitmap") != null;
		}


		private AssetProperty GetAssetProprty(Asset asset, string propertyName)
		{
			for (int i = 0; i < asset.Size; i++)
			{
				if (asset.Get(i).Name == propertyName)
				{
					return asset.Get(i);
				}
			}
			return null;
		}


		/// <summary>
		/// 修复文件路径——转化为文件绝对路径。
		/// </summary>
		/// <param name="inputPath">输入要处理的路径</param>
		/// <returns></returns>
		private string FixTexturePath(string inputPath)
			
		{
			//判断输入路径是否为空为空返回“”。
			if (inputPath.Length == 0)
			{
				return "";
			}
			//不为空将输入路径赋值给text变量
			string text = inputPath;
			//检索字符"|"所在字符串的引索，并判读是否存在（大于等于0）
			if (text.IndexOf('|') >= 0)
			{
				//存在 赋值给 temp变量
				string temp = text;
				//删除"|"字符。
				text = temp.Remove(temp.IndexOf('|'));
			}
			//判读指定文件text是否存在。
			if (File.Exists(text))
			{
				//存在提交text
				return text;
			}
			//判断指定路径text是否包含根。
			if (!Path.IsPathRooted(text))
			{
				//不包含根路径将 获取文件绝对路径赋值给text变量。
				text = Path.GetFileName(text);
			}
			//判读指定文件text是否存在。
			if (!File.Exists(text))
			{
				//不存在则查找贴图并将找到的结果赋值给text变量
				text = FindTextureInTextureFolders(text);
			}
			//提交text
			return text;
		}


		private string FindTextureInTextureFolders(string file)
		{
			//从贴图文件夹中查找贴图
			//迭代目录，查找指定文件，如果存在就返回。不存在返回空。
			foreach (string current in this.textureFolders)
			{
				if (File.Exists(current + "\\" + file))
				{
					return current + "\\" + file;
				}
			}
			return "";
		}


		public string FindDiffuseTextureDecal(Element element)
		{
			//查找漫反射纹理贴图
			try
			{
				// 获取元素ID
				ElementId typeId = element.GetTypeId();
				//获取外部文件参考
				ExternalFileReference externalFileReference = (document.GetElement(typeId) as ElementType).GetExternalFileReference();
				//判断外部文件参考类型是不是
				if (externalFileReference.ExternalFileReferenceType.Equals(AssetPropertyType.Float))
				{
					//是的话将模型路径转换为用户可见路径，（链接模型的完整路径作为参数）
					string inputPath = ModelPathUtils.ConvertModelPathToUserVisiblePath(externalFileReference.GetAbsolutePath());
					//最后返回文件绝对路径。
					return FixTexturePath(inputPath);
				}
			}
			catch (Exception)
			{
			}
			return "";
		}




		//以下参考Command类。
		private string GetAssetDescription(Asset asset)
		{
			//获取资产说明

			//定义初始化字符串
			string str = "";
			//如果当前连接的属性数是否大于零。那么加入字符串“Asset CONNECTED START\n”。
			if (asset.NumberOfConnectedProperties > 0)
			{
				str += "Asset CONNECTED START\n";
			}
			//
			for (int i = 0; i < asset.NumberOfConnectedProperties; i++)
			{
				str = str + "       " + GetPropertyDescription(asset.GetConnectedProperty(i));
			}
			if (asset.NumberOfConnectedProperties > 0)
			{
				str += "Asset CONNECTED END\n";
			}
			str += "Asset prop START\n";
			for (int j = 0; j < asset.Size; j++)
			{
				AssetProperty assetProperty = asset.Get(j);
				str += GetPropertyDescription(assetProperty);
			}
			return str + "Asset prop EEND\n";
		}

		/// <summary>
		/// 获取属性说明
		/// text = assetProperty.Name + ": " + assetProperty.Type.ToString() + "  " + 属性值 + "CONNECTED START\n" +      多个assetProperty + "CONNECTED END\n"
		/// </summary>
		/// <param name="assetProperty"></param>
		/// <returns></returns>
		public string GetPropertyDescription(AssetProperty assetProperty)
		{
			string text = assetProperty.Name + ": " + assetProperty.Type.ToString() + "  ";

			//数值代表变量类型。获取属性值并加入字符串末尾。
			switch (assetProperty.Type)
			{
				case AssetPropertyType.Boolean:
					text += (assetProperty as AssetPropertyBoolean).Value.ToString();
					break;
				case AssetPropertyType.Enumeration:
					text += (assetProperty as AssetPropertyEnum).Value.ToString();
					break;
				case AssetPropertyType.Integer:
					text += (assetProperty as AssetPropertyInteger).Value.ToString();
					break;
				case AssetPropertyType.Float:
					text += (assetProperty as AssetPropertyFloat).Value.ToString();
					break;
				case AssetPropertyType.Double1:
					text += (assetProperty as AssetPropertyDouble).Value.ToString();
					break;
				case AssetPropertyType.Double2:
					text += (assetProperty as AssetPropertyDoubleArray2d).Value.ToString();
					break;
				case AssetPropertyType.Double3:
					text += (assetProperty as AssetPropertyDoubleArray3d).GetValueAsDoubles().ToString();
					break;
				case AssetPropertyType.Double4:
					text += (assetProperty as AssetPropertyDoubleArray4d).GetValueAsDoubles().ToString();
					break;
				case AssetPropertyType.Double44:
					text += (assetProperty as AssetPropertyDoubleMatrix44).Value.ToString();
					break;
				case AssetPropertyType.String:
					text += (assetProperty as AssetPropertyString).Value.ToString();
					break;
				case AssetPropertyType.Distance:
					text += (assetProperty as AssetPropertyDistance).Value.ToString();
					break;
				case AssetPropertyType.Asset:
					text = text + "\n" + GetAssetDescription(assetProperty as Asset);
					break;
				case AssetPropertyType.Longlong:
					text += (assetProperty as AssetPropertyInt64).Value.ToString();
					break;
				case AssetPropertyType.ULonglong:
					text += (assetProperty as AssetPropertyUInt64).Value.ToString();
					break;
				case AssetPropertyType.Float3:
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
				text = text + "       " + GetPropertyDescription(assetProperty.GetConnectedProperty(i));
			}
			if (assetProperty.NumberOfConnectedProperties > 0)
			{
				text += "CONNECTED END\n";
			}
			return text;
		}





		//未使用的方法
		//在资源中查找纹理路径
		private string FindTexturePathInAsset(Asset asset)
		{
			for (int i = 0; i < asset.Size; i++)
			{
				AssetProperty assetProperty = asset.Get(i);//获取给定索引处的属性。
				string text = FindTexturePathInAssetProperty(assetProperty);
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
			if (type != AssetPropertyType.String) //String = 11,非字符串类型，判定为Asset类型。
			{
				if (type == AssetPropertyType.Asset)//Asset = 15
				{
					string text = FindTexturePathInAsset(assetProperty as Asset);
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
				string text2 = FindTexturePathInAssetProperty(assetProperty.GetConnectedProperty(i));
				if (text2.Length > 0)
				{
					return text2;
				}
			}
			return "";
		}






		internal void MakeTexturePathsRelative(Dictionary<Tuple<Document, ElementId>, ModelMaterial> documentAndMaterialIdToExportedMaterial, ExportingOptions options)
		{
			IEnumerable<ModelMaterial> expr_MaterialSet3 = documentAndMaterialIdToExportedMaterial.Values;
			Func<ModelMaterial, bool> IsValidTexturePath;
			if ((IsValidTexturePath = Inner.mIsValidTexturePath) == null)
			{
				IsValidTexturePath = (Inner.mIsValidTexturePath = new Func<ModelMaterial, bool>(Inner.tools.IsValidTexturePath));
			}


			foreach (ModelMaterial current in expr_MaterialSet3.Where(IsValidTexturePath))
			{
				current.TexturePath = "textures\\" + this.CleanPath(Path.GetFileName(current.TexturePath), options);
			}
		}


		/// <summary>
		/// 通过构建委托函数获得ExportedMaterial中的贴图路径做迭代器的选择函数判断路径非空。
		/// 并定义导出材质的文件夹并将贴图放进这个文件夹。
		/// </summary>
		/// <param name="documentAndMaterialIdToExportedMaterial">欲导出材质的revit文档及材质ID</param>
		internal void CollectTextures(Dictionary<Tuple<Document, ElementId>, ModelMaterial> documentAndMaterialIdToExportedMaterial, ExportingOptions options)
		{
			//将导出材质的文档和材质ID赋值给迭代器。
			IEnumerable<ModelMaterial> expr_MaterialSet = documentAndMaterialIdToExportedMaterial.Values;
			//定义一个委托 in 为 ExportedMaterial, out为 string。
			Func<ModelMaterial, string> TexturePath1;
			//这里是委托函数，若内部类定义的Func为空，就new 一个func并将内部类中getGetTexturePath1方法作为参数传递给func形成委托函数调用，之后赋值给
			//内部类中的变量和此方法内部变量。
			if ((TexturePath1 = Inner.TexturePath1) == null)
			{
				//把委托作为参数
				TexturePath1 = Inner.TexturePath1 = new Func<ModelMaterial, string>(Inner.tools.GetTexturePath1);
			}
			//检查元素是否为空。
			//使用Ienumerable.select() 吧定义好从导出材质获取贴图路径的委托方法作为参数传递。并排序，获取元素个数，判读是否为空。若为空，结束本函数。
			if (expr_MaterialSet.Select(TexturePath1).Distinct<string>().Count<string>() == 0)
			{
				return;
			}


			//定义导出材质贴图路径的文件夹并保存在text变量中。
			string text = Path.GetDirectoryName(options.FilePath) + "\\textures";
			try
			{
				//尝试创建材质贴图文件夹textures。
				Directory.CreateDirectory(text);
			}
			catch (Exception)
			{
			}



			//引用委托函数。
			IEnumerable<ModelMaterial> expr_MaterialSet2 = documentAndMaterialIdToExportedMaterial.Values;
			Func<ModelMaterial, string> TexturePath2;
			if ((TexturePath2 = Inner.TexturePath2) == null)
			{
				TexturePath2 = (Inner.TexturePath2 = new Func<ModelMaterial, string>(Inner.tools.GetTexturePath2));
			}
			//迭代非重复贴图路径集合。若不为空就把贴图文件夹所在绝对路径加上格式化后的文件名及后缀。
			foreach (string current in expr_MaterialSet2.Select(TexturePath2).Distinct())
			{
				if (!(current == ""))
				{
					string text2 = text + "\\" + CleanPath(Path.GetFileName(current), options);

					//若指定文件text2不存在,那么就尝试将current中的文件复制到text2中。
					if (!File.Exists(text2))
					{
						try
						{
							File.Copy(current, text2);
						}
						catch (Exception)
						{
						}
					}
				}
			}
		}


		/// <summary>
		/// 重新编码整理名字
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		internal string CleanName(string name, ExportingOptions options)
		{
			string text;
			if (options.UnicodeSupport)
			{
				byte[] utf16Bytes = Utf16Encoder.GetBytes(name);
				text = Utf16Encoder.GetString(utf16Bytes);
			}
			else
			{
				byte[] asciiBytes = usAsciiEncoder.GetBytes(name);
				text = usAsciiEncoder.GetString(asciiBytes);
			}
			IEnumerable<char> char_set = text;
			Func<char, bool> IsSADM; //S：空格、A：字母、D：数字、M：标点符号。
			if ((IsSADM = Inner.IsChar1) == null)
			{
				IsSADM = Inner.IsChar1 = new Func<char, bool>(Inner.tools.IsSADM);
			}
			text = new string(char_set.Where(IsSADM).ToArray());
			return SecurityElement.Escape(text);
		}


		/// <summary>
		/// 根据用户设置，格式化字符串为Unicode或者ASCII编码。并筛选符合条件的字符串返回。
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		internal string CleanPath(string name, ExportingOptions options)
		{

			//string 类型名为@string的变量,目的是防止与string重名。
			string @string;
			//如果用户选择支持Unicode编码，那么将传入的字符串通过Utf16Encoder得到Utf16编码的字符串保存在@string中。
			if (options.UnicodeSupport)
			{
				byte[] bytes = Utf16Encoder.GetBytes(name);
				@string = Utf16Encoder.GetString(bytes);
			}
			//否者使用ASCII编码。
			else
			{
				byte[] bytes2 = usAsciiEncoder.GetBytes(name);
				@string = usAsciiEncoder.GetString(bytes2);
			}
			//将编码为特定格式的字符串赋值给字符类型的枚举。
			IEnumerable<char> char_set = @string;

			//创建判断字符是否否和特定要求的委托方法
			Func<char, bool> IsSADS;//S：空格、A：字母、D：数字、S：特殊标点符号。
			if ((IsSADS = Inner.IsChar2) == null)
			{
				IsSADS = Inner.IsChar2 = new Func<char, bool>(Inner.tools.IsSADS);
			}

			//筛选一个表中有没有满足条件的数据,返回一个字符串。
			return new string(char_set.Where(IsSADS).ToArray());
		}
	}
}

















