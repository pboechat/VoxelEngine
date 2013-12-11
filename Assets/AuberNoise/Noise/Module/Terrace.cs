using System.Collections.Generic;

namespace Aubergine.Noise.Module {

	public class Terrace : IModule {
		//Properties//
		public IModule Module0 { get; set; }
		public List<double> ControlPoints = new List<double>();
		public bool InvertTerraces { get; set; }

		//Constructor//
		public Terrace() { }
		public Terrace(IModule mod0) {
			Module0 = mod0;
			InvertTerraces = false;
		}

		//Methods//
		public double GetValue(double x, double y, double z) {
			double value = Module0.GetValue(x, y, z);
			//Find the first element which has an input value larger than output value
			int indexPos;
			for (indexPos = 0; indexPos < ControlPoints.Count; indexPos++) {
				if (value < ControlPoints[indexPos]) {
					break;
				}
			}
			//Find two nearest control points to do quadratic interpolation
			int index0 = MathUtils.ClampValue(indexPos - 1, 0, ControlPoints.Count - 1);
			int index1 = MathUtils.ClampValue(indexPos, 0, ControlPoints.Count - 1);
			//If some points are missing, get nearest points value and exit now
			if (index0 == index1) {
				return ControlPoints[index1];
			}
			//Compute alpha value for cubic interpolation
			double value0 = ControlPoints[index0];
			double value1 = ControlPoints[index1];
			double alpha = (value - value0) / (value1 - value0);
			if (InvertTerraces) {
				alpha = 1.0 - alpha;
				MathUtils.SwapValues(ref value0, ref value1);
			}
			//Squaring alpha produces terrace effect.
			alpha *= alpha;
			//Now perform linear interpolation with the alpha
			return MathUtils.LinearInterp(value0, value1, alpha);
		}
	}

}