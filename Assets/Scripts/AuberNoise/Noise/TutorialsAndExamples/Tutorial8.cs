using UnityEngine;
using Aubergine.Noise;
using Aubergine.Noise.Module;
using Aubergine.Noise.NoiseUtils;

public class Tutorial8 : MonoBehaviour {

	Texture2D tex;

	// Use this for initialization
	void Start () {
		IModule module = new Perlin();
		NoiseMapBuilderSphere heightMap = new NoiseMapBuilderSphere(512, 256);
		heightMap.SetBounds(-90.0, 90.0, -180.0, 180.0);
		heightMap.Build(module);
		RendererImage render = new RendererImage();
		render.SourceNoiseMap = heightMap.Map;
		render.ClearGradient ();
		render.AddGradientPoint (-1.0000, new Color32(  0,   0, 128, 255)); // deeps
		render.AddGradientPoint (-0.2500, new Color32(  0,   0, 255, 255)); // shallow
		render.AddGradientPoint ( 0.0000, new Color32(  0, 128, 255, 255)); // shore
		render.AddGradientPoint ( 0.0625, new Color32(240, 240,  64, 255)); // sand
		render.AddGradientPoint ( 0.1250, new Color32( 32, 160,   0, 255)); // grass
		render.AddGradientPoint ( 0.3750, new Color32(224, 224,   0, 255)); // dirt
		render.AddGradientPoint ( 0.7500, new Color32(128, 128, 128, 255)); // rock
		render.AddGradientPoint ( 1.0000, new Color32(255, 255, 255, 255)); // snow
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