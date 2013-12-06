namespace Aubergine.Noise.Model {

	public class Sphere {
		//Properties//
		public IModule Module0 { get; set; }

		//Constructor//
		public Sphere(IModule mod0) {
			Module0 = mod0;
		}

		//Methods//
		public double GetValue(double lat, double lon) {
			double x = 0, y = 0, z = 0;
			MathUtils.LatLonToXYZ(lat, lon, ref x, ref y, ref z);
			return Module0.GetValue(x, y, z);
		}
	}

}