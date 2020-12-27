using Autodesk.Revit.DB;
using Microsoft.Win32;
using Autodesk.Revit.DB.Visual;
using System;
using System.Collections.Generic;
using System.IO;

namespace ExportDAE
{
	internal class TextureFinder    //纹理查找器
	{
		private ICollection<string> textureFolders;
		private Document document;

		public void Init(Document document)
		{
			//初始化，获取revit模型
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
	}
}

















