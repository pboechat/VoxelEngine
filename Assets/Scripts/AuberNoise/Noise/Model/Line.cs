namespace Aubergine.Noise.Model {

	public class Line {
		//Variables//
		double m_x0, m_x1, m_y0, m_y1, m_z0, m_z1;

		//Properties//
		public IModule Module0 { get; set; }
		public bool Attenuate { get; set; }

		//Constructor//
		public Line(IModule mod0) {
			Module0 = mod0;
			Attenuate = true;
			m_x0 = 0.0; m_y0 = 0.0; m_z0 = 0.0;
			m_x1 = 1.0; m_y1 = 1.0; m_z1 = 1.0;
		}

		//Methods//
		public double GetValue(double v) {
			double x = (m_x1 - m_x0) * v + m_x0;
			double y = (m_y1 - m_y0) * v + m_y0;
			double z = (m_z1 - m_z0) * v + m_z0;
			double value = Module0.GetValue(x, y, z);

			if (Attenuate) {
				return v * (1.0 - v) * 4 * value;
			}
			else {
				return value;
			}
		}

		public void SetStartPoint(double x, double y, double z) {
			m_x0 = x; m_y0 = y; m_z0 = z;
		}

		public void SetEndPoint(double x, double y, double z) {
			m_x1 = x; m_y1 = y; m_z1 = z;
		}
	}

}