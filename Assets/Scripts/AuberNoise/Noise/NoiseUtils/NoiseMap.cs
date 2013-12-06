namespace Aubergine.Noise.NoiseUtils {

	public class NoiseMap {
		//Variables//
		private float[,] m_Map;

		//Properties//
		public float Border { get; set; }
		public int Height { get; set; }
		public int Width { get; set; }

		//Constructors//
		public NoiseMap() : this(64, 64, 0.0f) {
		}

		public NoiseMap(int width, int height) : this(width, height, 0.0f) {
		}

		public NoiseMap(int width, int height, float border) {
			SetSize(width, height);
			Border = border;
		}

		//Methods//
		public float GetValue(int x, int y) {
			if (m_Map != null) {
				if (x >= 0 && x < Width && y >= 0 && y < Height) {
					return m_Map[x, y];
				}
			}
			return Border;
		}

		public void SetValue(int x, int y, float value) {
			if (m_Map != null) {
				if (x >= 0 && x < Width && y >= 0 && y < Height) {
					m_Map[x, y] = value;
				}
			}
		}

		public void SetSize(int width, int height) {
			Width = width;
			Height = height;
			m_Map = new float[Width, Height];
		}

	}
}