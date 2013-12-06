using System;

namespace Aubergine.Noise {

	public static class MathUtils {
		public const double PI = 3.1415926535897932385;
		public const double SQRT_2 = 1.4142135623730950488;
		public const double SQRT_3 = 1.7320508075688772935;
		public const double DEG_TO_RAD = PI / 180.0;
		public const double RAD_TO_DEG = 1.0 / DEG_TO_RAD;

		//Linear Interpolation
		public static double LinearInterp(double v1, double v2, double s) {
			return ((1.0 - s) * v1) + (s * v2);
		}

		//Cubic Interpolation
		public static double CubicInterp(double v1, double v2, double v3, double v4, double s) {
			double s2 = s * s;
			double s3 = s2 * s;
			//double p1 = v4 - v3 - v1 + v2;
			double p1 = (v4 - v3) - (v1 - v2);
			double p2 = (v1 - v2) - p1;
			double p3 = v3 - v1;
			double p4 = v2;
			return p1 * s3 + p2 * s2 + p3 * s + p4;
		}

		//Cubic S-Curve value
		public static double SCurve3(double s) {
			return (s * s * (3.0 - 2.0 * s));
		}

		//Quintic S-Curve value
		public static double SCurve5(double s) {
			double s3 = s * s * s;
			double s4 = s3 * s;
			double s5 = s4 * s;
			return (6.0 * s5) - (15.0 * s4) + (10.0 * s3);
		}

		//Convert double to integer
		public static double MakeInt32Range(double s) {
			if (s >= 1073741824.0) {
				return (2.0 * Math.IEEERemainder(s, 1073741824.0)) - 1073741824.0;
			}
			else if (s <= -1073741824.0) {
				return (2.0 * Math.IEEERemainder(s, 1073741824.0)) + 1073741824.0;
			}
			else {
				return s;
			}
		}

		//Latitude and Longitude to XYZ on a unit sphere
		public static void LatLonToXYZ(double lat, double lon, ref double x, ref double y, ref double z) {
			double r = Math.Cos(DEG_TO_RAD * lat);
			x = r * Math.Cos(DEG_TO_RAD * lon);
			y = Math.Sin(DEG_TO_RAD * lat);
			z = r * Math.Sin(DEG_TO_RAD * lon);
		}

		//Swap two ref values
		public static void SwapValues(ref double a, ref double b) {
			double c = a;
			a = b;
			b = c;
		}

		//Clamp a int between defined borders
		public static int ClampValue(int value, int lowerBound, int upperBound) {
			if (value < lowerBound) {
				return lowerBound;
			}
			else if (value > upperBound) {
				return upperBound;
			}
			else {
				return value;
			}
		}
	}

}