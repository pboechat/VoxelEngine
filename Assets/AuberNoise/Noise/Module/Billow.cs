using System;

namespace Aubergine.Noise.Module {

	public class Billow : NoiseGen, IModule {
		//Properties//
		public double Frequency { get; set; }
		public double Lacunarity { get; set; }
		public NoiseQuality NoiseQuality { get; set; }
		public int OctaveCount { get; set; }
		public double Persistence { get; set; }
		public int Seed { get; set; }

		//Constructors//
		public Billow() {
			Frequency = 1.0;
			Lacunarity = 2.0;
			NoiseQuality = NoiseQuality.Standard;
			OctaveCount = 6;
			Persistence = 0.5;
			Seed = 0;
		}

		//Methods//
		public double GetValue(double x, double y, double z) {
			double value = 0.0;
			double signal = 0.0;
			double curPersistence = 1.0;
			double nx, ny, nz;
			long seed;
			
			x *= Frequency;
			y *= Frequency;
			z *= Frequency;
			
			for (int curOctave = 0; curOctave < OctaveCount; curOctave++) {
				nx = MathUtils.MakeInt32Range(x);
				ny = MathUtils.MakeInt32Range(y);
				nz = MathUtils.MakeInt32Range(z);
				//Get the coherent-noise value from input value and add it to final result
				seed = (Seed + curOctave) & 0xffffffff;
				signal = GradientCoherentNoise3D(nx, ny, nz, (int)seed, NoiseQuality);
				signal = 2.0 * Math.Abs(signal) - 1.0;
				value += signal * curPersistence;
				
				x *= Lacunarity;
				y *= Lacunarity;
				z *= Lacunarity;
				curPersistence *= Persistence;
			}
			value += 0.5;

			return value;
		}
	}

}