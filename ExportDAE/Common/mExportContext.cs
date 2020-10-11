using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Visual;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;

namespace ExportDAE
{

    internal class mExportContext : IExportContext
    {
		[CompilerGenerated]
		[Serializable]
		private sealed class Inner
		{
			public static readonly Inner tools = new Inner();
			public static Func<ModelMaterial, bool> mIsValidTexturePath;
			public static Func<ModelMaterial, string> TexturePath1;
			public static Func<ModelMaterial, string> TexturePath2;
			public static Func<char, bool> tools__25_0;
			public static Func<char, bool> IsChar2;


			/// <summary>
			/// IsValidTexturePath 判断贴图路径是否为空。
			/// </summary>
			internal bool IsValidTexturePath(ModelMaterial i) 	
			{
				return i.TexturePath != string.Empty;

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
			/// Is1	 指示指定的 Unicode 字符是否属于空格类别、字母或十进制数字类别、标点符号类别。中的一种。
			/// </summary>
			internal bool Is1(char c) 
			{
				return char.IsWhiteSpace(c) || char.IsLetterOrDigit(c) || char.IsPunctuation(c);
			}

			/// <summary>
			/// Is2	 指示指定的 Unicode 字符是否属于空格类别、字母或十进制数字类别、'.'、'-'之中的一种。
			/// </summary>
			internal bool Is2(char c)
			{
				return char.IsWhiteSpace(c) || char.IsLetterOrDigit(c) || c == '.' || c == '-';
			}

        }




		private Document mainDocument;
        private ExportingOptions exportingOptions;
        private bool isCancelled;
        private bool isElementDoubleSided;
        private Stack<ElementId> elementStack = new Stack<ElementId>();
		//定义transform(坐标转换矩阵)类型的堆栈。
        private Stack<Transform> transformationStack = new Stack<Transform>();
		//定义revit文档类型的堆栈。
        private Stack<Document> documentStack = new Stack<Document>();
        private Tuple<Document, ElementId> currentDocumentAndMaterialId;
        private Dictionary<Tuple<Document, ElementId>, IList<ModelGeometry>> documentAndMaterialIdToGeometries = new Dictionary<Tuple<Document, ElementId>, IList<ModelGeometry>>();
        private TextureFinder textureFinder;
        private int currentDecalMaterialId = -2147483648;
        private Dictionary<ElementId, Element> decalMaterialIdToDecal = new Dictionary<ElementId, Element>();
        private Options geometryOptions;
        private AssetSet libraryAssetSet;
        private static Encoding usAsciiEncoder = Encoding.GetEncoding("us-ascii", new EncoderReplacementFallback(string.Empty), new DecoderExceptionFallback());
        private static Encoding Utf16Encoder = Encoding.GetEncoding("unicode", new EncoderReplacementFallback(string.Empty), new DecoderExceptionFallback());


        //构造方法
        public mExportContext(Document document, ExportingOptions exportingOptions)
        {
			//初始化类，接受revit文档，导出选项，创建纹理查找工具，接受指定资产类型的revit数组并转换为集合。
			//创建解析首选项工具，并设置计算引用几何对象及设置提取几何图形精度为完整。

			//接受revit文档保存在mainDocument中。
            mainDocument = document;
            //接收导出选项保存在exportingOptions中。
            this.exportingOptions = exportingOptions;
			//创建TextureFinder（纹理查找工具）。保存在textureFinder中。
			textureFinder = new TextureFinder();
			//通过整数类型的资源属性类型转换为资产类型 获取指定类型的Revit中所有资产的数组。并转换为资产集合。保存在libraryAssetSet。中
			libraryAssetSet = (AssetSet)document.Application.GetAssets((AssetType)AssetPropertyType.Integer);
			//创建一个对象以指定几何解析中的用户首选项。保存在geometryOptions中。
			geometryOptions = mainDocument.Application.Create.NewGeometryOptions();
			//确定是否计算对几何对象的引用。计算引用的几何对象。
            geometryOptions.ComputeReferences = true;
			//使用这些选项提取的几何图形的详细程度。精细程度为完整。
			geometryOptions.DetailLevel = ViewDetailLevel.Fine;
        }

        public bool Start()
        {

			//创建文档与元素ID的元组，参数为本类中保存的revit文档及  IntegerValue为-1的无效ElementId。
			currentDocumentAndMaterialId = new Tuple<Document, ElementId>(this.mainDocument, ElementId.InvalidElementId);
			//textureFinder初始化，得到revit文档。
			textureFinder.Init(this.mainDocument);
			//从堆栈中移除所有的元素。
            documentStack.Clear();
			//向Stack的顶部添加revit文档对象。
            documentStack.Push(this.mainDocument);
            //从堆栈中移除所有的元素。
            transformationStack.Clear();
			//向Stack的顶部    TODO
			transformationStack.Push(this.GetProjectLocationTransform(this.exportingOptions.InsertionPoint));
            return true;
        }

        public void Finish()
        {
            if (documentAndMaterialIdToGeometries.Count > 0)
            {
                WriteFilePart();
            }
            textureFinder.Clear();
        }







		private void WriteFilePart()
		{

			//导出材质的文档和材质ID
			Dictionary<Tuple<Document, ElementId>, ModelMaterial> documentAndMaterialIdToExportedMaterial = ExportMaterials();

			//判断是否已经完成CollectTextures（纹理收集）
			if (exportingOptions.CollectTextures)
			{
				//定义导出材质的文件夹并将贴图放进这个文件夹。
				CollectTextures(documentAndMaterialIdToExportedMaterial);


				MakeTexturePathsRelative(documentAndMaterialIdToExportedMaterial);
			}

			//TODO collada
			new ColladaWriter(documentAndMaterialIdToExportedMaterial, documentAndMaterialIdToGeometries).Write(exportingOptions.FilePath);
			documentAndMaterialIdToGeometries.Clear();
			ChangeCurrentMaterial(currentDocumentAndMaterialId);
		}

		private void MakeTexturePathsRelative(Dictionary<Tuple<Document, ElementId>, ModelMaterial> documentAndMaterialIdToExportedMaterial)
		{
			IEnumerable<ModelMaterial> expr_MaterialSet3 = documentAndMaterialIdToExportedMaterial.Values;
			Func<ModelMaterial, bool> IsValidTexturePath;
			if ((IsValidTexturePath = Inner.mIsValidTexturePath) == null)
			{
				IsValidTexturePath = (Inner.mIsValidTexturePath = new Func<ModelMaterial, bool>(Inner.tools.IsValidTexturePath));
			}


			foreach (ModelMaterial current in expr_MaterialSet3.Where(IsValidTexturePath))
			{
				current.TexturePath = "textures\\" + this.CleanPath(Path.GetFileName(current.TexturePath));
			}
		}


		/// <summary>
		/// 通过构建委托函数获得ExportedMaterial中的贴图路径做迭代器的选择函数判断路径非空。
		/// 并定义导出材质的文件夹并将贴图放进这个文件夹。
		/// </summary>
		/// <param name="documentAndMaterialIdToExportedMaterial">欲导出材质的revit文档及材质ID</param>
		private void CollectTextures(Dictionary<Tuple<Document, ElementId>, ModelMaterial> documentAndMaterialIdToExportedMaterial)
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
			string text = Path.GetDirectoryName(this.exportingOptions.FilePath) + "\\textures";
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
					string text2 = text + "\\" + CleanPath(Path.GetFileName(current));

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

		private Transform GetProjectLocationTransform(int insertionPoint)
		{
			Transform transform = Transform.Identity;
			using (IEnumerator<Element> enumerator = new FilteredElementCollector(this.mainDocument).OfClass(typeof(BasePoint)).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					BasePoint basePoint = (BasePoint)enumerator.Current;
					if (basePoint.IsShared == (insertionPoint != 0))
					{
						double num = 0.0;
						Parameter parameter = basePoint.get_Parameter(BuiltInParameter.BASEPOINT_ANGLETON_PARAM);
						if (parameter != null)
						{
							num = parameter.AsDouble();
						}
						transform = this.GetTransformFromLocation(basePoint.get_Parameter(BuiltInParameter.BASEPOINT_EASTWEST_PARAM).AsDouble(), basePoint.get_Parameter(BuiltInParameter.BASEPOINT_NORTHSOUTH_PARAM).AsDouble(), basePoint.get_Parameter(BuiltInParameter.BASEPOINT_ELEVATION_PARAM).AsDouble(), -num);
					}
				}
			}
			ProjectPosition projectPosition = this.mainDocument.ActiveProjectLocation.GetProjectPosition(XYZ.Zero);
			Transform transformFromLocation = this.GetTransformFromLocation(projectPosition.EastWest, projectPosition.NorthSouth, projectPosition.Elevation, projectPosition.Angle);
			return transform.Inverse * transformFromLocation;
		}

		private Transform GetTransformFromLocation(double eastWest, double northSouth, double elevation, double northAngle)
		{
			Transform transform = Transform.CreateRotation(XYZ.BasisZ, northAngle);
			return Transform.CreateTranslation(new XYZ(eastWest, northSouth, elevation)) * transform;
		}

		private Dictionary<Tuple<Document, ElementId>, ModelMaterial> ExportMaterials()
		{
			Dictionary<Tuple<Document, ElementId>, ModelMaterial> dictionary = new Dictionary<Tuple<Document, ElementId>, ModelMaterial>();
			foreach (Tuple<Document, ElementId> current in this.documentAndMaterialIdToGeometries.Keys)
			{
				ElementId item = current.Item2;
				if (item.IntegerValue <= this.currentDecalMaterialId)
				{
					dictionary[current] = this.ExportDecalMaterial(item);
				}
				else
				{
					dictionary[current] = this.ExportMaterial(current);
				}
			}
			return dictionary;
		}

		private string CleanName(string name)
		{
			string text;
			if (this.exportingOptions.UnicodeSupport)
			{
				byte[] bytes = mExportContext.Utf16Encoder.GetBytes(name);
				text = mExportContext.Utf16Encoder.GetString(bytes);
			}
			else
			{
				byte[] bytes2 = mExportContext.usAsciiEncoder.GetBytes(name);
				text = mExportContext.usAsciiEncoder.GetString(bytes2);
			}
			IEnumerable<char> arg_65_0 = text;
			Func<char, bool> arg_65_1;
			if ((arg_65_1 = Inner.tools__25_0) == null)
			{
				arg_65_1 = (Inner.tools__25_0 = new Func<char, bool>(Inner.tools.Is1));
			}
			text = new string(arg_65_0.Where(arg_65_1).ToArray<char>());
			return SecurityElement.Escape(text);
		}



		/// <summary>
		/// 根据用户设置，格式化字符串为Unicode或者ASCII编码。并筛选符合条件的字符串返回。
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		private string CleanPath(string name)
		{

			//string 类型名为@string的变量,目的是防止与string重名。
			string @string;
			//如果用户选择支持Unicode编码，那么将传入的字符串通过Utf16Encoder得到Utf16编码的字符串保存在@string中。
			if (this.exportingOptions.UnicodeSupport)
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
			Func<char, bool> IsChar2;
			if ((IsChar2 = Inner.IsChar2) == null)
			{
				IsChar2 = Inner.IsChar2 = new Func<char, bool>(Inner.tools.Is2);
			}

			//筛选一个表中有没有满足条件的数据,返回一个字符串。
			return new string(char_set.Where(IsChar2).ToArray());
		}



		private ModelMaterial ExportMaterial(Tuple<Document, ElementId> documentAndMaterialId)
		{
			Document item = documentAndMaterialId.Item1;
			ElementId item2 = documentAndMaterialId.Item2;
			ModelMaterial exportedMaterial = new ModelMaterial();
			Material material = item.GetElement(item2) as Material;
			if (material != null && material.IsValidObject)
			{
				exportedMaterial.Name = this.CleanName(material.Name);
				if (material.Color.IsValid)
				{
					exportedMaterial.Color = System.Drawing.Color.FromArgb((int)material.Color.Red, (int)material.Color.Green, (int)material.Color.Blue);
				}
				exportedMaterial.Shininess = (double)material.Shininess;
				exportedMaterial.Transparency = (double)material.Transparency;
				if (material.AppearanceAssetId != ElementId.InvalidElementId)
				{
					Asset asset = (item.GetElement(material.AppearanceAssetId) as AppearanceAssetElement).GetRenderingAsset();
					if (asset.Size == 0)
					{
						AssetSetIterator assetSetIterator = this.libraryAssetSet.ForwardIterator();
						while (assetSetIterator.MoveNext())
						{
							Asset asset2 = assetSetIterator.Current as Asset;
							if (asset2 != null && asset2.Name == asset.Name)
							{
								asset = asset2;
								break;
							}
						}
					}
					this.textureFinder.FindDiffuseTexturePathFromAsset(exportedMaterial, asset);
					AssetPropertyDoubleArray4d assetPropertyDoubleArray4d = asset.FindByName("generic_diffuse") as AssetPropertyDoubleArray4d;
					if (assetPropertyDoubleArray4d != null)
					{
						exportedMaterial.Color = System.Drawing.Color.FromArgb((int)(byte)(assetPropertyDoubleArray4d.GetValueAsDoubles().ElementAt(1) * 255.0), (int)((byte)(assetPropertyDoubleArray4d.GetValueAsDoubles().ElementAt(1) * 255.0)), (int)(byte)(assetPropertyDoubleArray4d.GetValueAsDoubles().ElementAt(2) * 255.0));
					}
				}
			}
			return exportedMaterial;
		}

		private ModelMaterial ExportDecalMaterial(ElementId decalMaterialId)
		{
			return new ModelMaterial
			{
				Name = this.CleanName(this.decalMaterialIdToDecal[decalMaterialId].Name),
				TexturePath = this.textureFinder.FindDiffuseTextureDecal(this.decalMaterialIdToDecal[decalMaterialId])
			};
		}










		private void ExportSolids(Element element)
		{
			foreach (GeometryObject current in element.get_Geometry(this.geometryOptions))
			{
				this.ExportGeometryObject(current);
			}
		}

		private void ExportGeometryObject(GeometryObject geometryObject)
		{
			if (geometryObject == null)
			{
				return;
			}
			if (geometryObject.Visibility != null)
			{
				return;
			}
			GraphicsStyle graphicsStyle = this.documentStack.Peek().GetElement(geometryObject.GraphicsStyleId) as GraphicsStyle;
			if (graphicsStyle != null && graphicsStyle.Name.Contains("Light Source"))
			{
				return;
			}
			GeometryInstance geometryInstance = geometryObject as GeometryInstance;
			if (geometryInstance != null)
			{
				this.transformationStack.Push(this.transformationStack.Peek().Multiply(geometryInstance.Transform));
				GeometryElement symbolGeometry = geometryInstance.GetSymbolGeometry();
				if (symbolGeometry != null)
				{
					foreach (GeometryObject current in symbolGeometry)
					{
						if (current != null)
						{
							this.ExportGeometryObject(current);
						}
					}
				}
				this.transformationStack.Pop();
				return;
			}
			Solid solid = geometryObject as Solid;
			if (solid != null)
			{
				this.ExportSolid2(solid);
				return;
			}
		}

		private void ExportSolid(Solid solid)
		{
			SolidOrShellTessellationControls solidOrShellTessellationControls = new SolidOrShellTessellationControls();
			solidOrShellTessellationControls.LevelOfDetail=(double)this.exportingOptions.LevelOfDetail / 30.0;
			solidOrShellTessellationControls.Accuracy = 0.1;
			solidOrShellTessellationControls.MinAngleInTriangle = 0.0001;
			solidOrShellTessellationControls.MinExternalAngleBetweenTriangles = 1.0;
			try
			{
				TriangulatedSolidOrShell triangulatedSolidOrShell = SolidUtils.TessellateSolidOrShell(solid, solidOrShellTessellationControls);
				int shellComponentCount = triangulatedSolidOrShell.ShellComponentCount;
				for (int i = 0; i < shellComponentCount; i++)
				{
					TriangulatedShellComponent shellComponent = triangulatedSolidOrShell.GetShellComponent(i);
					ModelGeometry exportedGeometry = new ModelGeometry();
					exportedGeometry.Transform = this.transformationStack.Peek();
					exportedGeometry.Points = new List<XYZ>(shellComponent.VertexCount);
					for (int num = 0; num != shellComponent.VertexCount; num++)
					{
						exportedGeometry.Points.Add(shellComponent.GetVertex(num));
					}
					exportedGeometry.Indices = new List<int>(shellComponent.TriangleCount * 3);
					for (int j = 0; j < shellComponent.TriangleCount; j++)
					{
						TriangleInShellComponent triangle = shellComponent.GetTriangle(j);
						exportedGeometry.Indices.Add(triangle.VertexIndex0);
						exportedGeometry.Indices.Add(triangle.VertexIndex1);
						exportedGeometry.Indices.Add(triangle.VertexIndex2);
					}
					exportedGeometry.CalculateNormals(false);
					exportedGeometry.CalculateUVs(true, false);
					ElementId materialElementId = solid.Faces.get_Item(0).MaterialElementId;
					Tuple<Document, ElementId> tuple = new Tuple<Document, ElementId>(this.documentStack.Peek(), materialElementId);
					this.ChangeCurrentMaterial(tuple);
					this.documentAndMaterialIdToGeometries[tuple].Add(exportedGeometry);
				}
			}
			catch (Exception)
			{
			}
		}

		private void ExportSolid2(Solid solid)
		{
			foreach (Face face in solid.Faces)
			{
				if (!(face == null))
				{
					Mesh mesh = face.Triangulate((double)this.exportingOptions.LevelOfDetail / 15.0);
					if (!(mesh == null) && !mesh.Visibility.Equals(3))
					{
						ModelGeometry exportedGeometry = new ModelGeometry();
						exportedGeometry.Transform = this.transformationStack.Peek();
						exportedGeometry.Points = new List<XYZ>(mesh.Vertices);
						exportedGeometry.Indices = new List<int>(mesh.NumTriangles * 3);
						if (exportedGeometry.Transform.IsConformal && exportedGeometry.Transform.HasReflection)
						{
							for (int i = 0; i < mesh.NumTriangles; i++)
							{
								MeshTriangle meshTriangle = mesh.get_Triangle(i);
								exportedGeometry.Indices.Add((int)meshTriangle.get_Index(0));
								exportedGeometry.Indices.Add((int)meshTriangle.get_Index(2));
								exportedGeometry.Indices.Add((int)meshTriangle.get_Index(1));
							}
						}
						else
						{
							for (int j = 0; j < mesh.NumTriangles; j++)
							{
								MeshTriangle meshTriangle2 = mesh.get_Triangle(j);
								exportedGeometry.Indices.Add((int)meshTriangle2.get_Index(0));
								exportedGeometry.Indices.Add((int)meshTriangle2.get_Index(1));
								exportedGeometry.Indices.Add((int)meshTriangle2.get_Index(2));
							}
						}
						ModelGeometry expr_166 = exportedGeometry;
						expr_166.CalculateNormals(expr_166.Transform.IsConformal && exportedGeometry.Transform.HasReflection);
						exportedGeometry.CalculateUVs(true, false);
						if (face.IsTwoSided)
						{
							exportedGeometry.MakeDoubleSided();
						}
						ElementId materialElementId = face.MaterialElementId;
						Tuple<Document, ElementId> tuple = new Tuple<Document, ElementId>(this.documentStack.Peek(), materialElementId);
						this.ChangeCurrentMaterial(tuple);
						this.documentAndMaterialIdToGeometries[tuple].Add(exportedGeometry);
					}
				}
			}
		}

		public bool IsElementDecal(Element element)
		{
			try
			{
				ElementId typeId = element.GetTypeId();
				if ((this.documentStack.Peek().GetElement(typeId) as ElementType).GetExternalFileReference().ExternalFileReferenceType.Equals(5))
				{
					return true;
				}
			}
			catch (Exception)
			{
			}
			return false;
		}

		public bool IsDecalCurved(Element element)
		{
			GeometryElement source = element.get_Geometry(this.geometryOptions);
			try
			{
				if (source.ToArray<GeometryObject>()[1].GetType() == typeof(Arc) && source.ToArray<GeometryObject>()[3].GetType() == typeof(Arc))
				{
					return true;
				}
			}
			catch (Exception)
			{
			}
			return false;
		}

		private void ExportDecal(Element element)
		{
			if (this.IsDecalCurved(element))
			{
				this.ExportDecalCurved(element);
				return;
			}
			this.ExportDecalFlat(element);
		}

		private void ExportDecalFlat(Element element)
		{
			ModelGeometry exportedGeometry = new ModelGeometry();
			exportedGeometry.Transform = this.transformationStack.Peek();
			GeometryElement arg_2F_0 = element.get_Geometry(this.geometryOptions);
			exportedGeometry.Points = new List<XYZ>(4);
			using (IEnumerator<GeometryObject> enumerator = arg_2F_0.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Line line = (Line)enumerator.Current;
					if (!(line == null))
					{
						exportedGeometry.Points.Add(line.Origin);
						if (exportedGeometry.Points.Count >= 4)
						{
							break;
						}
					}
				}
			}
			if (exportedGeometry.Points.Count != 4)
			{
				return;
			}
			exportedGeometry.Indices = new List<int>(6);
			exportedGeometry.Indices.Add(0);
			exportedGeometry.Indices.Add(2);
			exportedGeometry.Indices.Add(1);
			exportedGeometry.Indices.Add(0);
			exportedGeometry.Indices.Add(3);
			exportedGeometry.Indices.Add(2);
			exportedGeometry.Uvs = new List<UV>(4);
			exportedGeometry.Uvs.Add(new UV(0.0, 1.0));
			exportedGeometry.Uvs.Add(new UV(1.0, 1.0));
			exportedGeometry.Uvs.Add(new UV(1.0, 0.0));
			exportedGeometry.Uvs.Add(new UV(0.0, 0.0));
			exportedGeometry.CalculateNormals(false);
			exportedGeometry.MakeDoubleSided();
			int num = this.currentDecalMaterialId;
			this.currentDecalMaterialId = num + 1;
			ElementId elementId = new ElementId(num);
			this.decalMaterialIdToDecal.Add(elementId, element);
			Tuple<Document, ElementId> tuple = new Tuple<Document, ElementId>(this.documentStack.Peek(), elementId);
			this.ChangeCurrentMaterial(tuple);
			this.documentAndMaterialIdToGeometries[tuple].Add(exportedGeometry);
		}

		private void ExportDecalCurved(Element element)
		{
			ModelGeometry exportedGeometry = new ModelGeometry();
			exportedGeometry.Transform = this.transformationStack.Peek();
			GeometryElement expr_23 = element.get_Geometry(this.geometryOptions);
			Arc arc = expr_23.ToArray<GeometryObject>()[1] as Arc;
			Curve arg_49_0 = expr_23.ToArray<GeometryObject>()[3] as Arc;
			XYZ[] array = arc.Tessellate().ToArray<XYZ>();
			XYZ[] array2 = arg_49_0.Tessellate().ToArray<XYZ>();
			exportedGeometry.Points = new List<XYZ>(array.Length + array2.Length);
			exportedGeometry.Uvs = new List<UV>(array.Length + array2.Length);
			for (int i = 0; i < array.Length; i++)
			{
				exportedGeometry.Points.Add(array[i]);
				exportedGeometry.Uvs.Add(new UV((double)(1f - (float)i * (1f / (float)(array.Length - 1))), 1.0));
			}
			for (int j = 0; j < array2.Length; j++)
			{
				exportedGeometry.Points.Add(array2[j]);
				exportedGeometry.Uvs.Add(new UV((double)((float)j * (1f / (float)(array.Length - 1))), 0.0));
			}
			exportedGeometry.Indices = new List<int>((array.Length - 1) * 6);
			for (int k = 0; k < array.Length - 1; k++)
			{
				exportedGeometry.Indices.Add(k);
				exportedGeometry.Indices.Add(k + 1);
				exportedGeometry.Indices.Add(array.Length * 2 - k - 1);
				exportedGeometry.Indices.Add(k + 1);
				exportedGeometry.Indices.Add(array.Length * 2 - k - 2);
				exportedGeometry.Indices.Add(array.Length * 2 - k - 1);
			}
			exportedGeometry.CalculateNormals(false);
			exportedGeometry.MakeDoubleSided();
			int num = this.currentDecalMaterialId;
			this.currentDecalMaterialId = num + 1;
			ElementId elementId = new ElementId(num);
			this.decalMaterialIdToDecal.Add(elementId, element);
			Tuple<Document, ElementId> tuple = new Tuple<Document, ElementId>(this.documentStack.Peek(), elementId);
			this.ChangeCurrentMaterial(tuple);
			this.documentAndMaterialIdToGeometries[tuple].Add(exportedGeometry);
		}
		private bool IsElementSmallerThan(Element element, double size)
		{
			BoundingBoxXYZ boundingBoxXYZ = element.get_BoundingBox(this.exportingOptions.MainView3D);
			return boundingBoxXYZ != null && boundingBoxXYZ.Enabled && (boundingBoxXYZ.Max - boundingBoxXYZ.Min).GetLength() < size;
		}

		private bool IsElementStructural(Element element)
		{
			Category category = element.Category;
			return category != null && (category.Id.IntegerValue.Equals(-2001320) || category.Id.IntegerValue.Equals(-2000175) || category.Id.IntegerValue.Equals(-2000017) || category.Id.IntegerValue.Equals(-2000018) || category.Id.IntegerValue.Equals(-2000019) || category.Id.IntegerValue.Equals(-2000020) || category.Id.IntegerValue.Equals(-2000126));
		}

		private bool IsElementInInteriorCategory(Element element)
		{
			Category category = element.Category;
			return category != null && (category.Id.IntegerValue.Equals(-2000080) || category.Id.IntegerValue.Equals(-2001000) || category.Id.IntegerValue.Equals(-2001040) || category.Id.IntegerValue.Equals(-2001060) || category.Id.IntegerValue.Equals(-2001100) || category.Id.IntegerValue.Equals(-2001140) || category.Id.IntegerValue.Equals(-2001160) || category.Id.IntegerValue.Equals(-2001350) || category.Id.IntegerValue.Equals(-2008013) || category.Id.IntegerValue.Equals(-2008075) || category.Id.IntegerValue.Equals(-2008077) || category.Id.IntegerValue.Equals(-2008079) || category.Id.IntegerValue.Equals(-2008081) || category.Id.IntegerValue.Equals(-2008085) || category.Id.IntegerValue.Equals(-2008083) || category.Id.IntegerValue.Equals(-2008087) || category.Id.IntegerValue.Equals(-2000151) || category.Id.IntegerValue.Equals(-2001120));
		}

		private bool IsElementInDraftCategory(Element element)
		{
			Category category = element.Category;
			if (category != null)
			{
				while (category.Parent != null)
				{
					category = category.Parent;
				}
				if (category.Id.IntegerValue.Equals(-2001391) || category.Id.IntegerValue.Equals(-2001352) || category.Id.IntegerValue.Equals(-2000011) || category.Id.IntegerValue.Equals(-2000035) || category.Id.IntegerValue.Equals(-2001340) || category.Id.IntegerValue.Equals(-2000100) || category.Id.IntegerValue.Equals(-2000032) || category.Id.IntegerValue.Equals(-2000038) || category.Id.IntegerValue.Equals(-2000014) || category.Id.IntegerValue.Equals(-2000120) || category.Id.IntegerValue.Equals(-2001180) || category.Id.IntegerValue.Equals(-2001220) || category.Id.IntegerValue.Equals(-2000180) || category.Id.IntegerValue.Equals(-2000090) || category.Id.IntegerValue.Equals(-2000170) || category.Id.IntegerValue.Equals(-2000171) || category.Id.IntegerValue.Equals(-2001300) || category.Id.IntegerValue.Equals(-2001330) || category.Id.IntegerValue.Equals(-2001320) || category.Id.IntegerValue.Equals(-2000023))
				{
					return true;
				}
			}
			return false;
		}

		private bool IsElementInDoubleSidedCategory(Element element)
		{
			Category category = element.Category;
			if (category != null)
			{
				while (category.Parent != null)
				{
					category = category.Parent;
				}
				if (category.Id.IntegerValue.Equals(-2000038))
				{
					return true;
				}
			}
			return false;
		}







		public bool IsCanceled()
        {
			return this.isCancelled;
		}

        public RenderNodeAction OnViewBegin(ViewNode node)
        {
			node.LevelOfDetail = this.exportingOptions.LevelOfDetail;
			return 0;
		}

        public void OnViewEnd(ElementId elementId)
        {
            throw new NotImplementedException();
        }

        public void OnElementEnd(ElementId elementId)
        {
			this.elementStack.Pop();
		}

        public RenderNodeAction OnInstanceBegin(InstanceNode node)
        {
			this.transformationStack.Push(this.transformationStack.Peek().Multiply(node.GetTransform()));
			return 0;
		}

        public void OnInstanceEnd(InstanceNode node)
        {
			this.transformationStack.Pop();
		}

        public RenderNodeAction OnLinkBegin(LinkNode node)
        {

			this.documentStack.Push(node.GetDocument());
			this.transformationStack.Push(this.transformationStack.Peek().Multiply(node.GetTransform()));
			return 0;
		}

        public void OnLinkEnd(LinkNode node)
        {
			this.documentStack.Pop();
			this.transformationStack.Pop();
		}

        public RenderNodeAction OnFaceBegin(FaceNode node)
        {
			return 0;
		}

        public void OnFaceEnd(FaceNode node)
        {
            
        }

        public void OnRPC(RPCNode node)
        {
           
        }

        public void OnLight(LightNode node)
        {
            throw new NotImplementedException();
        }

        public void OnMaterial(MaterialNode node)
        {
			this.ChangeCurrentMaterial(new Tuple<Document, ElementId>(this.documentStack.Peek(), node.MaterialId));
		}

        public void OnPolymesh(PolymeshTopology polymesh)
        {
			ModelGeometry exportedGeometry = new ModelGeometry();
			exportedGeometry.Points = polymesh.GetPoints();
			exportedGeometry.Normals = polymesh.GetNormals();
			exportedGeometry.Uvs = polymesh.GetUVs();
			exportedGeometry.Transform = this.transformationStack.Peek();
			exportedGeometry.DistributionOfNormals = polymesh.DistributionOfNormals;
			exportedGeometry.Indices = new List<int>(polymesh.GetFacets().Count * 3);
			if (exportedGeometry.Transform.IsConformal && exportedGeometry.Transform.HasReflection)
			{
				using (IEnumerator<PolymeshFacet> enumerator = polymesh.GetFacets().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						PolymeshFacet current = enumerator.Current;
						exportedGeometry.Indices.Add(current.V1);
						exportedGeometry.Indices.Add(current.V3);
						exportedGeometry.Indices.Add(current.V2);
					}
					goto IL_131;
				}
			}
			foreach (PolymeshFacet current2 in polymesh.GetFacets())
			{
				exportedGeometry.Indices.Add(current2.V1);
				exportedGeometry.Indices.Add(current2.V2);
				exportedGeometry.Indices.Add(current2.V3);
			}
			IL_131:
			if (this.isElementDoubleSided)
			{
				exportedGeometry.MakeDoubleSided();
			}
			this.documentAndMaterialIdToGeometries[this.currentDocumentAndMaterialId].Add(exportedGeometry);
		}

		private void ChangeCurrentMaterial(Tuple<Document, ElementId> documentAndMaterialId)
		{
			this.currentDocumentAndMaterialId = documentAndMaterialId;
			if (!this.documentAndMaterialIdToGeometries.ContainsKey(this.currentDocumentAndMaterialId))
			{
				this.documentAndMaterialIdToGeometries.Add(this.currentDocumentAndMaterialId, new List<ModelGeometry>(100));
			}
		}

		public RenderNodeAction OnElementBegin(ElementId elementId)
		{
			this.elementStack.Push(elementId);
			Element element = this.documentStack.Peek().GetElement(elementId);
			if (element != null)
			{
				if (this.exportingOptions.SkipInteriorDetails && this.IsElementInInteriorCategory(element))
				{
					return RenderNodeAction.Skip;
				}
				this.isElementDoubleSided = false;
				if (this.IsElementInDoubleSidedCategory(element))
				{
					this.isElementDoubleSided = true;
				}
				if (element is FamilyInstance && this.exportingOptions.GeometryOptimization)
				{
					this.ExportSolids(element);
					return RenderNodeAction.Skip;
				}
				if (this.IsElementStructural(element))
				{
					this.ExportSolids(element);
					return RenderNodeAction.Skip;
				}
				if (this.IsElementDecal(element))
				{
					this.ExportDecal(element);
					return RenderNodeAction.Skip;
				}
			}
			return 0;
		}
	}

}





