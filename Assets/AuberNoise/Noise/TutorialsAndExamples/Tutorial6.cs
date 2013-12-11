using UnityEngine;
using Aubergine.Noise;
using Aubergine.Noise.Module;
using Aubergine.Noise.NoiseUtils;

public class Tutorial6 : MonoBehaviour {

	Texture2D tex;

	// Use this for initialization
	void Start () {
		IModule mountainTerrain = new RidgedMulti();
		IModule baseFlatTerrain = new Billow();
		((Billow)baseFlatTerrain).Frequency = 2.0;
		IModule flatTerrain = new ScaleBias(baseFlatTerrain, 0.125, -0.75);
		IModule terrainType = new Perlin();
		((Perlin)terrainType).Frequency = 0.5;
		((Perlin)terrainType).Persistence = 0.25;
		IModule terrainSelector = new Select(flatTerrain, mountainTerrain, terrainType);
		((Select)terrainSelector).SetBounds(0.0, 1000.0);
		((Select)terrainSelector).SetEdgeFallOff(0.125);
		IModule finalTerrain = new Turbulence(terrainSelector);
		((Turbulence)finalTerrain).Frequency = 4.0;
		((Turbulence)finalTerrain).Power = 0.125;
		
		NoiseMapBuilderPlane heightMapBuilder = new NoiseMapBuilderPlane(256, 256);
		heightMapBuilder.SetBounds(6.0, 10.0, 1.0, 5.0);
		heightMapBuilder.Build(finalTerrain);
		RendererImage render = new RendererImage();
		render.SourceNoiseMap = heightMapBuilder.Map;
		render.ClearGradient ();
		render.AddGradientPoint(-1.0000, new Color32(32, 160, 0, 255));
		render.AddGradientPoint(-0.2500, new Color32(224, 224, 0, 255));
		render.AddGradientPoint(0.2500, new Color32(128, 128, 128, 255));
		render.AddGradientPoint(1.0000, new Color32(255, 255, 255, 255));
		render.IsLightEnabled = true;
		render.LightContrast = 3.0;
		render.LightBrightness = 2.0;
		render.Render();

		tex = render.GetTexture();
	}

	void OnGUI () {
		
		if (tex != null)
			GUI.DrawTexture(new Rect(0, 0, tex.width, tex.height), tex, ScaleMode.ScaleToFit, true);
	}
}