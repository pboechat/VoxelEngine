using UnityEngine;
using System.Collections.Generic;

namespace Aubergine.Noise.NoiseUtils {

	//Defines a point used to build a color gradient.
	public struct GradientPoint : System.IComparable<GradientPoint> {
		//Variables//
		public double Pos;
		public Color Col;

		//Constructor//
		public GradientPoint(double p, Color col) {
			Pos = p;
			Col = col;
		}

		//Comparer//
		public int CompareTo(GradientPoint other) {
			return this.Pos.CompareTo(other.Pos);
		}
	}

	public class GradientColor {
		//Properties//
		public List<GradientPoint> Gradient;

		//Constructors//
		public GradientColor() {
			Gradient = new List<GradientPoint>();
		}

		//Methods//
		//Add a gradient point to this gradient object (Preferably between -1.0 to +1.0 position range).
		//It doesnt check for duplicate entries
		public void AddGradientPoint(double pos, Color col) {
			Gradient.Add(new GradientPoint(pos, col));
			Gradient.Sort();
		}

		//Returns the color at the specified position in color gradient.
		public Color GetColor(double gradientPos) {
			//Find first element in gradient point that has gradient pos larger.
			int indexPos;
			for (indexPos = 0; indexPos < Gradient.Count; indexPos++) {
				if (gradientPos < Gradient[indexPos].Pos) {
					break;
				}
			}
			//Find two nearest gradient points to do linear interpolation
			int index0 = MathUtils.ClampValue(indexPos -1, 0, Gradient.Count -1);
			int index1 = MathUtils.ClampValue(indexPos, 0, Gradient.Count -1);
			//If some gradient points are missing
			if (index0 == index1) {
				return Gradient[index1].Col;
			}
			//Compute alpha value for linear interpolation
			double input0 = Gradient[index0].Pos;
			double input1 = Gradient[index1].Pos;
			double alpha = (gradientPos - input0) / (input1 - input0);
			//Perform linear interpolation with the alpha
			Color color0 = Gradient[index0].Col;
			Color color1 = Gradient[index1].Col;
			return Color.Lerp(color0, color1, (float)alpha);
		}
		/*
		Color LinearInterpColor(Color col0, Color col1, float alpha) {
			float r = BlendChannel(col0.r, col1.r, alpha);
			float g = BlendChannel(col0.g, col1.g, alpha);
			float b = BlendChannel(col0.b, col1.b, alpha);
			float a = BlendChannel(col0.a, col1.a, alpha);
			return new Color(r, g, b, a);
		}

		float BlendChannel(float channel0, float channel1, float alpha) {
			float c0 = channel0 / 255.0f;
			float c1 = channel1 / 255.0f;
			return ((c1 * alpha) + (c0 * (1.0f - alpha))) * 255.0f;
		}
		*/
	}

}