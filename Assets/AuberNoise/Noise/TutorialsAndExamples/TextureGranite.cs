using UnityEngine;
using Aubergine.Noise;
using Aubergine.Noise.Module;
using Aubergine.Noise.NoiseUtils;

public class TextureGranite : MonoBehaviour {

	Texture2D tex;

	// Use this for initialization
	void Start () {
		IModule primaryGranite = new Billow();
		((Billow)primaryGranite).Seed = 0;
		((Billow)primaryGranite).Frequency = 8.0;
		((Billow)primaryGranite).Persistence = 0.625;
		((Billow)primaryGranite).Lacunarity = 2.18359375;
		((Billow)primaryGranite).OctaveCount = 6;
		((Billow)primaryGranite).NoiseQuality = NoiseQuality.Standard;
		
		IModule baseGrains = new Voronoi();
		((Voronoi)baseGrains).Seed = 1;
		((Voronoi)baseGrains).Frequency = 16.0;
		((Voronoi)baseGrains).EnableDistance = true;
		
		IModule scaledGrains = new ScaleBias(baseGrains);
		((ScaleBias)scaledGrains).Scale = -0.5;
		((ScaleBias)scaledGrains).Bias = 0.0;
		
		IModule combinedGranite = new Add(primaryGranite, scaledGrains);
		
		IModule finalGranite = new Turbulence(combinedGranite);
		((Turbulence)finalGranite).Seed = 2;
		((Turbulence)finalGranite).Frequency = 4.0;
		((Turbulence)finalGranite).Power = 1.0 / 8.0;
		((Turbulence)finalGranite).Roughness = 6;
		
		NoiseMapBuilderPlane plane = new NoiseMapBuilderPlane(256, 256);
		plane.SetBounds(-1.0, 1.0, -1.0, 1.0);
		plane.Build(finalGranite);
		RendererImage render = new RendererImage();
		render.SourceNoiseMap = plane.Map;
		render.ClearGradient ();
		render.AddGradientPoint (-1.0000, new Color32(  0,   0,   0, 255));
		render.AddGradientPoint (-0.9375, new Color32(  0,   0,   0, 255));
		render.AddGradientPoint (-0.8750, new Color32(216, 216, 242, 255));
		render.AddGradientPoint ( 0.0000, new Color32(191, 191, 191, 255));
		render.AddGradientPoint ( 0.5000, new Color32(210, 116, 125, 255));
		render.AddGradientPoint ( 0.7500, new Color32(210, 113,  98, 255));
		render.AddGradientPoint ( 1.0000, new Color32(255, 176, 192, 255));
		render.IsLightEnabled = true;
		render.LightAzimuth = 135.0;
		render.LightElev = 60.0;
		render.LightContrast = 2.0;
		render.LightColor = new Color32(255, 255, 255, 0);
		render.Render();

		tex = render.GetTexture();
	}

	void OnGUI () {
		
		if (tex != null)
			GUI.DrawTexture(new Rect(0, 0, tex.width, tex.height), tex, ScaleMode.ScaleToFit, true);
	}
}