using System;

namespace Aubergine.Noise.Module {

	public class Exponent : IModule {
		//Properties//
		public IModule Module0 { get; set; }
		public double ExponentVal { get; set; }

		//Constructors//
		public Exponent() { }
		public Exponent(IModule mod0) : this(mod0, 1.0) {
		}

		public Exponent(IModule mod0, double exponent) {
			Module0 = mod0;
			ExponentVal = exponent;
		}

		//Methods//
		public double GetValue(double x, double y, double z) {
			double value = Module0.GetValue(x, y, z);
			return (Math.Pow(Math.Abs((value + 1.0) / 2.0), ExponentVal) * 2.0 - 1.0);
		}
	}

}