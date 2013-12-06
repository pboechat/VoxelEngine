using System;

namespace Aubergine.Noise.Module {

	public class Cylinders : IModule {
		//Properties//
		public double Frequency { get; set; }

		//Constructors//
		public Cylinders() {
			Frequency = 1.0;
		}

		//Methods//
		public double GetValue(double x, double y, double z) {
			x *= Frequency;
			z *= Frequency;
			double distFromCenter = Math.Sqrt(x * x + z * z);
			double distFromSmallerSphere = distFromCenter - Math.Floor(distFromCenter);
			double distFromLargerSphere = 1.0 - distFromSmallerSphere;
			double nearestDist = Math.Min(distFromSmallerSphere, distFromLargerSphere);
			return 1.0 - (nearestDist * 4.0); //Puts it in the -1 to +1 range
		}
	}

}