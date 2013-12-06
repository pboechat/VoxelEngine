namespace Aubergine.Noise.Module {

	public class Add : IModule {
		//Properties//
		public IModule Module0 { get; set; }
		public IModule Module1 { get; set; }

		//Constructors//
		public Add() { }
		public Add(IModule mod0, IModule mod1) {
			Module0 = mod0;
			Module1 = mod1;
		}

		//Methods//
		public double GetValue(double x, double y, double z) {
			return Module0.GetValue(x, y, z) + Module1.GetValue(x, y, z);
		}
	}

}