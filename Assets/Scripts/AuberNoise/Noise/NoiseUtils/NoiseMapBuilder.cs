using System;

namespace Aubergine.Noise.NoiseUtils {

	//NoiseMap classes
	public class NoiseMapBuilderCylinder {
		//Variables//
		public NoiseMap Map;

		//Properties//
		public double LowerAngleBound { get; set; }
		public double LowerHeightBound { get; set; }
		public double UpperAngleBound { get; set; }
		public double UpperHeightBound { get; set; }

		//Constructors//
		public NoiseMapBuilderCylinder() : this (64, 64) {
		}

		public NoiseMapBuilderCylinder(int width, int height) {
			Map = new NoiseMap(width, height);
			SetBounds(0.0, 0.0, 0.0, 0.0);
		}

		//Methods//
		public void Build(IModule sourceModule) {
			Model.Cylinder cylinderModel = new Model.Cylinder(sourceModule);
			
			double angleExtent = UpperAngleBound - LowerAngleBound;
			double heightExtent = UpperHeightBound - LowerHeightBound;
			double xDelta = angleExtent / (double)Map.Width;
			double yDelta = heightExtent / (double)Map.Height;
			double curAngle = LowerAngleBound;
			double curHeight = LowerHeightBound;

			for (int y = 0; y < Map.Height; y++) {
				curAngle = LowerAngleBound;
				for (int x = 0; x < Map.Width; x++) {
					float curValue = (float)cylinderModel.GetValue(curAngle, curHeight);
					Map.SetValue(x, y, curValue);
					curAngle += xDelta;
				}
				curHeight += yDelta;
			}
		}

		public void SetBounds(double lab, double uab, double lhb, double uhb) {
			LowerAngleBound = lab;
			UpperAngleBound = uab;
			LowerHeightBound = lhb;
			UpperHeightBound = uhb;
		}
	}

	public class NoiseMapBuilderPlane {
		//Variables//
		public NoiseMap Map;

		//Properties//
		public bool IsSeamless { get; set; }
		public double LowerXBound { get; set; }
		public double LowerZBound { get; set; }
		public double UpperXBound { get; set; }
		public double UpperZBound { get; set; }

		//Constructors//
		public NoiseMapBuilderPlane() : this (64, 64) {
		}

		public NoiseMapBuilderPlane(int width, int height) {
			Map = new NoiseMap(width, height);
			IsSeamless = false;
			SetBounds(0.0, 0.0, 0.0, 0.0);
		}

		//Methods//
		public void Build(IModule sourceModule) {
			Model.Plane planeModel = new Model.Plane(sourceModule);
			
			double xExtent = UpperXBound - LowerXBound;
			double zExtent = UpperZBound - LowerZBound;
			double xDelta = xExtent / (double)Map.Width;
			double zDelta = zExtent / (double)Map.Height;
			double xCur = LowerXBound;
			double zCur = LowerZBound;

			for (int z = 0; z < Map.Height; z++) {
				xCur = LowerXBound;
				for (int x = 0; x < Map.Width; x++) {
					float finalValue;
					if (!IsSeamless) {
						finalValue = (float)planeModel.GetValue(xCur, zCur);
					}
					else {
						double swValue, seValue, nwValue, neValue;
						swValue = planeModel.GetValue(xCur, zCur);
						seValue = planeModel.GetValue(xCur + xExtent, zCur);
						nwValue = planeModel.GetValue(xCur, zCur + zExtent);
						neValue = planeModel.GetValue(xCur + xExtent, zCur + zExtent);
						double xBlend = 1.0 - ((xCur - LowerXBound) / xExtent);
						double zBlend = 1.0 - ((zCur - LowerZBound) / zExtent);
						double z0 = MathUtils.LinearInterp(swValue, seValue, xBlend);
						double z1 = MathUtils.LinearInterp(nwValue, neValue, xBlend);
						finalValue = (float)MathUtils.LinearInterp(z0, z1, zBlend);
					}
					Map.SetValue(x, z, finalValue);
					xCur += xDelta;
				}
				zCur += zDelta;
			}
		}

		public void SetBounds(double lx, double ux, double lz, double uz) {
			LowerXBound = lx;
			UpperXBound = ux;
			LowerZBound = lz;
			UpperZBound = uz;
		}
	}

	public class NoiseMapBuilderSphere {
		//Variables//
		public NoiseMap Map;

		//Properties//
		public double SouthLatBound { get; set; }
		public double NorthLatBound { get; set; }
		public double WestLonBound { get; set; }
		public double EastLonBound { get; set; }

		//Constructors//
		public NoiseMapBuilderSphere() : this (64, 64) {
		}

		public NoiseMapBuilderSphere(int width, int height) {
			Map = new NoiseMap(width, height);
			SetBounds(0.0, 0.0, 0.0, 0.0);
		}

		//Methods//
		public void Build(IModule sourceModule) {
			Model.Sphere sphereModel = new Model.Sphere(sourceModule);
			
			double lonExtent = EastLonBound - WestLonBound;
			double latExtent = NorthLatBound - SouthLatBound;
			double xDelta = lonExtent / (double)Map.Width;
			double yDelta = latExtent / (double)Map.Height;
			double curLon = WestLonBound;
			double curLat = SouthLatBound;

			for (int y = 0; y < Map.Height; y++) {
				curLon = WestLonBound;
				for (int x = 0; x < Map.Width; x++) {
					float curValue = (float)sphereModel.GetValue(curLat, curLon);
					Map.SetValue(x, y, curValue);
					curLon += xDelta;
				}
				curLat += yDelta;
			}
		}

		public void SetBounds(double slb, double nlb, double wlb, double elb) {
			SouthLatBound = slb;
			NorthLatBound = nlb;
			WestLonBound = wlb;
			EastLonBound = elb;
		}
	}
}