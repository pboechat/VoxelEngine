using System;

namespace Aubergine.Noise.Model {

	public class Cylinder {
		//Properties//
		public IModule Module0 { get; set; }

		//Constructor//
		public Cylinder(IModule mod0) {
			Module0 = mod0;
		}

		//Methods//
		public double GetValue(double angle, double height) {
			double x = 0, y = 0, z = 0;
			x = Math.Cos(angle * MathUtils.DEG_TO_RAD);
			y = height;
			z = Math.Sin(angle * MathUtils.DEG_TO_RAD);
			return Module0.GetValue(x, y, z);
		}
	}

}