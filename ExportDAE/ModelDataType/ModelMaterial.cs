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

		public double Transparency
		{
			get;
			set;
		}

		public double Shininess
		{
			get;
			set;
		}

		public string TexturePath
		{
			get;
			set;
		}

		public double TextureScaleU
		{
			get;
			set;
		}

		public double TextureScaleV
		{
			get;
			set;
		}

		public double TextureOffsetU
		{
			get;
			set;
		}

		public double TextureOffsetV
		{
			get;
			set;
		}

		public double TextureRotationAngle
		{
			get;
			set;
		}

		public ModelMaterial()
		{
			this.Name = "Default material";
			this.Color = Color.Gray;
			this.Transparency = 0.0;
			this.Shininess = 0.0;
			this.TexturePath = "";
			this.TextureScaleU = 1.0;
			this.TextureScaleV = 1.0;
			this.TextureOffsetU = 0.0;
			this.TextureOffsetV = 0.0;
			this.TextureRotationAngle = 0.0;
		}

		public bool Equals(ModelMaterial other)
		{
			return other != null && this.GetHashCode() == other.GetHashCode();
		}

		public override int GetHashCode()
		{
			return this.Name.GetHashCode() ^ this.Color.R.GetHashCode() ^ this.Color.G.GetHashCode() ^ this.Color.B.GetHashCode() ^ this.Transparency.GetHashCode() ^ this.Shininess.GetHashCode() ^ this.TexturePath.GetHashCode() ^ this.TextureScaleU.GetHashCode() ^ this.TextureScaleV.GetHashCode() ^ this.TextureOffsetU.GetHashCode() ^ this.TextureOffsetV.GetHashCode() ^ this.TextureRotationAngle.GetHashCode();
		}
	}
}
