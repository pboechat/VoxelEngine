namespace Aubergine.Noise.Module {

	public class Blend : IModule {
		//Properties//
		public IModule Module0 { get; set; }
		public IModule Module1 { get; set; }
		public IModule ModuleA { get; set; }

		//Constructors//
		public Blend() { }
		public Blend(IModule mod0, IModule mod1,  IModule modA) {
			Module0 = mod0;
			Module1 = mod1;
			ModuleA = modA;
		}

		//Methods//
		public double GetValue(double x, double y, double z) {
			double v1 = Module0.GetValue(x, y, z);
			double v2 = Module1.GetValue(x, y, z);
			double a = (ModuleA.GetValue(x, y, z) + 1.0) / 2.0;
			return MathUtils.LinearInterp(v1, v2, a);
		}
	}

}