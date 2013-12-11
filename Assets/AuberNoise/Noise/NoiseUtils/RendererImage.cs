using UnityEngine;
using System;

namespace Aubergine.Noise.NoiseUtils {

	public class RendererImage {
		//Variables//
		public GradientColor gradient;
		private bool calcLightValues;
		private double cosAzimuth, sinAzimuth, cosElev, sinElev;
		public ImageMap BackgroundImage;
		public ImageMap DestImage;
		public NoiseMap SourceNoiseMap;

		//Properties//
		public bool IsLightEnabled { get; set; }
		public bool IsWrapEnabled { get; set; }
		public double LightAzimuth { get; set; }
		public double LightBrightness { get; set; }
		public Color LightColor { get; set; }
		public double LightContrast { get; set; }
		public double LightElev { get; set; }
		public double LightIntensity { get; set; }

		//Constructors//
		public RendererImage() {
			IsLightEnabled = false;
			IsWrapEnabled = false;
			LightAzimuth = 45.0;
			LightBrightness = 1.0;
			LightColor = Color.white;
			LightContrast = 1.0;
			LightElev = 45.0;
			LightIntensity = 1.0;
			BackgroundImage = null;
			DestImage = null;
			SourceNoiseMap = null;
			calcLightValues = true;
			//BuildGrayscaleGradient();
			BuildTerrainGradient();
		}


		//Methods//
		public void Render() {
			int width = SourceNoiseMap.Width;
			int height = SourceNoiseMap.Height;
			DestImage = new ImageMap(width, height);
			
			for (int y=0; y < height; y++) {
				for (int x=0; x < width; x++) {
					Color destColor = gradient.GetColor(SourceNoiseMap.GetValue(x, y));
					double lightIntensity;
					if (IsLightEnabled) {
						int xLeftOffset, xRightOffset;
						int yUpOffset, yDownOffset;
						if (IsWrapEnabled) {
							if (x == 0) {
								xLeftOffset = (int)width - 1;
								xRightOffset = 1;
							}
							else if (x == (int)width - 1) {
								xLeftOffset = -1;
								xRightOffset = -((int)width - 1);
							}
							else {
								xLeftOffset = -1;
								xRightOffset = 1;
							}
							if (y == 0) {
								yDownOffset = (int)height - 1;
								yUpOffset = 1;
							}
							else if (y == (int)height - 1) {
								yDownOffset = -1;
								yUpOffset = -((int)height - 1);
							}
							else {
								yDownOffset = -1;
								yUpOffset = 1;
							}
						}
						else {
							if (x == 0) {
								xLeftOffset = 0;
								xRightOffset = 1;
							}
							else if (x == (int)width - 1) {
								xLeftOffset = -1;
								xRightOffset = 0;
							}
							else {
								xLeftOffset = -1;
								xRightOffset = 1;
							}
							if (y == 0) {
								yDownOffset = 0;
								yUpOffset = 1;
							}
							else if (y == (int)height - 1) {
								yDownOffset = -1;
								yUpOffset = 0;
							}
							else {
								yDownOffset = -1;
								yUpOffset = 1;
							}
						}
						
						double nc = (double)SourceNoiseMap.GetValue(x, y);
						double nl = (double)SourceNoiseMap.GetValue(x + xLeftOffset, y);
						double nr = (double)SourceNoiseMap.GetValue(x + xRightOffset, y);
						double nd = (double)SourceNoiseMap.GetValue(x, y + yDownOffset);
						double nu = (double)SourceNoiseMap.GetValue(x, y + yUpOffset);
						
						lightIntensity = CalcLightIntensity(nc, nl, nr, nd, nu);
						lightIntensity *= LightBrightness;
					}
					else {
						lightIntensity = 1.0;
					}
					
					Color backgroundColor = Color.white;
					if (BackgroundImage != null) {
						backgroundColor = BackgroundImage.GetValue(x, y);
					}
					
					DestImage.SetValue(x, y, CalcDestColor(destColor, backgroundColor, lightIntensity));
				}
			}
		}

		public void AddGradientPoint(double gradPos, Color gradColor) {
			gradient.AddGradientPoint(gradPos, gradColor);
		}

		public double GetPositionAtPos(int pos) {
			double val = 0;
			if(pos < gradient.Gradient.Count)
				val = gradient.Gradient[pos].Pos;
			return val;
		}

		public Color GetColorAtPos(int pos) {
			Color val = new Color();
			if(pos < gradient.Gradient.Count)
				val = gradient.Gradient[pos].Col;
			return val;
		}

		public void BuildGrayscaleGradient() {
			ClearGradient();
			gradient.AddGradientPoint(-1.0, new Color32(0, 0, 0, 255));
			gradient.AddGradientPoint(1.0, new Color32(255, 255, 255, 255));
		}

		public void BuildTerrainGradient() {
			ClearGradient();
			gradient.AddGradientPoint(-1.00, new Color32(0, 0, 128, 255));
			gradient.AddGradientPoint(-0.20, new Color32(32, 64, 128, 255));
			gradient.AddGradientPoint(-0.04, new Color32(64, 96, 192, 255));
			gradient.AddGradientPoint(-0.02, new Color32(192, 192, 128, 255));
			gradient.AddGradientPoint( 0.00, new Color32(0, 192, 0, 255));
			gradient.AddGradientPoint( 0.25, new Color32(192, 192, 0, 255));
			gradient.AddGradientPoint( 0.50, new Color32(160, 96, 64, 255));
			gradient.AddGradientPoint( 0.75, new Color32(128, 255, 255, 255));
			gradient.AddGradientPoint( 1.00, new Color32(255, 255, 255, 255));
		}

		public void ClearGradient() {
			gradient = new GradientColor();
		}

		public Texture2D GetTexture() {
			Texture2D tex = new Texture2D(DestImage.Width, DestImage.Height);
			for (int y=0; y < tex.height; y++) {
				for (int x=0; x < tex.width; x++) {
					tex.SetPixel(x, y, DestImage.GetValue(x, y));
				}
			}
			tex.Apply();
			return tex;
		}

		private Color CalcDestColor(Color sourceColor, Color backgroundColor, double lightValue) {
			float alpha = (float)sourceColor.a; 
			Color result = Color.Lerp(backgroundColor, sourceColor, alpha);

			if (IsLightEnabled) {
				Color light = (float)lightValue * LightColor;
				result *= light;
			}
			result.r = (result.r < 0.0f) ? 0.0f : result.r;
			result.r = (result.r > 1.0f) ? 1.0f : result.r;
			result.g = (result.g < 0.0f) ? 0.0f : result.g;
			result.g = (result.g > 1.0f) ? 1.0f : result.g;
			result.b = (result.b < 0.0f) ? 0.0f : result.b;
			result.b = (result.b > 1.0f) ? 1.0f : result.b;
			return new Color(result.r, result.g, result.b, alpha);
		}

		private double CalcLightIntensity(double center, double left, double right, double down, double up) {
			if (calcLightValues) {
				cosAzimuth = Math.Cos(LightAzimuth * MathUtils.DEG_TO_RAD);
				sinAzimuth = Math.Sin(LightAzimuth * MathUtils.DEG_TO_RAD);
				cosElev = Math.Cos(LightElev * MathUtils.DEG_TO_RAD);
				sinElev = Math.Sin(LightElev * MathUtils.DEG_TO_RAD);
				calcLightValues = false;
			}
			double io = 1.0 * MathUtils.SQRT_2 * sinElev / 2.0;
			double ix = (1.0 - io) * LightContrast * MathUtils.SQRT_2 * cosElev * cosAzimuth;
			double iy = (1.0 - io) * LightContrast * MathUtils.SQRT_2 * cosElev * sinAzimuth;
			double intensity = (ix * (left - right) + iy * (down - up) + io);
			if (intensity < 0.0)
				intensity = 0.0;
			return intensity;
		}
	}
}