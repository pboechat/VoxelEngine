namespace Aubergine.Noise.Model {

	public class Plane {
		//Properties//
		public IModule Module0 { get; set; }

		//Constructor//
		public Plane(IModule mod0) {
			Module0 = mod0;
		}

		//Methods//
		public double GetValue(double x, double z) {
			return Module0.GetValue(x, 0, z);
		}
	}

}