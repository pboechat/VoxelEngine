using System;

namespace Aubergine.Noise.Module {

	public class RidgedMulti : NoiseGen, IModule {
		//Variables//
		private double my_Lacunarity;
		private double[] SpectralWeights = new double[30];

		//Properties//
		public double Frequency { get; set; }
		public NoiseQuality NoiseQuality { get; set; }
		public int OctaveCount { get; set; }
		public int Seed { get; set; }
		public double Lacunarity {
			get { return my_Lacunarity; }
			set {
				my_Lacunarity = value;
				CalcSpectralWeights();
			}
		}

		//Constructor//
		public RidgedMulti() {
			//Defaults
			Frequency = 1.0;
			Lacunarity = 2.0;
			NoiseQuality = NoiseQuality.Standard;
			OctaveCount = 6;
			Seed = 0;
		}

		//Methods//
		public double GetValue(double x, double y, double z) {
			x *= Frequency;
			y *= Frequency;
			z *= Frequency;

			double signal = 0.0;
			double value = 0.0;
			double weight = 1.0;

			//These parameters should be user-defined;
			double offset = 1.0;
			double gain = 2.0;

			for (int curOctave = 0; curOctave < OctaveCount; curOctave++) {
				double nx, ny, nz;
				nx = MathUtils.MakeInt32Range(x);
				ny = MathUtils.MakeInt32Range(y);
				nz = MathUtils.MakeInt32Range(z);

				//Get the coherent-noise value from input value and add it to final result
				long seed = (Seed + curOctave) & 0x7fffffff;
				signal = GradientCoherentNoise3D(nx, ny, nz, (int)seed, NoiseQuality);
				//Make the ridges
				signal = Math.Abs(signal);
				signal = offset - signal;
				//Square signal to increase sharpness of ridges.
				signal *= signal;
				//The weighting from previous octave is applied to the signal.
				signal *= weight;
				//Weight successive contributions by previous signal.
				weight = signal * gain;
				if (weight > 1.0) {
					weight = 1.0;
				}
				if (weight < 0.0) {
					weight = 0.0;
				}
				//Add the signal to output value.
				value += (signal * SpectralWeights[curOctave]);
				//Next Octave
				x *= Lacunarity;
				y *= Lacunarity;
				z *= Lacunarity;
			}

			return (value * 1.25) - 1.0;
		}

		void CalcSpectralWeights() {
			//This exponent parameter shoudl be user defined
			double h = 1.0;
			
			double frequency = 1.0;
			for (int i = 0; i < 30; i++) {
				SpectralWeights[i] = Math.Pow(frequency, -h);
				frequency *= Lacunarity;
			}
		}
	}

}