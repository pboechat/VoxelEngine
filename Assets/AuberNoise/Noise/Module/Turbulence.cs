namespace Aubergine.Noise.Module {

	public class Turbulence : IModule {
		//Variables//
		Perlin XDistortModule;
		Perlin YDistortModule;
		Perlin ZDistortModule;

		//Properties//
		public IModule Module0 { get; set; }
		public double Power { get; set; }
		public double Frequency {
			get { return XDistortModule.Frequency; }
			set { 
				XDistortModule.Frequency = value;
				YDistortModule.Frequency = value;
				ZDistortModule.Frequency = value;
			}
		}
		public int Roughness {
			get { return XDistortModule.OctaveCount; }
			set {
				XDistortModule.OctaveCount = value;
				YDistortModule.OctaveCount = value;
				ZDistortModule.OctaveCount = value;
			}
		}
		public int Seed {
			get { return XDistortModule.Seed; }
			set {
				XDistortModule.Seed = value;
				YDistortModule.Seed = value + 1;
				ZDistortModule.Seed = value + 2;
			}
		}

		//Constructor//
		public Turbulence() {
			XDistortModule = new Perlin();
			YDistortModule = new Perlin();
			ZDistortModule = new Perlin();
		}
		public Turbulence(IModule mod0) {
			Module0 = mod0;
			XDistortModule = new Perlin();
			YDistortModule = new Perlin();
			ZDistortModule = new Perlin();
			Frequency = 1.0;
			Power = 1.0;
			Roughness = 3;
			Seed = 0;
		}

		//Methods//
		public double GetValue(double x, double y, double z) {
			//Get values from three Perlins
			double x0, y0, z0;
			double x1, y1, z1;
			double x2, y2, z2;
			x0 = x + (12414.0 / 65536.0);
			y0 = y + (65124.0 / 65536.0);
			z0 = z + (31337.0 / 65536.0);
			x1 = x + (26519.0 / 65536.0);
			y1 = y + (18128.0 / 65536.0);
			z1 = z + (60493.0 / 65536.0);
			x2 = x + (53820.0 / 65536.0);
			y2 = y + (11213.0 / 65536.0);
			z2 = z + (44845.0 / 65536.0);
			double xDistort = x + (XDistortModule.GetValue(x0, y0, z0) * Power);
			double yDistort = y + (YDistortModule.GetValue(x1, y1, z1) * Power);
			double zDistort = z + (ZDistortModule.GetValue(x2, y2, z2) * Power);
			//Retrieve the output value
			return Module0.GetValue(xDistort, yDistort, zDistort);
		}
	}

}