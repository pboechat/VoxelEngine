namespace Aubergine.Noise.Module {

	public class Select : IModule {
		//Variables//
		private double edgeFalloff;

		//Properties//
		public IModule Module0 { get; set; }
		public IModule Module1 { get; set; }
		public IModule ModuleC { get; set; }
		public double EdgeFallOff { 
			get { return edgeFalloff; }
			set {
				double boundSize = UpperBound - LowerBound;
				edgeFalloff = (value > boundSize / 2) ? boundSize / 2 : value;
			}
		}
		public double LowerBound { get; private set; }
		public double UpperBound { get; private set; }

		//Constructors//
		public Select() { }
		public Select(IModule mod0, IModule mod1, IModule modC) : 
							this(mod0, mod1, modC, 0.0, -1.0, 1.0) {
		}

		public Select(IModule mod0, IModule mod1, IModule modC,
							double edge, double lowerBound, double upperBound) {
			Module0 = mod0;
			Module1 = mod1;
			ModuleC = modC;
			EdgeFallOff = edge;
			LowerBound = lowerBound;
			UpperBound = upperBound;
		}

		//Methods//
		public double GetValue(double x, double y, double z) {
			double controlValue = ModuleC.GetValue(x, y, z);
			double alpha;
			if (EdgeFallOff > 0.0) {
				if (controlValue < (LowerBound - EdgeFallOff)) {
					return Module0.GetValue(x, y, z);
				}
				else if (controlValue < (LowerBound + EdgeFallOff)) {
					double lowerCurve = (LowerBound - EdgeFallOff);
					double upperCurve = (LowerBound + EdgeFallOff);
					alpha = MathUtils.SCurve3((controlValue - lowerCurve) / (upperCurve - lowerCurve));
					return MathUtils.LinearInterp(Module0.GetValue(x, y, z), Module1.GetValue(x, y, z), alpha);
				}
				else if (controlValue < (UpperBound - EdgeFallOff)) {
					//Output value from control module is within selector threshold.
					return Module1.GetValue(x, y, z);
				}
				else if (controlValue < (UpperBound + EdgeFallOff)) {
					double lowerCurve = (UpperBound - EdgeFallOff);
					double upperCurve = (UpperBound + EdgeFallOff);
					alpha = MathUtils.SCurve3((controlValue - lowerCurve) / (upperCurve - lowerCurve));
					return MathUtils.LinearInterp(Module1.GetValue(x, y, z), Module0.GetValue(x, y, z), alpha);
				}
				else {
					return Module0.GetValue(x, y, z);
				}
			}
			else {
				if (controlValue < LowerBound || controlValue > UpperBound) {
					return Module0.GetValue(x, y, z);
				}
				else {
					return Module1.GetValue(x, y, z);
				}
			}
		}

		public void SetBounds(double lowerBound, double upperBound) {
			LowerBound = lowerBound;
			UpperBound = upperBound;
			//Make sure the edge falloff curves dont overlap.
			EdgeFallOff = edgeFalloff;
		}

		public void SetEdgeFallOff(double value) {
			double boundSize = UpperBound - LowerBound;
			edgeFalloff = (value > boundSize / 2) ? boundSize / 2 : value;
		}
	}

}