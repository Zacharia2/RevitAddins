using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ExportDAE
{
	internal class ColladaWriter
	{
		[CompilerGenerated]
		[Serializable]
		private sealed class Inner
		{
			public static readonly Inner T = new Inner();

			public static Func<ModelGeometry, int> T__17_0;

			public static Func<ModelGeometry, int> T__18_0;

			public static Func<ModelGeometry, int> T__19_0;

			public static Func<ModelGeometry, int> T__21_0;

			public static Func<ModelGeometry, int> T__21_1;

			public static Func<ModelGeometry, int> T__21_2;

			public static Func<ModelMaterial, string> T__24_0;

			/// <summary>
			/// 返回顶点数
			/// </summary>
			internal int b__17_0(ModelGeometry item)
			{
				return item.Points.Count;
			}

			/// <summary>
			/// 返回法线数量
			/// </summary>
			/// <param name="item"></param>
			/// <returns></returns>
			internal int b__18_0(ModelGeometry item)
			{
				return item.Normals.Count;
			}


			/// <summary>
			/// 返回顶点数量
			/// </summary>
			/// <param name="item"></param>
			/// <returns></returns>
			internal int b__19_0(ModelGeometry item)
			{
				return item.Points.Count;
			}

			internal int b__21_0(ModelGeometry item)
			{
				return item.Indices.Count;
			}

			internal int b__21_1(ModelGeometry item)
			{
				return item.Points.Count;
			}

			internal int b__21_2(ModelGeometry item)
			{
				return item.Normals.Count;
			}

			internal string b__24_0(ModelMaterial o)
			{
				return o.TexturePath;
			}

            public Inner()
            {
            }
        }

		private Dictionary<Tuple<Document, ElementId>, ModelMaterial> documentAndMaterialIdToExportedMaterial = new Dictionary<Tuple<Document, ElementId>, ModelMaterial>();
		private Dictionary<Tuple<Document, ElementId>, IList<ModelGeometry>> documentAndMaterialIdToGeometries = new Dictionary<Tuple<Document, ElementId>, IList<ModelGeometry>>();
		private StreamWriter streamWriter;
		private StringBuilder sb = new StringBuilder();

	public ColladaWriter(Dictionary<Tuple<Document, ElementId>, ModelMaterial> documentAndMaterialIdToExportedMaterial, Dictionary<Tuple<Document, ElementId>, IList<ModelGeometry>> documentAndMaterialIdToGeometries)
	{
		this.documentAndMaterialIdToExportedMaterial = documentAndMaterialIdToExportedMaterial;
		this.documentAndMaterialIdToGeometries = documentAndMaterialIdToGeometries;
		this.sb.Capacity = 1048576;
	}

	public bool Write(string filePath)
	{
		GC.Collect();
		if (!this.OpenStream(filePath))
		{
			return false;
		}
		this.WriteXmlColladaBegin();






		this.WriteXmlAsset();













		this.WriteXmlLibraryGeometries();
		this.WriteXmlLibraryImages();
		this.WriteXmlLibraryMaterials();
		this.WriteXmlLibraryEffects();
		this.WriteXmlLibraryVisualScenes();
		this.WriteXmlColladaEnd();















		this.CloseStream();
		return true;
	}

	private bool OpenStream(string filePath)
	{
		this.streamWriter = new StreamWriter(filePath, false, Encoding.UTF8, 11048576);
		return this.streamWriter != null;
	}

	private void CloseStream()
	{
		this.streamWriter.Close();
	}

	private void WriteXmlColladaBegin()
	{
		this.streamWriter.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
		this.streamWriter.Write("<COLLADA xmlns=\"http://www.collada.org/2005/11/COLLADASchema\" version=\"1.4.1\">\n");
	}

	private void WriteXmlColladaEnd()
	{
		this.streamWriter.Write("</COLLADA>\n");
	}

	/// <summary>
	/// 开始的时间及一些信息。
	/// </summary>
	private void WriteXmlAsset()
	{
		this.streamWriter.Write("<asset>\n");
		this.streamWriter.Write("<contributor>\n");
		this.streamWriter.Write("  <authoring_tool>Revit To Lumion bridge</authoring_tool>\n");
		this.streamWriter.Write("</contributor>\n");
		this.streamWriter.Write("<created>" + DateTime.Now.ToString(CultureInfo.InvariantCulture) + "</created>\n");
		this.streamWriter.Write("<unit name=\"feet\" meter=\"0.3048\"/>\n");
		this.streamWriter.Write("<up_axis>Z_UP</up_axis>\n");
		this.streamWriter.Write("</asset>\n");
	}
		//TODO 
	private void WriteXmlLibraryGeometriesSeparate()
	{
		this.WriteXmlLibraryGeometriesBegin();
			//迭代documentAndMaterialIdToGeometries
			foreach (KeyValuePair<Tuple<Document, ElementId>, IList<ModelGeometry>> current in this.documentAndMaterialIdToGeometries)
		{
				//将文档，及元素ID赋值给key
			Tuple<Document, ElementId> key = current.Key;
				//迭代current.Value也即是ExportedGeometry
				foreach (ModelGeometry current2 in current.Value)
			{
					//新建类型为ExportedGeometry的列表
					IList<ModelGeometry> list = new List<ModelGeometry>();
					//将导出模型加入到列表中。
				list.Add(current2);
					//判断列表非空并大于零
				if (list != null && list.Count > 0)
				{

					ModelMaterial exportedMaterial = this.documentAndMaterialIdToExportedMaterial[key];
					this.sb.Clear();
					this.WriteXmlGeometryBegin(current2.GetHashCode(), exportedMaterial);

					this.WriteXmlGeometrySourcePositions(current2.GetHashCode(), list);
					this.WriteXmlGeometrySourceNormals(current2.GetHashCode(), list);
					this.WriteXmlGeometrySourceMap(current2.GetHashCode(), exportedMaterial, list);
					this.WriteXmlGeometryVertices(current2.GetHashCode());
					this.WriteXmlGeometryTrianglesWithMap(current2.GetHashCode(), list);

					this.WriteXmlGeometryEnd();
					this.streamWriter.Write(this.sb.ToString());
				}
				list.Clear();
			}
		}
		this.WriteXmlLibraryGeometriesEnd();
	}
		/// <summary>
		/// 
		/// </summary>
	private void WriteXmlLibraryGeometries()
	{
			
		this.WriteXmlLibraryGeometriesBegin();

		foreach (KeyValuePair<Tuple<Document, ElementId>, IList<ModelGeometry>> current in this.documentAndMaterialIdToGeometries)
		{
			Tuple<Document, ElementId> key = current.Key;
			if (current.Value != null && current.Value.Count > 0)
			{
				ModelMaterial exportedMaterial = this.documentAndMaterialIdToExportedMaterial[key];
				this.sb.Clear();
				this.WriteXmlGeometryBegin(key.GetHashCode(), exportedMaterial);
				this.WriteXmlGeometrySourcePositions(key.GetHashCode(), current.Value);
				this.WriteXmlGeometrySourceNormals(key.GetHashCode(), current.Value);
				this.WriteXmlGeometrySourceMap(key.GetHashCode(), exportedMaterial, current.Value);
				this.WriteXmlGeometryVertices(key.GetHashCode());
				this.WriteXmlGeometryTrianglesWithMap(key.GetHashCode(), current.Value);
				this.WriteXmlGeometryEnd();
				int val = 10485760;
				while (this.sb.Length > 0)
				{
					int length = Math.Min(val, this.sb.Length);
					this.streamWriter.Write(this.sb.ToString(0, length));
					this.sb.Remove(0, length);
				}
			}
		}
		this.WriteXmlLibraryGeometriesEnd();
	}

		/// <summary>
		/// 开始标签:library_geometries
		/// </summary>
		private void WriteXmlLibraryGeometriesBegin()
	{
		this.streamWriter.Write("<library_geometries>\n");
	}

		/// <summary>
		/// 结束标签:library_geometries
		/// </summary>
		private void WriteXmlLibraryGeometriesEnd()
	{
		this.streamWriter.Write("</library_geometries>\n");
	}

	private void WriteXmlGeometryBegin(int documentAndMaterialIdHash, ModelMaterial exportedMaterial)
	{
		this.sb.AppendFormat("<geometry id=\"geom-{0}\" name=\"{1}\">\n", documentAndMaterialIdHash, this.Utf16ToUtf8(exportedMaterial.Name));
		this.sb.Append("<mesh>\n");
	}

	private void WriteXmlGeometryEnd()
	{
		this.sb.Append("</mesh>\n");
		this.sb.Append("</geometry>\n");
	}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="documentAndMaterialIdHash">定义Source的唯一ID</param>
		/// <param name="geometries"></param>
	private void WriteXmlGeometrySourcePositions(int documentAndMaterialIdHash, IList<ModelGeometry> geometries)
	{
		Func<ModelGeometry, int> arg_20_1;
		if ((arg_20_1 = Inner.T__17_0) == null)
		{
			arg_20_1 = (Inner.T__17_0 = new Func<ModelGeometry, int>(Inner.T.b__17_0));
		}
		//计算顶点数
		int num = geometries.Sum(arg_20_1);
		//source 节点属性
		this.sb.AppendFormat("<source id=\"geom-{0}-positions\">\n", documentAndMaterialIdHash);
		//source节点下的float_array节点属性
		this.sb.AppendFormat("<float_array id=\"geom-{0}-positions-array\" count=\"{1}\">\n", documentAndMaterialIdHash, num * 3);

	/*	定义一个范围，在范围结束时处理对象。
	场景：
	当在某个代码段中使用了类的实例，而希望无论因为什么原因，只要离开了这个代码段就自动调用这个类实例的Dispose。
	要达到这样的目的，用try...catch来捕捉异常也是可以的，但用using也很方便。//*/
			using (IEnumerator<ModelGeometry> enumerator = geometries.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				ModelGeometry geometry = enumerator.Current;
					//恒等变换不会改变它所应用的点或向量。
				if (!geometry.Transform.IsIdentity)
				{
						//Parallel.For方法，主要用于处理针对数组元素的并行操作(数据的并行) fromInclusive:
						//     开始索引（含）。
						//
						//   toExclusive:
						//     结束索引（不含）。
						//
						//   body:
						//     将为每个迭代调用一次的委托。
						//
						// 返回结果:
						//     包含有关已完成的循环部分的信息的结构。
						Parallel.For(0, geometry.Points.Count, delegate (int iPoint)
					{
						//将变换应用于点并返回结果。
						/*Revit 提供了Transform类来做二次开发时的坐标转换。 你可以给Transform对象进行赋值，构造一个变换矩阵。
						然后使用这个变化矩阵把给定的坐标点的坐标转成目标坐标系。

						初始化Transform，设置其目标坐标系的三个方向向量BasisX，BasisY，BasisZ的值。
						获得转换矩阵后，可以用Transform.OfPOint（XYZ pt) 把目标点坐标转换到新坐标系的坐标。
						也可以用Transform.OfVector(XYZ vector) 对向量进行坐标转换。

						注：这里转换后点坐标或向量的单位与输入点或向量的单位相同。默认Revit API所使用的坐标单位都是英尺。

						当然你可以使用第三方的代码做纯粹的坐标转换功能。

						Revit API的一些函数需要坐标转换时，都需要Transform类型的。 */
						geometry.Points[iPoint] = geometry.Transform.OfPoint(geometry.Points[iPoint]);
					});
				}
				for (int i = 0; i < geometry.Points.Count; i++)
				{
					XYZ xYZ = geometry.Points[i];
						///  参数:
						//   provider:
						//     一个提供区域性特定的格式设置信息的对象。
						//
						//   format:
						//     复合格式字符串（请参见“备注”）。
						//
						//   args:
						//     要设置其格式的对象的数组。
						//
						// 返回结果:
						//     完成追加操作后对此实例的引用。 完成追加操作后，此实例包含执行该操作之前已存在的任何数据，
						//     并且有一个 format 的副本作为后缀，其中任何格式规范都由相应对象参数的字符串表示形式替换。

						//{0:0.###} 格式化字符串
						this.sb.AppendFormat(CultureInfo.InvariantCulture.NumberFormat, "{0:0.###} {1:0.###} {2:0.###} ", new object[]
					{
							xYZ.X,
							xYZ.Y,
							xYZ.Z
					});
				}
			}
		}
		//float_array 节点结束
		this.sb.Append("</float_array>\n");
		this.sb.Append("<technique_common>\n");
			//访问器节点
		this.sb.AppendFormat("<accessor source=\"#geom-{0}-positions-array\" count=\"{1}\" stride=\"3\">\n", documentAndMaterialIdHash, num);
		this.sb.Append("<param name=\"X\" type=\"float\"/>\n");
		this.sb.Append("<param name=\"Y\" type=\"float\"/>\n");
		this.sb.Append("<param name=\"Z\" type=\"float\"/>\n");
			//访问器节点结束
		this.sb.Append("</accessor>\n");
		this.sb.Append("</technique_common>\n");
		this.sb.Append("</source>\n");
	}

	private void WriteXmlGeometrySourceNormals(int documentAndMaterialIdHash, IList<ModelGeometry> geometries)
	{
		Func<ModelGeometry, int> arg_20_1;
		if ((arg_20_1 = Inner.T__18_0) == null)
		{
			arg_20_1 = (Inner.T__18_0 = new Func<ModelGeometry, int>(Inner.T.b__18_0));
		}
		//计算法线数量
		int num = geometries.Sum(arg_20_1);
			//source节点属性
		this.sb.AppendFormat("<source id=\"geom-{0}-normals\">\n", documentAndMaterialIdHash);
			//float_array节点属性
		this.sb.AppendFormat("<float_array id=\"geom-{0}-normals-array\" count=\"{1}\">\n", documentAndMaterialIdHash, num * 3);
		using (IEnumerator<ModelGeometry> enumerator = geometries.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				ModelGeometry geometry = enumerator.Current;
				if (!geometry.Transform.IsIdentity)
				{
					Parallel.For(0, geometry.Normals.Count, delegate (int iNormal)
					{
						geometry.Normals[iNormal] = geometry.Transform.OfVector(geometry.Normals[iNormal]);
					});
				}
				for (int i = 0; i < geometry.Normals.Count; i++)
				{
					XYZ xYZ = geometry.Normals[i];
					this.sb.AppendFormat(CultureInfo.InvariantCulture.NumberFormat, "{0:0.###} {1:0.###} {2:0.###} ", new object[]
					{
							xYZ.X,
							xYZ.Y,
							xYZ.Z
					});
				}
			}
		}
		this.sb.Append("</float_array>\n");
		this.sb.Append("<technique_common>\n");
		this.sb.AppendFormat("<accessor source=\"#geom-{0}-normals-array\" count=\"{1}\" stride=\"3\">\n", documentAndMaterialIdHash, num);
		this.sb.Append("<param name=\"X\" type=\"float\"/>\n");
		this.sb.Append("<param name=\"Y\" type=\"float\"/>\n");
		this.sb.Append("<param name=\"Z\" type=\"float\"/>\n");
		this.sb.Append("</accessor>\n");
		this.sb.Append("</technique_common>\n");
		this.sb.Append("</source>\n");
	}

	private void WriteXmlGeometrySourceMap(int documentAndMaterialIdHash, ModelMaterial exportedMaterial, IList<ModelGeometry> geometries)
	{
		Func<ModelGeometry, int> arg_20_1;
		if ((arg_20_1 = Inner.T__19_0) == null)
		{
			arg_20_1 = (Inner.T__19_0 = new Func<ModelGeometry, int>(Inner.T.b__19_0));
		}
		//计算points（顶点）数
		int num = geometries.Sum(arg_20_1);
		this.sb.AppendFormat("<source id=\"geom-{0}-map\">\n", documentAndMaterialIdHash);
		this.sb.AppendFormat("<float_array id=\"geom-{0}-map-array\" count=\"{1}\">\n", documentAndMaterialIdHash, num * 2);
			//迭代geometries
			foreach (ModelGeometry current in geometries)
		{
			for (int i = 0; i < current.Uvs.Count; i++)
			{
				UV uV = current.Uvs[i];
				double num2 = uV.U;
				double num3 = uV.V;
				if (Math.Abs(exportedMaterial.TextureRotationAngle) > 1.0)
				{
					double textureOffsetU = exportedMaterial.TextureOffsetU;
					double textureOffsetV = exportedMaterial.TextureOffsetV;
					num2 = Math.Cos(exportedMaterial.TextureRotationAngle) * (uV.U - textureOffsetU) - Math.Sin(exportedMaterial.TextureRotationAngle) * (uV.V - textureOffsetV) + textureOffsetU;
					num3 = Math.Sin(exportedMaterial.TextureRotationAngle) * (uV.U - textureOffsetU) + Math.Cos(exportedMaterial.TextureRotationAngle) * (uV.V - textureOffsetV) + textureOffsetV;
				}
				num2 = (num2 - exportedMaterial.TextureOffsetU) * exportedMaterial.TextureScaleU;
				num3 = (num3 - exportedMaterial.TextureOffsetV) * exportedMaterial.TextureScaleV;
				current.Uvs[i] = new UV(num2, num3);
			}
			for (int j = 0; j < current.Uvs.Count; j++)
			{
				UV uV2 = current.Uvs[j];
				this.sb.AppendFormat(CultureInfo.InvariantCulture.NumberFormat, "{0:0.##} {1:0.##} ", new object[]
				{
						uV2.U,
						uV2.V
				});
			}
		}
		this.sb.Append("</float_array>\n");
		this.sb.Append("<technique_common>\n");
		this.sb.AppendFormat("<accessor source=\"#geom-{0}-map-array\" count=\"{1}\" stride=\"2\">\n", documentAndMaterialIdHash, num);
		this.sb.Append("<param name=\"S\" type=\"float\"/>\n");
		this.sb.Append("<param name=\"T\" type=\"float\"/>\n");
		this.sb.Append("</accessor>\n");
		this.sb.Append("</technique_common>\n");
		this.sb.Append("</source>\n");
	}

	private void WriteXmlGeometryVertices(int documentAndMaterialIdHash)
	{
		this.sb.AppendFormat("<vertices id=\"geom-{0}-vertices\">\n", documentAndMaterialIdHash);
		this.sb.AppendFormat("<input semantic=\"POSITION\" source=\"#geom-{0}-positions\"/>\n", documentAndMaterialIdHash);
		this.sb.Append("</vertices>\n");
	}

	private void WriteXmlGeometryTrianglesWithMap(int documentAndMaterialIdHash, IList<ModelGeometry> geometries)
	{
		Func<ModelGeometry, int> arg_20_1;
		if ((arg_20_1 = Inner.T__21_0) == null)
		{
			arg_20_1 = (Inner.T__21_0 = new Func<ModelGeometry, int>(Inner.T.b__21_0));
		}
		int num = geometries.Sum(arg_20_1) / 3;
		this.sb.AppendFormat("<triangles count=\"{0}\" material=\"material-{1}\">\n", num, documentAndMaterialIdHash);
		Func<ModelGeometry, int> arg_65_1;
		if ((arg_65_1 = Inner.T__21_1) == null)
		{
			arg_65_1 = (Inner.T__21_1 = new Func<ModelGeometry, int>(Inner.T.b__21_1));
		}
		int arg_91_0 = geometries.Sum(arg_65_1);
		Func<ModelGeometry, int> arg_8A_1;
		if ((arg_8A_1 = Inner.T__21_2) == null)
		{
			arg_8A_1 = (Inner.T__21_2 = new Func<ModelGeometry, int>(Inner.T.b__21_2));
		}
		int num2 = geometries.Sum(arg_8A_1);
		if (arg_91_0 == num2)
		{
			this.sb.AppendFormat("<input offset=\"0\" semantic=\"VERTEX\" source=\"#geom-{0}-vertices\"/>\n", documentAndMaterialIdHash);
			this.sb.AppendFormat("<input offset=\"0\" semantic=\"TEXCOORD\" source=\"#geom-{0}-map\" set=\"0\"/>\n", documentAndMaterialIdHash);
			this.sb.AppendFormat("<input offset=\"0\" semantic=\"NORMAL\" source=\"#geom-{0}-normals\"/>\n", documentAndMaterialIdHash);
			this.sb.Append("<p>\n");
			int num3 = 0;
			using (IEnumerator<ModelGeometry> enumerator = geometries.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ModelGeometry current = enumerator.Current;
					for (int i = 0; i < current.Indices.Count; i++)
					{
						this.sb.AppendFormat("{0} ", current.Indices[i] + num3);
					}
					num3 += current.Points.Count;
				}
				goto IL_2D3;
			}
		}
		this.sb.AppendFormat("<input offset=\"0\" semantic=\"VERTEX\" source=\"#geom-{0}-vertices\"/>\n", documentAndMaterialIdHash);
		this.sb.AppendFormat("<input offset=\"0\" semantic=\"TEXCOORD\" source=\"#geom-{0}-map\" set=\"0\"/>\n", documentAndMaterialIdHash);
		this.sb.AppendFormat("<input offset=\"1\" semantic=\"NORMAL\" source=\"#geom-{0}-normals\"/>\n", documentAndMaterialIdHash);
		this.sb.Append("<p>\n");
		int num4 = 0;
		int num5 = 0;
		foreach (ModelGeometry current2 in geometries)
		{
			switch (current2.DistributionOfNormals)
			{
				case 0:
					for (int j = 0; j < current2.Indices.Count; j++)
					{
						this.sb.AppendFormat("{0} {1} ", current2.Indices[j] + num4, current2.Indices[j] + num5);
					}
					break;
				case (DistributionOfNormals)1:
				case (DistributionOfNormals)2:
					for (int k = 0; k < current2.Indices.Count; k++)
					{
						this.sb.AppendFormat("{0} {1} ", current2.Indices[k] + num4, num5);
					}
					break;
			}
			num4 += current2.Points.Count;
			num5 += current2.Normals.Count;
		}
		IL_2D3:
		this.sb.Append("</p>\n");
		this.sb.Append("</triangles>\n");
	}

	private void WriteXmlLibraryMaterials()
	{
		this.streamWriter.Write("<library_materials>\n");
		foreach (KeyValuePair<Tuple<Document, ElementId>, ModelMaterial> current in this.documentAndMaterialIdToExportedMaterial)
		{
			int hashCode = current.Key.GetHashCode();
			ModelMaterial value = current.Value;
			string text = this.Utf16ToUtf8(value.Name);
			this.streamWriter.Write(string.Concat(new string[]
			{
					"<material id=\"material-",
					hashCode.ToString(),
					"\" name=\"",
					text,
					"\">\n"
			}));
			this.streamWriter.Write("<instance_effect url=\"#effect-" + hashCode.ToString() + "\" />\n");
			this.streamWriter.Write("</material>\n");
		}
		this.streamWriter.Write("</library_materials>\n");
	}

	private void WriteXmlLibraryEffects()
	{
		this.streamWriter.Write("<library_effects>\n");
		foreach (KeyValuePair<Tuple<Document, ElementId>, ModelMaterial> current in this.documentAndMaterialIdToExportedMaterial)
		{
			int hashCode = current.Key.GetHashCode();
			ModelMaterial value = current.Value;
			this.streamWriter.Write(string.Concat(new string[]
			{
					"<effect id=\"effect-",
					hashCode.ToString(),
					"\" name=\"",
					this.Utf16ToUtf8(value.Name),
					"\">\n"
			}));
			this.streamWriter.Write("<profile_COMMON>\n");
			this.streamWriter.Write("<technique sid=\"common\">\n");
			this.streamWriter.Write("<phong>\n");
			this.streamWriter.Write("<ambient>\n");
			this.streamWriter.Write("<color>0.1 0.1 0.1 1.0</color>\n");
			this.streamWriter.Write("</ambient>\n");
			this.streamWriter.Write("<diffuse>\n");
			if (value.TexturePath.Length > 0)
			{
				this.streamWriter.Write("<texture texture=\"image-" + value.TexturePath.GetHashCode() + "\" texcoord=\"CHANNEL0\"/>\n");
			}
			else
			{
				this.streamWriter.Write(string.Concat(new string[]
				{
						"<color>",
						Convert.ToString((double)value.Color.R / 255.0, CultureInfo.InvariantCulture.NumberFormat),
						" ",
						Convert.ToString((double)value.Color.G / 255.0, CultureInfo.InvariantCulture.NumberFormat),
						" ",
						Convert.ToString((double)value.Color.B / 255.0, CultureInfo.InvariantCulture.NumberFormat),
						" 1.0</color>\n"
				}));
			}
			this.streamWriter.Write("</diffuse>\n");
			this.streamWriter.Write("<specular>\n");
			this.streamWriter.Write("<color>1.0 1.0 1.0 1.0</color>\n");
			this.streamWriter.Write("</specular>\n");
			this.streamWriter.Write("<shininess>\n");
			this.streamWriter.Write("<float>" + Convert.ToString(value.Shininess, CultureInfo.InvariantCulture.NumberFormat) + "</float>\n");
			this.streamWriter.Write("</shininess>\n");
			this.streamWriter.Write("<reflective>\n");
			this.streamWriter.Write("<color>0 0 0 1.0</color>\n");
			this.streamWriter.Write("</reflective>\n");
			this.streamWriter.Write("<reflectivity>\n");
			this.streamWriter.Write("<float>1.0</float>\n");
			this.streamWriter.Write("</reflectivity>\n");
			this.streamWriter.Write("<transparent opaque=\"RGB_ZERO\">\n");
			this.streamWriter.Write("<color>1.0 1.0 1.0 1.0</color>\n");
			this.streamWriter.Write("</transparent>\n");
			this.streamWriter.Write("<transparency>\n");
			this.streamWriter.Write("<float>" + Convert.ToString(value.Transparency, CultureInfo.InvariantCulture.NumberFormat) + "</float>\n");
			this.streamWriter.Write("</transparency>\n");
			this.streamWriter.Write("</phong>\n");
			this.streamWriter.Write("</technique>\n");
			this.streamWriter.Write("</profile_COMMON>\n");
			this.streamWriter.Write("</effect>\n");
		}
		this.streamWriter.Write("</library_effects>\n");
	}

	private void WriteXmlLibraryImages()
	{
		this.streamWriter.Write("<library_images>\n");
		IEnumerable<ModelMaterial> arg_3A_0 = this.documentAndMaterialIdToExportedMaterial.Values;
		Func<ModelMaterial, string> arg_3A_1;
		if ((arg_3A_1 = Inner.T__24_0) == null)
		{
			arg_3A_1 = (Inner.T__24_0 = new Func<ModelMaterial, string>(Inner.T.b__24_0));
		}
		foreach (string current in arg_3A_0.Select(arg_3A_1).Distinct<string>())
		{
			if (!(current == ""))
			{
				this.streamWriter.Write("<image id=\"image-" + current.GetHashCode() + "\">\n");
				this.streamWriter.Write("<init_from><![CDATA[" + this.Utf16ToUtf8(current) + "]]></init_from>\n");
				this.streamWriter.Write("</image>\n");
			}
		}
		this.streamWriter.Write("</library_images>\n");
	}

	public void WriteXmlLibraryVisualScenes()
	{
		this.streamWriter.Write("<library_visual_scenes>\n");
		this.streamWriter.Write("<visual_scene id=\"Revit_project\">\n");
		foreach (Tuple<Document, ElementId> current in this.documentAndMaterialIdToGeometries.Keys)
		{
			this.streamWriter.Write("<node id=\"node-" + current.GetHashCode().ToString() + "\" name=\"elementName\">\n");
			this.streamWriter.Write("<instance_geometry url=\"#geom-" + current.GetHashCode().ToString() + "\">\n");
			this.streamWriter.Write("<bind_material>\n");
			this.streamWriter.Write("<technique_common>\n");
			this.streamWriter.Write(string.Concat(new string[]
			{
					"<instance_material target=\"#material-",
					current.GetHashCode().ToString(),
					"\" symbol=\"material-",
					current.GetHashCode().ToString(),
					"\" >\n"
			}));
			this.streamWriter.Write("</instance_material>\n");
			this.streamWriter.Write("</technique_common>\n");
			this.streamWriter.Write("</bind_material>\n");
			this.streamWriter.Write("</instance_geometry>\n");
			this.streamWriter.Write("</node>\n");
		}
		this.streamWriter.Write("</visual_scene>\n");
		this.streamWriter.Write("</library_visual_scenes>\n");
		this.streamWriter.Write("<scene>\n");
		this.streamWriter.Write("<instance_visual_scene url=\"#Revit_project\"/>\n");
		this.streamWriter.Write("</scene>\n");
	}

	public string Utf16ToUtf8(string utf16String)
	{
		return utf16String;
	}
}
}
