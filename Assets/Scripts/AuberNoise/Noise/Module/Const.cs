namespace Aubergine.Noise.Module {

	public class Const : IModule {
		//Properties//
		public IModule Module0 { get; set; }
		public double ConstantValue { get; set; }

		//Constructors//
		public Const() { }
		public Const(IModule mod0) : this(mod0, 0.0) {
		}

		public Const(IModule mod0, double constVal) {
			Module0 = mod0;
			ConstantValue = constVal;
		}

		//Methods//
		public double GetValue(double x, double y, double z) {
			return ConstantValue;
		}
	}

}