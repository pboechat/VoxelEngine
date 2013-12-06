using System;

namespace Aubergine.Noise.Module {

	public class Spheres : IModule {
		//Properties//
		public double Frequency { get; set; }

		//Constructors//
		public Spheres() {
			Frequency = 1.0;
		}

		//Methods//
		public double GetValue(double x, double y, double z) {
			x *= Frequency;
			y *= Frequency;
			z *= Frequency;
			double distFromCenter = Math.Sqrt(x * x + y * y + z * z);
			double distFromSmallerSphere = distFromCenter - Math.Floor(distFromCenter);
			double distFromLargerSphere = 1.0 - distFromSmallerSphere;
			double nearestDist = Math.Min(distFromSmallerSphere, distFromLargerSphere);
			return 1.0 - (nearestDist * 4.0); //Puts it in the -1 to +1 range
		}
	}

}