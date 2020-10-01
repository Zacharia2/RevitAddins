using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

namespace ExportDAE
{
	internal class ExportedGeometry
	{
		/// <summary>
		/// 坐标系
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
		/// 法线的分布
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
			IList<XYZ> list = new List<XYZ>(this.Points.Count);
			for (int i = 0; i < this.Indices.Count; i++)
			{
				XYZ xYZ = this.Points[this.Indices[i]];
				int hashCode = xYZ.GetHashCode();
				if (dictionary.ContainsKey(hashCode))
				{
					this.Indices[i] = dictionary[hashCode];
				}
				else
				{
					list.Add(xYZ);
					int value = list.Count - 1;
					dictionary[hashCode] = value;
					this.Indices[i] = value;
				}
			}
			this.Points = list;
		}

		/// <summary>
		/// 计算法线
		/// </summary>
		/// <param name="mirrored">镜像</param>
		public void CalculateNormals(bool mirrored)
		{
			this.Normals = new List<XYZ>(this.Points.Count);
			for (int i = 0; i < this.Points.Count; i++)
			{
				this.Normals.Add(XYZ.Zero);
			}
			for (int j = 0; j < this.Points.Count; j++)
			{
				for (int k = 0; k < this.Indices.Count; k += 3)
				{
					if (this.Indices[k + 0] == j || this.Indices[k + 1] == j || this.Indices[k + 2] == j)
					{
						XYZ xYZ;
						XYZ xYZ2;
						XYZ xYZ3;
						if (mirrored)
						{
							xYZ = this.Points[this.Indices[k + 0]];
							xYZ2 = this.Points[this.Indices[k + 2]];
							xYZ3 = this.Points[this.Indices[k + 1]];
						}
						else
						{
							xYZ = this.Points[this.Indices[k + 0]];
							xYZ2 = this.Points[this.Indices[k + 1]];
							xYZ3 = this.Points[this.Indices[k + 2]];
						}
						XYZ arg_139_0 = xYZ2 - xYZ;
						XYZ xYZ4 = xYZ3 - xYZ;
						XYZ xYZ5 = arg_139_0.CrossProduct(xYZ4);
						if (!xYZ5.IsZeroLength())
						{
							xYZ5 = xYZ5.Normalize();
							if (this.Normals[j].IsZeroLength())
							{
								IList<XYZ> normals = this.Normals;
								int index = j;
								normals[index] += xYZ5;
							}
							else if (this.Normals[j].DotProduct(xYZ5) > 0.1)
							{
								IList<XYZ> normals = this.Normals;
								int index = j;
								normals[index] += xYZ5;
							}
							else
							{
								this.Points.Add(this.Points[j]);
								this.Normals.Add(xYZ5);
								int value = this.Points.Count - 1;
								if (this.Indices[k + 0] == j)
								{
									this.Indices[k + 0] = value;
								}
								if (this.Indices[k + 1] == j)
								{
									this.Indices[k + 1] = value;
								}
								if (this.Indices[k + 2] == j)
								{
									this.Indices[k + 2] = value;
								}
							}
						}
					}
				}
			}
			for (int l = 0; l < this.Normals.Count; l++)
			{
				this.Normals[l] = this.Normals[l].Normalize();
			}
			this.DistributionOfNormals = 0;
		}

		/// <summary>
		/// 反转法线
		/// </summary>
		public void FlipNormals()
		{
			for (int i = 0; i < this.Normals.Count; i++)
			{
				this.Normals[i].Negate();
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
			int count = this.Points.Count;
			for (int i = 0; i < count; i++)
			{
				XYZ xYZ = this.Points[i];
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
			this.Uvs = new List<UV>(this.Points.Count);
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
			for (int i = 0; i < this.Points.Count; i++)
			{
				double num4 = 0.0;
				double num5 = 0.0;
				XYZ xYZ3 = this.Points[i];
				XYZ xYZ4 = this.Normals[i];
				if (xYZ4.IsZeroLength())
				{
					this.Uvs.Add(UV.Zero);
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
					this.Uvs.Add(new UV(num4, flipForizontal ? (1.0 - num5) : num5));
				}
			}
		}

		/// <summary>
		/// 使双面？？？
		/// </summary>
		public void MakeDoubleSided()
		{
			((List<XYZ>)this.Points).Capacity = this.Points.Count * 2;
			((List<XYZ>)this.Normals).Capacity = this.Normals.Count * 2;
			((List<UV>)this.Uvs).Capacity = this.Uvs.Count * 2;
			((List<int>)this.Indices).Capacity = this.Indices.Count * 2;
			int count = this.Points.Count;
			for (int i = 0; i < count; i++)
			{
				this.Points.Add(this.Points[i]);
			}
			int count2 = this.Normals.Count;
			for (int j = 0; j < count2; j++)
			{
				this.Normals.Add(this.Normals[j]);
			}
			int count3 = this.Uvs.Count;
			for (int k = 0; k < count3; k++)
			{
				this.Uvs.Add(this.Uvs[k]);
			}
			for (int l = 0; l < count2; l++)
			{
				this.Normals[l].Negate();
			}
			int count4 = this.Indices.Count;
			for (int m = 0; m < count4; m++)
			{
				this.Indices.Add(this.Indices[m]);
			}
			int value = 0;
			for (int n = 0; n < count4; n++)
			{
				int num = n % 3;
				if (num != 1)
				{
					if (num == 2)
					{
						this.Indices[n] = value;
					}
				}
				else
				{
					value = this.Indices[n];
					this.Indices[n] = this.Indices[n + 1];
				}
			}
		}
	}
}
