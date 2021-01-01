using System;
using System.Drawing;

namespace ExportDAE
{
	internal class ModelMaterial : IEquatable<ModelMaterial>
	{
		public string Name
		{
			get;
			set;
		}

		public Color Color
		{
			get;
			set;
		}

		/// <summary>
		/// 透明度
		/// </summary>
		public double Transparency
		{
			get;
			set;
		}

		/// <summary>
		/// Shininess 反射;反光度;光滑度;高光范围;
		/// </summary>
		public double Shininess
		{
			get;
			set;
		}

		/// <summary>
		/// 贴图路径
		/// </summary>
		public string TexturePath
		{
			get;
			set;
		}

		/// <summary>
		/// 纹理缩放U
		/// </summary>
		public double TextureScaleU
		{
			get;
			set;
		}

		/// <summary>
		/// 纹理缩放V
		/// </summary>
		public double TextureScaleV
		{
			get;
			set;
		}

		/// <summary>
		/// 纹理偏移 U
		/// </summary>
		public double TextureOffsetU
		{
			get;
			set;
		}

		/// <summary>
		/// 纹理偏移 V
		/// </summary>
		public double TextureOffsetV
		{
			get;
			set;
		}

		/// <summary>
		/// 纹理旋转角度
		/// </summary>
		public double TextureRotationAngle
		{
			get;
			set;
		}

		public ModelMaterial()
		{
			Name = "Default material";
			Color = Color.Gray;
			Transparency = 0.0;
			Shininess = 0.0;
			TexturePath = "";
			TextureScaleU = 1.0;
			TextureScaleV = 1.0;
			TextureOffsetU = 0.0;
			TextureOffsetV = 0.0;
			TextureRotationAngle = 0.0;
		}

		public bool Equals(ModelMaterial other)
		{
			return other != null && this.GetHashCode() == other.GetHashCode();
		}

		public override int GetHashCode()
		{
			return this.Name.GetHashCode()
				^ this.Color.R.GetHashCode()
				^ this.Color.G.GetHashCode()
				^ this.Color.B.GetHashCode()
				^ this.Transparency.GetHashCode()
				^ this.Shininess.GetHashCode()
				^ this.TexturePath.GetHashCode()
				^ this.TextureScaleU.GetHashCode()
				^ this.TextureScaleV.GetHashCode()
				^ this.TextureOffsetU.GetHashCode()
				^ this.TextureOffsetV.GetHashCode() 
				^ this.TextureRotationAngle.GetHashCode();
		}
	}
}
