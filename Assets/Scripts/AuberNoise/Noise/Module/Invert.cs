namespace Aubergine.Noise.Module {

	public class Invert : IModule {
		//Properties//
		public IModule Module0 { get; set; }

		//Constructors//
		public Invert() { }
		public Invert(IModule mod0) {
			Module0 = mod0;
		}

		//Methods//
		public double GetValue(double x, double y, double z) {
			return -(Module0.GetValue(x, y, z));
		}
	}

}