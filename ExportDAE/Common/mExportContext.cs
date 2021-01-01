using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Visual;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExportDAE
{

	internal class MExportContext : IExportContext
	{
	
		/// <summary>
		/// 文档
		/// </summary>
		private Document document;

		/// <summary>
		/// 用户导出设置
		/// </summary>
		private ExportingOptions userSetting;

		/// <summary>
		/// 是否结束导出？默认否。
		/// </summary>
		private bool isCancelled = false;

		/// <summary>
		/// 元素是否是双面？Sided 双面双面材质显示单面的有边的。DoubleSided 材质双面材质材质面板图层背面是否显示
		/// </summary>
		private bool isElementDoubleSided;

		/// <summary>
		/// 定义元素堆栈。
		/// </summary>
		private Stack<ElementId> elementStack = new Stack<ElementId>();
		/// <summary>
		/// 定义transform(坐标转换矩阵)类型的堆栈。
		/// </summary>
		private Stack<Transform> transformationStack = new Stack<Transform>();
		/// <summary>
		/// 定义revit文档类型的堆栈。
		/// </summary>
		private Stack<Document> documentStack = new Stack<Document>();

		/// <summary>
		/// 当前的Document 和 材质ID。
		/// </summary>
		private Tuple<Document, ElementId> currentDocumentAndMaterialId;

		/// <summary>
		/// 文档和材料ID到Geometries
		/// </summary>
		private Dictionary<Tuple<Document, ElementId>, IList<ModelGeometry>> documentAndMaterialIdToGeometries = new Dictionary<Tuple<Document, ElementId>, IList<ModelGeometry>>();
		private TextureFinder textureFinder;

		/// <summary>
		/// 当前Decal贴花效果材质ID，在游戏中，decal是一种非常常见的效果，常用来实现弹孔，血迹，涂鸦等效果。
		/// </summary>
		private int currentDecalMaterialId = Int32.MinValue;
		private Dictionary<ElementId, Element> decalMaterialIdToDecal = new Dictionary<ElementId, Element>();

		/// <summary>
		/// 你要对几何对象做什么可选的修改？
		/// </summary>
		private Options Geometry_Options;
		private AssetSet libraryAssetSet = new AssetSet();



		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="document">Revit活动视图的文档</param>
		/// <param name="userSetting">保存导出的用户设置</param>
		public MExportContext(Document document, ExportingOptions userSetting)
		{
			//初始化类，接受revit文档，导出选项，创建纹理查找工具，接受指定资产类型的revit数组并转换为集合。
			//创建解析首选项工具，并设置计算引用几何对象及设置提取几何图形精度为完整。

			this.document = document;
			this.userSetting = userSetting;
			textureFinder = new TextureFinder();

			//获取文档中所有Appearance assets（materials）迭代保存到libraryAssetSet集合里面。
			IList<Asset> assets = document.Application.GetAssets(AssetType.Appearance);
			foreach (Asset asset in assets)
			{

				libraryAssetSet.Insert(asset);

			}

			//创建几何解析中的用户首选项。
			Geometry_Options = this.document.Application.Create.NewGeometryOptions();
			//确定是否计算对几何对象的引用。计算引用的几何对象。//使用这些选项提取的几何图形的详细程度。精细程度为完整。
			Geometry_Options.ComputeReferences = true;
			Geometry_Options.DetailLevel = ViewDetailLevel.Fine;
		}



		/// <summary>
		/// 在导出过程的最开始即在发送模型的第一个实体之前就调用此方法。
		/// </summary>
		/// <returns>如果您准备继续进行导出，则返回True。</returns>
		bool IExportContext.Start()
		{

			//当前的模型文档和材料ID是，文档与一个无效的ID
			currentDocumentAndMaterialId = new Tuple<Document, ElementId>(document, ElementId.InvalidElementId);

			//textureFinder，收集包含材质的文件夹。
			textureFinder.Init(document);

			//从堆栈中移除所有的元素并向Stack的顶部添加revit文档。
			documentStack.Clear();
			documentStack.Push(document);

			//从堆栈中移除所有的元素，并添加新的插入点坐标系。
			transformationStack.Clear();
			transformationStack.Push(GetProjectLocationTransform(userSetting.InsertionPoint));

			return true;
		}


		/// <summary>
		/// 标记要导出的3D视图的开始。
		/// </summary>
		/// <param name="node">与视图关联的几何节点。</param>
		/// <returns>如果要跳过导出此视图，则返回RenderNodeAction.Skip，否则返回RenderNodeAction.Proceed。</returns>
		RenderNodeAction IExportContext.OnViewBegin(ViewNode node)
		{
			///视图将呈现的详细程度。
			/*整数形式，取值范围是[0,15]（含[0,15]）或{-1}。 如果该值为正，则Revit将在细分面时使用建议的细节等级。
             * 否则，它将使用基于输出分辨率的默认算法。 如果要求明确的细节级别（即正值），
             * 则使用接近有效范围中间值的值会产生非常合理的细分。  Revit使用级别8作为其“正常” LOD。*/
			node.LevelOfDetail = this.userSetting.LevelOfDetail;
			return RenderNodeAction.Proceed;
		}



		/// <summary>
		/// 标记要导出的元素的开始。 表明为一个element intance 也就是一个模型实例数据的开始.
		/// 一个模型实例可以包含自身顶点数据,也可以包含child instance.
		/// 所以这里需要建立模型实例的树状结构.
		/// </summary>
		/// <param name="elementId">即将处理的元素的ID。</param>
		/// <returns>如果要跳过导出此元素，请返回RenderNodeAction.Skip，否则返回RenderNodeAction.Proceed。</returns>
		RenderNodeAction IExportContext.OnElementBegin(ElementId elementId)
		{
            elementStack.Push(elementId);
            Element element = this.documentStack.Peek().GetElement(elementId);
            if (element != null)
            {
                if (this.userSetting.SkipInteriorDetails && this.IsElementInInteriorCategory(element))
                {
                    return RenderNodeAction.Skip;
                }
                this.isElementDoubleSided = false;
                if (this.IsElementInDoubleSidedCategory(element))
                {
                    this.isElementDoubleSided = true;
                }
                if (element is FamilyInstance && this.userSetting.GeometryOptimization)
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
            return RenderNodeAction.Proceed;
		}


		/// <summary>
		/// 标志着材料的变化。
		/// </summary>
		/// <param name="node">描述当前材料的节点。</param>
		void IExportContext.OnMaterial(MaterialNode node)
		{
			this.ChangeCurrentMaterial(new Tuple<Document, ElementId>(this.documentStack.Peek(), node.MaterialId));
		}


		/// <summary>
		/// 标记要导出的Face的开始。
		/// </summary>
		/// <param name="node">表示面的输出节点。</param>
		/// <returns>返回RenderNodeAction。 如果希望接收此面的几何图形（多边形），请继续，否则返回RenderNodeAction.Skip。</returns>
		RenderNodeAction IExportContext.OnFaceBegin(FaceNode node)
		{
			return RenderNodeAction.Proceed;
		}


		/// <summary>
		/// 当输出3d面的镶嵌多边形时，将调用此方法。
		/// </summary>
		/// <param name="polymesh">表示多边形网格拓扑的节点</param>
		void IExportContext.OnPolymesh(PolymeshTopology polymesh)
		{
            ModelGeometry exportedGeometry = new ModelGeometry
            {
                Points = polymesh.GetPoints(),
                Normals = polymesh.GetNormals(),
                Uvs = polymesh.GetUVs(),
                Transform = transformationStack.Peek(),
                DistributionOfNormals = polymesh.DistributionOfNormals,
                Indices = new List<int>(polymesh.GetFacets().Count * 3)
            };
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
                    goto Fine;
                }
            }
            foreach (PolymeshFacet current2 in polymesh.GetFacets())
            {
                exportedGeometry.Indices.Add(current2.V1);
                exportedGeometry.Indices.Add(current2.V2);
                exportedGeometry.Indices.Add(current2.V3);
            }
            Fine:
            if (isElementDoubleSided)
            {
                exportedGeometry.MakeDoubleSided();
            }
            documentAndMaterialIdToGeometries[currentDocumentAndMaterialId].Add(exportedGeometry);
        }


		/// <summary>
		/// 标记要导出的当前面的末端。
		/// </summary>
		/// <param name="node">表示面的输出节点。</param>
		void IExportContext.OnFaceEnd(FaceNode node)
		{
			
		}


		/// <summary>
		/// 在处理完所有实体之后（或取消处理之后），在导出过程的最后将调用此方法。
		/// </summary>
		void IExportContext.Finish()
		{
            if (documentAndMaterialIdToGeometries.Count > 0)
            {
                //导出材质的文档和材质ID
                Dictionary<Tuple<Document, ElementId>, ModelMaterial> documentAndMaterialIdToExportedMaterial = ExportMaterials();

                //判断是否已经完成CollectTextures（纹理收集）
                if (userSetting.CollectTextures)
                {
                    //定义导出材质的文件夹并将贴图放进这个文件夹。//确保路径是绝对路径
                    textureFinder.CollectTextures(documentAndMaterialIdToExportedMaterial, userSetting);
                    textureFinder.MakeTexturePathsRelative(documentAndMaterialIdToExportedMaterial, userSetting);
                }

                //TODO collada
                new ColladaWriter(documentAndMaterialIdToExportedMaterial, documentAndMaterialIdToGeometries).Write(userSetting.FilePath);
                //new ColladaStream(documentAndMaterialIdToExportedMaterial, documentAndMaterialIdToGeometries);
                documentAndMaterialIdToGeometries.Clear();
                ChangeCurrentMaterial(currentDocumentAndMaterialId);
            }
            textureFinder.Clear();
        }



		/// <summary>
		/// 在每个元素的开头都会查询此方法。
		/// </summary>
		/// <returns>如果您希望取消导出，则返回True，否则返回False。</returns>
		bool IExportContext.IsCanceled()
		{
			return isCancelled;
		}


		/// <summary>
		/// 标记要导出的3D视图的结束。
		/// </summary>
		/// <param name="elementId">刚刚处理过的3D视图的ID。</param>
		void IExportContext.OnViewEnd(ElementId elementId)
		{
			throw new NotImplementedException();
		}


		/// <summary>
		/// 标记要导出的元素的结尾。
		/// </summary>
		/// <param name="elementId">刚刚处理过的元素的ID。</param>
		void IExportContext.OnElementEnd(ElementId elementId)
		{
			this.elementStack.Pop();
		}



		/// <summary>
		/// 标记了要导出的族实例的开始。
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		RenderNodeAction IExportContext.OnInstanceBegin(InstanceNode node)
		{
			this.transformationStack.Push(this.transformationStack.Peek().Multiply(node.GetTransform()));
			return RenderNodeAction.Proceed;
		}


		/// <summary>
		/// 标志着要导出的族实例的结尾。
		/// </summary>
		/// <param name="node"></param>
		void IExportContext.OnInstanceEnd(InstanceNode node)
		{
			this.transformationStack.Pop();
		}


		/// <summary>
		/// 标记要导出的链接实例的开始。
		/// </summary>
		/// <param name="node">表示Revit链接的输出节点。</param>
		/// <returns>如果您希望跳过处理此链接实例，则返回RenderNodeAction.Skip，否则返回RenderNodeAction.Proceed。</returns>
		RenderNodeAction IExportContext.OnLinkBegin(LinkNode node)
		{

			this.documentStack.Push(node.GetDocument());
			this.transformationStack.Push(this.transformationStack.Peek().Multiply(node.GetTransform()));
			return RenderNodeAction.Proceed;
		}


		/// <summary>
		/// 标记要导出的链接实例的结尾。
		/// </summary>
		/// <param name="node">表示Revit链接的输出节点。</param>
		void IExportContext.OnLinkEnd(LinkNode node)
		{
			this.documentStack.Pop();
			this.transformationStack.Pop();
		}


		/// <summary>
		/// 标志着RPC对象导出的开始。RPC（Remote Procedure Call Protocol）远程过程调用协议，
		/// 它是一种通过网络从远程计算机程序上请求服务，而不需要了解底层网络技术的协议。
		/// 简言之，RPC使得程序能够像访问本地系统资源一样，去访问远端系统资源。
		/// </summary>
		/// <param name="node">具有有关RPC对象的资产信息的节点。</param>
		void IExportContext.OnRPC(RPCNode node)
		{
			throw new NotImplementedException();
		}


		/// <summary>
		/// 标记了已启用渲染的灯光的开始输出。
		/// 仅对照片渲染导出（实现Autodesk.Revit.DB.IPhotoRenderContext的自定义导出器）调用此方法。
		/// </summary>
		/// <param name="node">描述灯光对象的节点。</param>
		void IExportContext.OnLight(LightNode node)
		{
			throw new NotImplementedException();
		}






		#region 设置模型原点
		/// <summary>
		/// 设置模型原点
		/// </summary>
		/// <param name="insertionPoint">如果是0表示项目基点，非零表示观测点。</param>
		/// <returns></returns>
		private Transform GetProjectLocationTransform(int insertionPoint)
		{
			Transform transform = Transform.Identity;
			using (IEnumerator<Element> enumerator = new FilteredElementCollector(this.document).OfClass(typeof(BasePoint)).GetEnumerator())
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
			ProjectPosition projectPosition = this.document.ActiveProjectLocation.GetProjectPosition(XYZ.Zero);
			Transform transformFromLocation = this.GetTransformFromLocation(projectPosition.EastWest, projectPosition.NorthSouth, projectPosition.Elevation, projectPosition.Angle);
			return transform.Inverse * transformFromLocation;
		}

		private Transform GetTransformFromLocation(double eastWest, double northSouth, double elevation, double northAngle)
		{
			Transform transform = Transform.CreateRotation(XYZ.BasisZ, northAngle);
			return Transform.CreateTranslation(new XYZ(eastWest, northSouth, elevation)) * transform;
		}

        #endregion



        #region 材质处理
        /// <summary>
        /// 将要导出的材质
        /// </summary>
        /// <returns>返回字典类型数据</returns>
        private Dictionary<Tuple<Document, ElementId>, ModelMaterial> ExportMaterials()
		{
			Dictionary<Tuple<Document, ElementId>, ModelMaterial> dictionary = new Dictionary<Tuple<Document, ElementId>, ModelMaterial>();
			foreach (Tuple<Document, ElementId> current in documentAndMaterialIdToGeometries.Keys)
			{
				ElementId item = current.Item2;
				if (item.IntegerValue <= currentDecalMaterialId)
				{
					dictionary[current] = ExportDecalMaterial(item);
				}
				else
				{
					dictionary[current] = ExportMaterial(current);
				}
			}
			return dictionary;
		}


		private ModelMaterial ExportMaterial(Tuple<Document, ElementId> documentAndMaterialId)
		{
			Document item = documentAndMaterialId.Item1;
			ElementId item2 = documentAndMaterialId.Item2;
			ModelMaterial exportedMaterial = new ModelMaterial();
			Material material = item.GetElement(item2) as Material;
			if (material != null && material.IsValidObject)
			{
				exportedMaterial.Name = textureFinder.CleanName(material.Name, userSetting);
				if (material.Color.IsValid)
				{
					exportedMaterial.Color = System.Drawing.Color.FromArgb(material.Color.Red, material.Color.Green, material.Color.Blue);
				}
				exportedMaterial.Shininess = material.Shininess;
				exportedMaterial.Transparency = material.Transparency;
				if (material.AppearanceAssetId != ElementId.InvalidElementId)
				{
					Asset asset = (item.GetElement(material.AppearanceAssetId) as AppearanceAssetElement).GetRenderingAsset();
					if (asset.Size == 0)
					{
						AssetSetIterator assetSetIterator = libraryAssetSet.ForwardIterator();
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
					textureFinder.FindDiffuseTexturePathFromAsset(exportedMaterial, asset);
					AssetPropertyDoubleArray4d assetPropertyDoubleArray4d = asset.FindByName("generic_diffuse") as AssetPropertyDoubleArray4d;
					if (assetPropertyDoubleArray4d != null)
					{
						exportedMaterial.Color = System.Drawing.Color.FromArgb((byte)(assetPropertyDoubleArray4d.GetValueAsDoubles().ElementAt(1) * 255.0), ((byte)(assetPropertyDoubleArray4d.GetValueAsDoubles().ElementAt(1) * 255.0)), (byte)(assetPropertyDoubleArray4d.GetValueAsDoubles().ElementAt(2) * 255.0));
					}
				}
			}
			return exportedMaterial;
		}


		private ModelMaterial ExportDecalMaterial(ElementId decalMaterialId)
		{
			return new ModelMaterial
			{
				Name = textureFinder.CleanName(decalMaterialIdToDecal[decalMaterialId].Name, userSetting),
				TexturePath = textureFinder.FindDiffuseTextureDecal(decalMaterialIdToDecal[decalMaterialId])
			};
		}



		private void ChangeCurrentMaterial(Tuple<Document, ElementId> documentAndMaterialId)
		{
			this.currentDocumentAndMaterialId = documentAndMaterialId;
			if (!this.documentAndMaterialIdToGeometries.ContainsKey(this.currentDocumentAndMaterialId))
			{
				this.documentAndMaterialIdToGeometries.Add(this.currentDocumentAndMaterialId, new List<ModelGeometry>(100));
			}
		}

        #endregion



        #region 导出实体Solids
        /// <summary>
        /// 体 - Solid::::
        /// 一个 Solid 是由 Face 和 Edge 来定义它边界的实体。
        /// </summary>
        /// <param name="element"></param>
        private void ExportSolids(Element element)
		{
			foreach (GeometryObject current in element.get_Geometry(this.Geometry_Options))
			{
				this.ExportGeometryObject(current);
			}
		}


		private void ExportGeometryObject(GeometryObject geometryObject)
		{
			if (geometryObject.Equals(null))
			{
				return;
			}
            if (geometryObject.Visibility.Equals(Visibility.Invisible))
            {
                return;
            }
            if (documentStack.Peek().GetElement(geometryObject.GraphicsStyleId) is GraphicsStyle graphicsStyle && graphicsStyle.Name.Contains("Light Source"))
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
            SolidOrShellTessellationControls solidOrShellTessellationControls = new SolidOrShellTessellationControls
            {
                LevelOfDetail = userSetting.LevelOfDetail / 30.0,
                Accuracy = 0.1,
                MinAngleInTriangle = 0.0001,
                MinExternalAngleBetweenTriangles = 1.0
            };
            try
			{
				TriangulatedSolidOrShell triangulatedSolidOrShell = SolidUtils.TessellateSolidOrShell(solid, solidOrShellTessellationControls);
				int shellComponentCount = triangulatedSolidOrShell.ShellComponentCount;
				for (int i = 0; i < shellComponentCount; i++)
				{
					TriangulatedShellComponent shellComponent = triangulatedSolidOrShell.GetShellComponent(i);
                    ModelGeometry exportedGeometry = new ModelGeometry
                    {
                        Transform = this.transformationStack.Peek(),
                        Points = new List<XYZ>(shellComponent.VertexCount)
                    };
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
					Mesh mesh = face.Triangulate(userSetting.LevelOfDetail / 15.0);
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
		#endregion

		

		#region 贴花（Decal）
		/// <summary>
		/// 导出贴花（Decal）
		/// </summary>
		/// <param name="element"></param>
		private void ExportDecal(Element element)
		{
			if (IsDecalCurved(element))
			{
				ExportDecalCurved(element);
				return;
			}
			ExportDecalFlat(element);
		}


		/// <summary>
		/// 是不是元素贴花（Decal）？
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public bool IsElementDecal(Element element)
		{
			try
			{
				ElementId typeId = element.GetTypeId();
				ElementType E = documentStack.Peek().GetElement(typeId) as ElementType;

				//获取与元素引用的外部文件有关的信息。该对象引用的外部文件的类型。
				if (E != null)
				{
					if (E.GetExternalFileReference().ExternalFileReferenceType == ExternalFileReferenceType.Decal)
					{
						return true;
					}
				}
			}
			catch (Exception)
			{
			}
			return false;
		}



		/// <summary>
		/// 该贴花是弯曲的嘛？curved 弯曲的，弄弯的，倒弧角，曲线的。
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public bool IsDecalCurved(Element element)
		{
			//source 描述组成该元素的材料的元素
			GeometryElement source = element.get_Geometry(this.Geometry_Options);
			try
			{
				if (source.ToArray()[1].GetType() == typeof(Arc) && source.ToArray()[3].GetType() == typeof(Arc))
				{
					return true;
				}
			}
			catch (Exception)
			{
			}
			return false;
		}



		/// <summary>
		/// 导出有弧度弯曲的贴花DecalCurved
		/// </summary>
		/// <param name="element"></param>
		private void ExportDecalCurved(Element element)
		{
			ModelGeometry exportedGeometry = new ModelGeometry();
			exportedGeometry.Transform = this.transformationStack.Peek();
			GeometryElement expr_23 = element.get_Geometry(this.Geometry_Options);
			Arc arc = expr_23.ToArray()[1] as Arc;
			Curve arg_49_0 = expr_23.ToArray()[3] as Arc;
			XYZ[] array = arc.Tessellate().ToArray<XYZ>();
			XYZ[] array2 = arg_49_0.Tessellate().ToArray();
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

		/// <summary>
		/// 导出扁平的贴花（DecalFlat）adj. (flat) 光滑均匀的；（陆地）平坦的；（水面）平静的；无坡度的；扁的；
		/// </summary>
		/// <param name="element"></param>
		private void ExportDecalFlat(Element element)
		{
			ModelGeometry exportedGeometry = new ModelGeometry();
			exportedGeometry.Transform = transformationStack.Peek();
			GeometryElement arg_2F_0 = element.get_Geometry(this.Geometry_Options);
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
		#endregion



        #region 判断元素类型
        /// <summary>
        /// 元素是否小于给定的值，是就忽略掉。
        /// </summary>
        /// <param name="element"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private bool IsElementSmallerThan(Element element, double size)
		{
			BoundingBoxXYZ boundingBoxXYZ = element.get_BoundingBox(userSetting.MainView3D);
			return boundingBoxXYZ != null && boundingBoxXYZ.Enabled && (boundingBoxXYZ.Max - boundingBoxXYZ.Min).GetLength() < size;
		}


		/// <summary>
		/// 元素是结构建筑类别吗？
		/// 结构框架、金属栏杆、框架/竖梃、窗框竖框切割、栏杆扶手、窗台/盖板
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		private bool IsElementStructural(Element element)
		{
			Category category = element.Category;
			return category != null && (category.Id.IntegerValue.Equals(BuiltInCategory.OST_StructuralFraming)
				|| category.Id.IntegerValue.Equals(BuiltInCategory.OST_Railings)
				|| category.Id.IntegerValue.Equals(BuiltInCategory.OST_WindowsFrameMullionCut)
				|| category.Id.IntegerValue.Equals(BuiltInCategory.OST_WindowsFrameMullionProjection)
				|| category.Id.IntegerValue.Equals(BuiltInCategory.OST_WindowsSillHeadCut)
				|| category.Id.IntegerValue.Equals(BuiltInCategory.OST_WindowsSillHeadProjection)
				|| category.Id.IntegerValue.Equals(BuiltInCategory.OST_StairsRailing));
		}



		/// <summary>
		/// 元素是内部元素吗？家具、橱柜、电气设备、电气装置、家具系统、机械设备
		/// 卫浴装置、专用设备、风道末端 、电话设备、护理呼叫设备、安全设备、
		/// 通讯设备、火警设备、数据设备、灯具、常规模型、照明设备
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		private bool IsElementInInteriorCategory(Element element)
		{
			Category category = element.Category;
			return category != null && (category.Id.IntegerValue.Equals(BuiltInCategory.OST_Furniture) || 
				category.Id.IntegerValue.Equals(BuiltInCategory.OST_Casework) || 
				category.Id.IntegerValue.Equals(BuiltInCategory.OST_ElectricalEquipment) || 
				category.Id.IntegerValue.Equals(BuiltInCategory.OST_ElectricalFixtures) || 
				category.Id.IntegerValue.Equals(BuiltInCategory.OST_FurnitureSystems) || 
				category.Id.IntegerValue.Equals(BuiltInCategory.OST_MechanicalEquipment) || 
				category.Id.IntegerValue.Equals(BuiltInCategory.OST_PlumbingFixtures) || 
				category.Id.IntegerValue.Equals(BuiltInCategory.OST_SpecialityEquipment) || 
				category.Id.IntegerValue.Equals(BuiltInCategory.OST_DuctTerminal) || 
				category.Id.IntegerValue.Equals(BuiltInCategory.OST_TelephoneDevices) || 
				category.Id.IntegerValue.Equals(BuiltInCategory.OST_NurseCallDevices) || 
				category.Id.IntegerValue.Equals(BuiltInCategory.OST_SecurityDevices) || 
				category.Id.IntegerValue.Equals(BuiltInCategory.OST_CommunicationDevices) || 
				category.Id.IntegerValue.Equals(BuiltInCategory.OST_FireAlarmDevices) || 
				category.Id.IntegerValue.Equals(BuiltInCategory.OST_DataDevices) || 
				category.Id.IntegerValue.Equals(BuiltInCategory.OST_LightingDevices) || 
				category.Id.IntegerValue.Equals(BuiltInCategory.OST_GenericModel) || 
				category.Id.IntegerValue.Equals(BuiltInCategory.OST_LightingFixtures));
		}


		/// <summary>
		/// 元素是草稿类别嘛？
		/// 墙、屋顶、地形、柱、楼板、天花板、窗、栏杆扶手
		/// 、停车场、道路、坡道、幕墙系统、幕墙嵌板、幕墙竖梃
		/// 、结构基础、结构柱、结构框架、门
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		private bool IsElementInDraftCategory(Element element)
		{
			Category category = element.Category;
			if (category != null)
			{
				while (category.Parent != null)
				{
					category = category.Parent;
				}
				if (category.Id.IntegerValue.Equals(BuiltInCategory.OST_Gutter)
					|| category.Id.IntegerValue.Equals(BuiltInCategory.OST_RvtLinks)
					|| category.Id.IntegerValue.Equals(BuiltInCategory.OST_Walls)
					|| category.Id.IntegerValue.Equals(BuiltInCategory.OST_Roofs)
					|| category.Id.IntegerValue.Equals(BuiltInCategory.OST_Topography)
					|| category.Id.IntegerValue.Equals(BuiltInCategory.OST_Columns)
					|| category.Id.IntegerValue.Equals(BuiltInCategory.OST_Floors)
					|| category.Id.IntegerValue.Equals(BuiltInCategory.OST_Ceilings)
					|| category.Id.IntegerValue.Equals(BuiltInCategory.OST_Windows)
					|| category.Id.IntegerValue.Equals(BuiltInCategory.OST_Stairs)
					|| category.Id.IntegerValue.Equals(BuiltInCategory.OST_Parking)
					|| category.Id.IntegerValue.Equals(BuiltInCategory.OST_Roads)
					|| category.Id.IntegerValue.Equals(BuiltInCategory.OST_Ramps)
					|| category.Id.IntegerValue.Equals(BuiltInCategory.OST_Curtain_Systems)
					|| category.Id.IntegerValue.Equals(BuiltInCategory.OST_CurtainWallPanels)
					|| category.Id.IntegerValue.Equals(BuiltInCategory.OST_CurtainWallMullions)
					|| category.Id.IntegerValue.Equals(BuiltInCategory.OST_StructuralFoundation)
					|| category.Id.IntegerValue.Equals(BuiltInCategory.OST_StructuralColumns)
					|| category.Id.IntegerValue.Equals(BuiltInCategory.OST_StructuralFraming)
					|| category.Id.IntegerValue.Equals(BuiltInCategory.OST_Doors))
				{
					return true;
				}
			}
			return false;
		}



		/// <summary>
		/// 元素是双面类别嘛？
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		private bool IsElementInDoubleSidedCategory(Element element)
		{
			Category category = element.Category;
			if (category != null)
			{
				while (category.Parent != null)
				{
					category = category.Parent;
				}
				if (category.Id.IntegerValue.Equals(BuiltInCategory.OST_Ceilings))//是不是天花板？
				{
					return true;
				}
			}
			return false;
		}
        #endregion
    }

}