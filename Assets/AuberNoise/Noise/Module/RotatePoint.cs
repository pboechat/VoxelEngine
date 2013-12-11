using System;

namespace Aubergine.Noise.Module {

	public class RotatePoint : IModule {
		//Properties//
		public IModule Module0 { get; set; }
		public double XAngle { get; set; }
		public double YAngle { get; set; }
		public double ZAngle { get; set; }

		//Variables//
		double m_x1Matrix, m_x2Matrix, m_x3Matrix;
		double m_y1Matrix, m_y2Matrix, m_y3Matrix;
		double m_z1Matrix, m_z2Matrix, m_z3Matrix;

		//Constructor//
		public RotatePoint() { }
		public RotatePoint(IModule mod0) : this(mod0, 0.0, 0.0, 0.0) {
		}

		public RotatePoint(IModule mod0, double xAngle, double yAngle, double zAngle) {
			Module0 = mod0;
			SetAngles(xAngle, yAngle, zAngle);
		}

		//Methods//
		public void SetAngles(double xAngle, double yAngle, double zAngle) {
			double xCos, yCos, zCos, xSin, ySin, zSin;
			xCos = Math.Cos(xAngle * MathUtils.DEG_TO_RAD);
			yCos = Math.Cos(yAngle * MathUtils.DEG_TO_RAD);
			zCos = Math.Cos(zAngle * MathUtils.DEG_TO_RAD);
			xSin = Math.Sin(xAngle * MathUtils.DEG_TO_RAD);
			ySin = Math.Sin(yAngle * MathUtils.DEG_TO_RAD);
			zSin = Math.Sin(zAngle * MathUtils.DEG_TO_RAD);

			m_x1Matrix = ySin * xSin * zSin + yCos * zCos;
			m_y1Matrix = xCos * zSin;
			m_z1Matrix = ySin * zCos - yCos * xSin * zSin;
			m_x2Matrix = ySin * xSin * zCos - yCos * zSin;
			m_y2Matrix = xCos * zCos;
			m_z2Matrix = -yCos * xSin * zCos - ySin * zSin;
			m_x3Matrix = -ySin * xCos;
			m_y3Matrix = xSin;
			m_z3Matrix = yCos * xCos;

			XAngle = xAngle; 
			YAngle = yAngle;
			ZAngle = zAngle;
		}

		public double GetValue(double x, double y, double z) {
			double nx = (m_x1Matrix * x) + (m_y1Matrix * y) + (m_z1Matrix * z);
			double ny = (m_x2Matrix * x) + (m_y2Matrix * y) + (m_z2Matrix * z);
			double nz = (m_x3Matrix * x) + (m_y3Matrix * y) + (m_z3Matrix * z);
			return Module0.GetValue(nx, ny, nz);
		}
	}

}