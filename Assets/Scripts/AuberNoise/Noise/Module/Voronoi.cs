using System;

namespace Aubergine.Noise.Module {

	public class Voronoi : NoiseGen, IModule {
		//Properties//
		public double Frequency { get; set; }
		public double Displacement { get; set; }
		public bool EnableDistance { get; set; }
		public int Seed { get; set; }

		//Constructor//
		public Voronoi() {
			//Defaults
			Frequency = 1.0;
			Displacement = 1.0;
			EnableDistance = true;
			Seed = 0;
		}

		//Methods//
		public double GetValue(double x, double y, double z) {
			//This method could be more efficient by caching.
			x *= Frequency;
			y *= Frequency;
			z *= Frequency;

			int xInt = (x > 0.0 ? (int)x : (int)x - 1);
			int yInt = (x > 0.0 ? (int)y : (int)y - 1);
			int zInt = (x > 0.0 ? (int)z : (int)z - 1);
			
			double minDist = 2147483647.0;
			double xCandidate = 0.0;
			double yCandidate = 0.0;
			double zCandidate = 0.0;

			//Inside each unit cube, theres a seed point at random position.
			for (int zCur = zInt - 2; zCur <= zInt + 2; zCur++) {
				for (int yCur = yInt - 2; yCur <= yInt + 2; yCur++) {
					for (int xCur = xInt - 2; xCur <= xInt + 2; xCur++) {
						//Calculate the position and distance to the seed point.
						double xPos = xCur + ValueNoise3D(xCur, yCur, zCur, Seed);
						double yPos = yCur + ValueNoise3D(xCur, yCur, zCur, Seed+1);
						double zPos = zCur + ValueNoise3D(xCur, yCur, zCur, Seed+2);
						double xDist = xPos - x;
						double yDist = yPos - y;
						double zDist = zPos - z;
						double dist = xDist * xDist + yDist * yDist + zDist * zDist;
						
						if (dist < minDist) {
							//This seed point is closer to any others found so far.
							minDist = dist;
							xCandidate = xPos;
							yCandidate = yPos;
							zCandidate = zPos;
						}
					}
				}
			}

			double value;
			if (EnableDistance) {
				//Determine the distance to nearest seed point
				double xDist = xCandidate - x;
				double yDist = yCandidate - y;
				double zDist = zCandidate - z;
				value = (Math.Sqrt(xDist * xDist + yDist * yDist + zDist * zDist)) *
							MathUtils.SQRT_3 - 1.0;
			}
			else {
				value = 0.0;
			}
			//Return calculated distance with displacement applied
			return value + (Displacement * (double)ValueNoise3D(
						(int)(Math.Floor(xCandidate)),
						(int)(Math.Floor(yCandidate)),
						(int)(Math.Floor(zCandidate))));
		}
	}

}