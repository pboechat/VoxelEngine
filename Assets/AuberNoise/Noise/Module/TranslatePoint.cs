namespace Aubergine.Noise.Module {

	public class TranslatePoint : IModule {
		//Properties//
		public IModule Module0 { get; set; }
		public double XTranslation { get; set; }
		public double YTranslation { get; set; }
		public double ZTranslation { get; set; }

		//Constructors//
		public TranslatePoint() { }
		public TranslatePoint(IModule mod0) : this(mod0, 1.0, 1.0, 1.0) {
		}

		public TranslatePoint(IModule mod0, double xTrans, double yTrans, double zTrans) {
			Module0 = mod0;
			XTranslation = xTrans;
			YTranslation = yTrans;
			ZTranslation = zTrans;
		}

		//Methods//
		public double GetValue(double x, double y, double z) {
			return Module0.GetValue(x + XTranslation, y + YTranslation, z + ZTranslation);
		}
	}

}