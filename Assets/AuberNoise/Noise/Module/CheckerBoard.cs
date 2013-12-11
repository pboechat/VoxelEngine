using System;

namespace Aubergine.Noise.Module {

	public class CheckerBoard : IModule {
		//Constructors//
		public CheckerBoard() {
		}

		//Methods//
		public double GetValue(double x, double y, double z) {
			int ix = (int)(Math.Floor(MathUtils.MakeInt32Range(x)));
			int iy = (int)(Math.Floor(MathUtils.MakeInt32Range(y)));
			int iz = (int)(Math.Floor(MathUtils.MakeInt32Range(z)));
			return (ix & 1 ^ iy & 1 ^ iz & 1) != 0 ? -1.0 : 1.0;
		}
	}

}