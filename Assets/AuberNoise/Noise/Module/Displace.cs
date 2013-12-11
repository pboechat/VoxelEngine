namespace Aubergine.Noise.Module {

	public class Displace : IModule {
		//Properties//
		public IModule Module0 { get; set; }
		public IModule XDisplaceModule { get; set; }
		public IModule YDisplaceModule { get; set; }
		public IModule ZDisplaceModule { get; set; }

		//Constructors//
		public Displace() { }
		public Displace(IModule mod0, IModule xDisplaceModule, IModule yDisplaceModule, IModule zDisplaceModule) {
			Module0 = mod0;
			XDisplaceModule = xDisplaceModule;
			YDisplaceModule = yDisplaceModule;
			ZDisplaceModule = zDisplaceModule;
		}

		//Methods//
		public double GetValue(double x, double y, double z) {
			double xDisplace = x + XDisplaceModule.GetValue(x, y, z);
			double yDisplace = y + YDisplaceModule.GetValue(x, y, z);
			double zDisplace = z + ZDisplaceModule.GetValue(x, y, z);
			return Module0.GetValue(xDisplace, yDisplace, zDisplace);
		}
	}

}