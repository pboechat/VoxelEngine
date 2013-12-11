using System;

namespace Aubergine.Noise.Module {

	public class Min : IModule {
		//Properties//
		public IModule Module0 { get; set; }
		public IModule Module1 { get; set; }

		//Constructors//
		public Min() { }
		public Min(IModule mod0, IModule mod1) {
			Module0 = mod0;
			Module1 = mod1;
		}

		//Methods//
		public double GetValue(double x, double y, double z) {
			double a = Module0.GetValue(x, y, z);
			double b = Module1.GetValue(x, y, z);
			return Math.Min(a, b);
		}
	}

}