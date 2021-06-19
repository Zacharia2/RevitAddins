using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

namespace ExportDAE
{
	internal class ModelGeometry
	{
		/// <summary>
		/// 主要用于物体的旋转移动和缩放
		/// </summary>
		public Transform Transform
		{
			get;
			set;
		}

		/// <summary>
		/// 坐标点
		/// </summary>
		public IList<XYZ> Points
		{
			get;
			set;
		}

		/// <summary>
		/// 法线
		/// </summary>
		public IList<XYZ> Normals
		{
			get;
			set;
		}

		/// <summary>
		/// UV
		/// </summary>
		public IList<UV> Uvs
		{
			get;
			set;
		}

		/// <summary>
		/// Index 引索
		/// </summary>
		public IList<int> Indices
		{
			get;
			set;
		}

		/// <summary>
		/// 法线的分布,AtEachPoint = 0:一个法向量被分配给多边形的每个顶点。OnePerFace = 1:只有一个公共法向量分配给Face/polymesh多边形网格。OnEachFacet = 2:多边形网格的每个面都有一个法向量。
		/// </summary>
		public DistributionOfNormals DistributionOfNormals
		{
			get;
			set;
		}

		/// <summary>
		/// 优化点
		/// </summary>
		public void OptimizePoints()
		{
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			IList<XYZ> list = new List<XYZ>(Points.Count);
			for (int i = 0; i < Indices.Count; i++)
			{
				XYZ xYZ = Points[Indices[i]];
				int hashCode = xYZ.GetHashCode();
				if (dictionary.ContainsKey(hashCode))
				{
					Indices[i] = dictionary[hashCode];
				}
				else
				{
					list.Add(xYZ);
					int value = list.Count - 1;
					dictionary[hashCode] = value;
					Indices[i] = value;
				}
			}
			Points = list;
		}

		/// <summary>
		/// 计算法线
		/// </summary>
		/// <param name="mirrored">镜像</param>
		public void CalculateNormals(bool mirrored)
		{
			Normals = new List<XYZ>(Points.Count);
			for (int i = 0; i < Points.Count; i++)
			{
				Normals.Add(XYZ.Zero);
			}
			for (int j = 0; j < Points.Count; j++)
			{
				for (int k = 0; k < Indices.Count; k += 3)
				{
					if (Indices[k + 0] == j || Indices[k + 1] == j || Indices[k + 2] == j)
					{
						XYZ xYZ;
						XYZ xYZ2;
						XYZ xYZ3;
						if (mirrored)
						{
							xYZ = Points[Indices[k + 0]];
							xYZ2 = Points[Indices[k + 2]];
							xYZ3 = Points[Indices[k + 1]];
						}
						else
						{
							xYZ = Points[Indices[k + 0]];
							xYZ2 = Points[Indices[k + 1]];
							xYZ3 = Points[Indices[k + 2]];
						}
						XYZ K = xYZ2 - xYZ;
						XYZ xYZ4 = xYZ3 - xYZ;
						XYZ xYZ5 = K.CrossProduct(xYZ4);
						if (!xYZ5.IsZeroLength())
						{
							xYZ5 = xYZ5.Normalize();
							if (Normals[j].IsZeroLength())
							{
								IList<XYZ> normals = Normals;
								int index = j;
								normals[index] += xYZ5;
							}
							else if (Normals[j].DotProduct(xYZ5) > 0.1)
							{
								IList<XYZ> normals = Normals;
								int index = j;
								normals[index] += xYZ5;
							}
							else
							{
								Points.Add(Points[j]);
								Normals.Add(xYZ5);
								int value = Points.Count - 1;
								if (Indices[k + 0] == j)
								{
									Indices[k + 0] = value;
								}
								if (Indices[k + 1] == j)
								{
									Indices[k + 1] = value;
								}
								if (Indices[k + 2] == j)
								{
									Indices[k + 2] = value;
								}
							}
						}
					}
				}
			}
			for (int l = 0; l < Normals.Count; l++)
			{
				Normals[l] = Normals[l].Normalize();
			}
			DistributionOfNormals = 0;
		}

		/// <summary>
		/// 反转法线
		/// </summary>
		public void FlipNormals()
		{
			for (int i = 0; i < Normals.Count; i++)
			{
				Normals[i].Negate();
			}
		}

		/// <summary>
		/// 计算边界框
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		public void CalculateBoundingBox(ref XYZ min, ref XYZ max)
		{
			double num = 3.4028234663852886E+38;
			double num2 = 3.4028234663852886E+38;
			double num3 = 3.4028234663852886E+38;
			double num4 = -3.4028234663852886E+38;
			double num5 = -3.4028234663852886E+38;
			double num6 = -3.4028234663852886E+38;
			int count = Points.Count;
			for (int i = 0; i < count; i++)
			{
				XYZ xYZ = Points[i];
				if (xYZ.X < num)
				{
					num = xYZ.X;
				}
				if (xYZ.Y < num2)
				{
					num2 = xYZ.Y;
				}
				if (xYZ.Z < num3)
				{
					num3 = xYZ.Z;
				}
				if (xYZ.X > num4)
				{
					num4 = xYZ.X;
				}
				if (xYZ.Y > num5)
				{
					num5 = xYZ.Y;
				}
				if (xYZ.Z > num6)
				{
					num6 = xYZ.Z;
				}
			}
			min = new XYZ(num, num2, num3);
			max = new XYZ(num4, num5, num6);
		}

		/// <summary>
		/// 计算UV
		/// </summary>
		/// <param name="keepDensity"></param>
		/// <param name="flipForizontal"></param>
		public void CalculateUVs(bool keepDensity = true, bool flipForizontal = false)
		{
			Uvs = new List<UV>(Points.Count);
			XYZ xYZ = new XYZ();
			XYZ xYZ2 = new XYZ();
			this.CalculateBoundingBox(ref xYZ, ref xYZ2);
			double num = xYZ2.X - xYZ.X;
			double num2 = xYZ2.Y - xYZ.Y;
			double num3 = xYZ2.Z - xYZ.Z;
			if (keepDensity)
			{
				num2 = (num = (num3 = 1.0));
			}
			for (int i = 0; i < Points.Count; i++)
			{
				double num4 = 0.0;
				double num5 = 0.0;
				XYZ xYZ3 = Points[i];
				XYZ xYZ4 = Normals[i];
				if (xYZ4.IsZeroLength())
				{
					Uvs.Add(UV.Zero);
				}
				else
				{
					if (Math.Abs(xYZ4.X) >= Math.Abs(xYZ4.Y) && Math.Abs(xYZ4.X) >= Math.Abs(xYZ4.Z))
					{
						num4 = xYZ4.X / Math.Abs(xYZ4.X) * ((xYZ3.Z - xYZ.Z) / num3);
						num5 = -((xYZ3.Y - xYZ.Y) / num2);
					}
					else if (Math.Abs(xYZ4.Z) >= Math.Abs(xYZ4.Y) && Math.Abs(xYZ4.Z) >= Math.Abs(xYZ4.X))
					{
						num4 = -(xYZ4.Z / Math.Abs(xYZ4.Z)) * ((xYZ3.X - xYZ.X) / num);
						num5 = -((xYZ3.Y - xYZ.Y) / num2);
					}
					else if (Math.Abs(xYZ4.Y) >= Math.Abs(xYZ4.Z) && Math.Abs(xYZ4.Y) >= Math.Abs(xYZ4.X))
					{
						num4 = xYZ4.Y / Math.Abs(xYZ4.Y) * ((xYZ3.X - xYZ.X) / num);
						num5 = -((xYZ3.Z - xYZ.Z) / num3);
					}
					Uvs.Add(new UV(num4, flipForizontal ? (1.0 - num5) : num5));
				}
			}
		}

		/// <summary>
		/// 当前实体数据变成两倍，使单面实体变成双面实体。
		/// </summary>
		public void MakeDoubleSided()
		{
			((List<XYZ>)Points).Capacity = Points.Count * 2;
			((List<XYZ>)Normals).Capacity = this.Normals.Count * 2;
			((List<UV>)Uvs).Capacity = this.Uvs.Count * 2;
			((List<int>)Indices).Capacity = this.Indices.Count * 2;
			int count = Points.Count;
			for (int i = 0; i < count; i++)
			{
				Points.Add(Points[i]);
			}
			int count2 = Normals.Count;
			for (int j = 0; j < count2; j++)
			{
				Normals.Add(Normals[j]);
			}
			int count3 = Uvs.Count;
			for (int k = 0; k < count3; k++)
			{
				Uvs.Add(Uvs[k]);
			}
			for (int l = 0; l < count2; l++)
			{
				Normals[l].Negate();
			}
			int count4 = Indices.Count;
			for (int m = 0; m < count4; m++)
			{
				Indices.Add(Indices[m]);
			}
			int value = 0;
			for (int n = 0; n < count4; n++)
			{
				int num = n % 3;
				if (num != 1)
				{
					if (num == 2)
					{
						Indices[n] = value;
					}
				}
				else
				{
					value = Indices[n];
					Indices[n] = Indices[n + 1];
				}
			}
		}
	}
}
