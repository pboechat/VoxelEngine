namespace Aubergine.Noise.Module {

	public class ScalePoint : IModule {
		//Properties//
		public IModule Module0 { get; set; }
		public double XScale { get; set; }
		public double YScale { get; set; }
		public double ZScale { get; set; }

		//Constructors//
		public ScalePoint() { }
		public ScalePoint(IModule mod0) : this(mod0, 1.0, 1.0, 1.0) {
		}

		public ScalePoint(IModule mod0, double xScale, double yScale, double zScale) {
			Module0 = mod0;
			XScale = xScale;
			YScale = yScale;
			ZScale = zScale;
		}

		//Methods//
		public double GetValue(double x, double y, double z) {
			return Module0.GetValue(x * XScale, y * YScale, z * ZScale);
		}
	}

}