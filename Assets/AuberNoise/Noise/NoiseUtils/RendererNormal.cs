using UnityEngine;
using System;

namespace Aubergine.Noise.NoiseUtils {

	public class RendererNormal {
		//Variables//
		public ImageMap DestImage;
		public NoiseMap SourceNoiseMap;

		//Properties//
		public double BumpHeight { get; set; }
		public bool IsWrapEnabled { get; set; }

		//Constructors//
		public RendererNormal() {
			BumpHeight = 1.0;
			IsWrapEnabled = false;
			DestImage = null;
			SourceNoiseMap = null;
		}


		//Methods//
		public void Render() {
			int width = SourceNoiseMap.Width;
			int height = SourceNoiseMap.Height;
			DestImage = new ImageMap(width, height);

			for (int y=0; y < height; y++) {
				for (int x=0; x < width; x++) {
					int xRightOffset, yUpOffset;
					if (IsWrapEnabled) {
						if (x == (int)width - 1) {
							xRightOffset = -((int)width - 1);
						}
						else {
							xRightOffset = 1;
						}
						if ( y == (int)height - 1) {
							yUpOffset = -((int)height - 1);
						}
						else {
							yUpOffset = 1;
						}
					}
					else {
						if (x == (int)width - 1) {
							xRightOffset = 0;
						}
						else {
							xRightOffset = 1;
						}
						if ( y == (int)height - 1) {
							yUpOffset = 0;
						}
						else {
							yUpOffset = 1;
						}
					}
					double nc = (double)SourceNoiseMap.GetValue(x, y);
					double nr = (double)SourceNoiseMap.GetValue(x + xRightOffset, y);
					double nu = (double)SourceNoiseMap.GetValue(x, y + yUpOffset);
					
					DestImage.SetValue(x, y, CalcNormalColor(nc, nr, nu, BumpHeight));
				}
			}
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

		private Color CalcNormalColor(double nc, double nr, double nu, double height) {
			nc *= height;
			nr *= height;
			nu *= height;
			double ncr = (nc - nr);
			double ncu = (nc - nu);
			double d = Math.Sqrt((ncu * ncu) + (ncr * ncr) + 1);
			double vxc = (nc - nr) / d;
			double vyc = (nc - nu) / d;
			double vzc = 1.0 / d;
			byte xc = (byte)((int)Math.Floor((vxc + 1.0) * 127.5) & 0xff);
			byte yc = (byte)((int)Math.Floor((vyc + 1.0) * 127.5) & 0xff);
			byte zc = (byte)((int)Math.Floor((vzc + 1.0) * 127.5) & 0xff);
			return new Color32(xc, yc, zc, 255);
		}
	}
}