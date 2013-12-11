namespace Aubergine.Noise.Module {

	public class Clamp : IModule {
		//Properties//
		public IModule Module0 { get; set; }
		public double LowerBound { get; set; }
		public double UpperBound { get; set; }

		//Constructors//
		public Clamp() { }
		public Clamp(IModule mod0) : this(mod0, -1.0, 1.0) {
		}

		public Clamp(IModule mod0, double lowerBound, double upperBound) {
			Module0 = mod0;
			LowerBound = lowerBound;
			UpperBound = upperBound;
		}

		//Methods//
		public double GetValue(double x, double y, double z) {
			double value = Module0.GetValue(x, y, z);
			if (value < LowerBound) {
				return LowerBound;
			}
			else if (value > UpperBound) {
				return UpperBound;
			}
			else {
				return value;
			}
		}
	}

}