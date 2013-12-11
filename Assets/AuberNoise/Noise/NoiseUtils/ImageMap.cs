using UnityEngine;

namespace Aubergine.Noise.NoiseUtils {

	public class ImageMap {
		//Variables//
		private Color[,] m_Map;

		//Properties//
		public Color Border { get; set; }
		public int Height { get; set; }
		public int Width { get; set; }

		//Constructors//
		public ImageMap() : this(64, 64, Color.gray) {
		}

		public ImageMap(int width, int height) : this(width, height, Color.gray) {
		}

		public ImageMap(int width, int height, Color border) {
			SetSize(width, height);
			Border = border;
		}

		public ImageMap(Texture2D tex, Color border) {
			SetSize(tex.width, tex.height);
			Border = border;
			Color[] col = tex.GetPixels();
			int i = 0;
			for(int y = 0; y < tex.height; y++) {
				for(int x = 0; x < tex.width; x++) {
					m_Map[x, y] = col[i];
					i++;
				}
			}
		}

		//Methods//
		public Color GetValue(int x, int y) {
			if (m_Map != null) {
				if (x >= 0 && x < Width && y >= 0 && y < Height) {
					return m_Map[x, y];
				}
			}
			return Border;
		}

		public void SetValue(int x, int y, Color value) {
			if (m_Map != null) {
				if (x >= 0 && x < Width && y >= 0 && y < Height) {
					m_Map[x, y] = value;
				}
			}
		}

		public void SetSize(int width, int height) {
			Width = width;
			Height = height;
			m_Map = new Color[Width, Height];
		}

	}
}