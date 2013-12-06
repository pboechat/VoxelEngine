using System.Collections.Generic;

namespace Aubergine.Noise.Module {

	public struct ControlPoint {
		public double InVal;
		public double OutVal;
	}

	public class Curve : IModule {
		//Properties//
		public IModule Module0 { get; set; }
		public List<ControlPoint> ControlPoints = new List<ControlPoint>();

		//Constructor//
		public Curve() { }
		public Curve(IModule mod0) {
			Module0 = mod0;
		}

		//Methods//
		public double GetValue(double x, double y, double z) {
			double value = Module0.GetValue(x, y, z);
			//Find the first element which has an input value larger than output value
			int indexPos;
			for (indexPos = 0; indexPos < ControlPoints.Count; indexPos++) {
				if (value < ControlPoints[indexPos].InVal) {
					break;
				}
			}
			//Find four nearest control points to do cubic interpolation
			int index0 = MathUtils.ClampValue(indexPos - 2, 0, ControlPoints.Count - 1);
			int index1 = MathUtils.ClampValue(indexPos - 1, 0, ControlPoints.Count - 1);
			int index2 = MathUtils.ClampValue(indexPos, 0, ControlPoints.Count - 1);
			int index3 = MathUtils.ClampValue(indexPos + 1, 0, ControlPoints.Count - 1);
			//If some points are missing, get nearest points value and exit now
			if (index1 == index2) {
				return ControlPoints[index1].OutVal;
			}
			//Compute alpha value for cubic interpolation
			double input0 = ControlPoints[index1].InVal;
			double input1 = ControlPoints[index2].InVal;
			double alpha = (value - input0) / (input1 - input0);
			//Now perform cubic interpolation with the alpha
			return MathUtils.CubicInterp(
											ControlPoints[index0].OutVal,
											ControlPoints[index1].OutVal,
											ControlPoints[index2].OutVal,
											ControlPoints[index3].OutVal,
											alpha);
		}
	}

}