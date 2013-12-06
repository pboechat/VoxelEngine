using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Aubergine.Noise;
using Aubergine.Noise.Module;
using Aubergine.Noise.NoiseUtils;

public class Edt_TextureCreator : EditorWindow {
	private enum MAPTYPE {
		Cylindirical, Spherical, Planar
	}

	private enum MODULES {
		Abs, Add, Billow, Blend, CheckerBoard, Clamp, Const, Curve, Cylinders, Displace,
		Exponent, Invert, Max, Min, Multiply, Perlin, Power, RidgedMulti, RotatePoint,
		ScaleBias, ScalePoint, Select, Spheres, Terrace, TranslatePoint, Turbulence, Voronoi
	}

	//Editor Variables//
	private static Edt_TextureCreator window;
	private static Rect windowPosition = new Rect(80, 80, 0, 0);
	private static Vector2 windowMinSize = new Vector2(1110, 520);
	//private static Vector2 windowMaxSize = new Vector2(1112, 522);
	private static string windowTitle = "TextureCreator";
	
	//GUI Variables//
	private bool togModuleList = true;
	private bool togGradientList = true;
	private bool togModuleOptions = true;
	private bool togTextureOptions = true;
	private bool togRenderOptions = true;
	private Vector2 scrlModuleList = Vector2.zero;
	private Vector2 scrlGradientList = Vector2.zero;
	private static System.Enum popMapTypes = MAPTYPE.Planar;
	private static System.Enum popModules = MODULES.Perlin;
	private static IModule selectedModule = null;
	int mod0 = 0;
	int mod1 = 0;
	int mod2 = 0;
	private double frequency = 1.0;
	private double lacunarity = 2.0;
	private System.Enum noiseQuality = NoiseQuality.Standard;
	private int octaveCount = 6;
	private double persistence = 0.5;
	private int seed = 0;
	private double lowerBound = -1.0;
	private double upperBound = 1.0;
	private double tempVal1 = 0.0;
	private double tempVal2 = 0.0;
	private double tempVal3 = 0.0;
	private double power = 1.0;
	private int roughness = 3;
	private bool tempBool = true;
	private double lowerXBound = -1.0;
	private double lowerZBound = -1.0;
	private double upperXBound = 1.0;
	private double upperZBound = 1.0;
	private double gradPos = 0.0;

	//Renderer Variables//
	private bool seamlessEnabled = false;
	private double bumpHeight = 1.0;
	private bool wrapEnabled = false;
	private bool lightEnabled = false;
	private double lightAzimuth = 45.0;
	private double lightElevation = 45.0;
	private double lightBrightness = 1.0;
	private double lightContrast = 1.0;
	private double lightIntensity = 1.0;
	private Color32 lightColor = Color.white;
	private static RendererImage render;
	private Color32 gradientColor = Color.white;

	//Texture Variables//
	private Texture2D finalTexture;
	private Texture2D backgroundTexture;
	private string textureName = "Texture";
	private int textureWidth = 256;
	private int textureHeight = 256;

	//Noise Variables//
	private static List<IModule> modules;
	
	//Terrain variables//
	private static Terrain terrain;
	
	[MenuItem("Aubergine/TextureCreator")]
	static void Init() {
		//Init render
		render = new RendererImage();
		//Init noise
		modules = new List<IModule>();
		//Init window
		window = (Edt_TextureCreator)EditorWindow.GetWindow(typeof(Edt_TextureCreator));
		window.position = windowPosition;
		window.minSize = windowMinSize;
		//window.maxSize = windowMaxSize;
		window.title = windowTitle;
		window.Show();
	}

	void OnProjectChange() {
		EditorApplication.ExecuteMenuItem("Aubergine/TextureCreator");
	}

	void OnGUI() {
		//Whole Window Begin
		GUILayout.BeginHorizontal();
		//First Column Begin
		GUILayout.BeginVertical(GUILayout.Width(150), GUILayout.Height(512));
		//Modules List Begin
		GUILayout.BeginVertical("box");
		togModuleList = EditorGUILayout.Foldout(togModuleList, "Modules(.Type)");
		if(togModuleList) {
			scrlModuleList = GUILayout.BeginScrollView(scrlModuleList);
			//List all modules as label
			for(int i = 0; i < modules.Count; i++) {
				string str = modules[i].GetType().ToString();
				int n = str.LastIndexOf(".");
				str = str.Substring(n, str.Length - n);
				GUILayout.Label("Module " + i.ToString() + "(" + str + ")");
			}
			GUILayout.EndScrollView();
		}
		//Modules List End
		GUILayout.EndVertical();
		//Gradients List Begin
		GUILayout.BeginVertical("box");
		togGradientList = EditorGUILayout.Foldout(togGradientList, "Gradients");
		if(togGradientList) {
			scrlGradientList = GUILayout.BeginScrollView(scrlGradientList);
			//List all modules as label
			for(int i = 0; i < render.gradient.Gradient.Count; i++) {
				GUILayout.BeginHorizontal();
				EditorGUILayout.FloatField((float)render.GetPositionAtPos(i));
				EditorGUILayout.ColorField(render.GetColorAtPos(i));
				GUILayout.EndHorizontal();
			}
			GUILayout.EndScrollView();
			if (GUILayout.Button("Delete Gradient")) {
				if(render != null) {
					render.ClearGradient();
				}
			}
			GUILayout.BeginHorizontal();
			gradPos = (double)EditorGUILayout.FloatField((float)gradPos);
			gradientColor = EditorGUILayout.ColorField(gradientColor);
			GUILayout.EndHorizontal();
			if (GUILayout.Button("Add Gradient")) {
				if(render != null) {
					render.AddGradientPoint(gradPos, gradientColor);
				}
			}
		}
		//Gradients List End
		GUILayout.EndVertical();
		//First Column End
		GUILayout.EndVertical();

		//Second Column Begin
		GUILayout.BeginVertical(GUILayout.Width(300), GUILayout.Height(512));
		//Texture Options Begin
		GUILayout.BeginVertical("box");
		togTextureOptions = EditorGUILayout.Foldout(togTextureOptions, "Texture Options");
		if(togTextureOptions) {
			popMapTypes = (MAPTYPE)EditorGUILayout.EnumPopup("Choose Map Type: ", popMapTypes);
			if((MAPTYPE)popMapTypes == MAPTYPE.Planar)
				seamlessEnabled = EditorGUILayout.Toggle("Is Seamless: ", seamlessEnabled);
			textureName = EditorGUILayout.TextField("Name: ", textureName);
			textureWidth = EditorGUILayout.IntField("Width: ", textureWidth);
			textureHeight = EditorGUILayout.IntField("Height: ", textureHeight);
		}
		//Texture Options End
		GUILayout.EndVertical();
		//Module Options Begin
		GUILayout.BeginVertical("box");
		togModuleOptions = EditorGUILayout.Foldout(togModuleOptions, "Module Options");
		if(togModuleOptions) {
			popModules = (MODULES)EditorGUILayout.EnumPopup("Add a Module: ", popModules);
			//List appropriate module properties
			switch((MODULES)popModules) {
				case MODULES.Abs:
					mod0 = EditorGUILayout.IntField("Source Module: ", mod0);
				break;
				case MODULES.Add:
					mod0 = EditorGUILayout.IntField("Source Module0: ", mod0);
					mod1 = EditorGUILayout.IntField("Source Module1: ", mod1);
				break;
				case MODULES.Billow:
					frequency = (double)EditorGUILayout.FloatField("Frequency: ", (float)frequency);
					lacunarity = (double)EditorGUILayout.FloatField("Lacunarity: ", (float)lacunarity);
					noiseQuality = EditorGUILayout.EnumPopup("NoiseQuality: ", noiseQuality);
					octaveCount = EditorGUILayout.IntField("OctaveCount: ", octaveCount);
					persistence = (double)EditorGUILayout.FloatField("Persistence: ", (float)persistence);
					seed = EditorGUILayout.IntField("Seed: ", seed);
				break;
				case MODULES.Blend:
					mod0 = EditorGUILayout.IntField("Source Module0: ", mod0);
					mod1 = EditorGUILayout.IntField("Source Module1: ", mod1);
					mod2 = EditorGUILayout.IntField("Control Module: ", mod2);
				break;
				case MODULES.CheckerBoard:
					
				break;
				case MODULES.Clamp:
					mod0 = EditorGUILayout.IntField("Source Module0: ", mod0);
					lowerBound = (double)EditorGUILayout.FloatField("Lower Bound: ", (float)lowerBound);
					upperBound = (double)EditorGUILayout.FloatField("Upper Bound: ", (float)upperBound);
				break;
				case MODULES.Const:
					tempVal1 = (double)EditorGUILayout.FloatField("Constant Value: ", (float)tempVal1);
				break;
				case MODULES.Cylinders:
					frequency = (double)EditorGUILayout.FloatField("Frequency: ", (float)frequency);
				break;
				case MODULES.Exponent:
					mod0 = EditorGUILayout.IntField("Source Module0: ", mod0);
					tempVal1 = (double)EditorGUILayout.FloatField("Exponent Value: ", (float)tempVal1);
				break;
				case MODULES.Invert:
					mod0 = EditorGUILayout.IntField("Source Module0: ", mod0);
				break;
				case MODULES.Max:
					mod0 = EditorGUILayout.IntField("Source Module0: ", mod0);
					mod1 = EditorGUILayout.IntField("Source Module1: ", mod1);
				break;
				case MODULES.Min:
					mod0 = EditorGUILayout.IntField("Source Module0: ", mod0);
					mod1 = EditorGUILayout.IntField("Source Module1: ", mod1);
				break;
				case MODULES.Multiply:
					mod0 = EditorGUILayout.IntField("Source Module0: ", mod0);
					mod1 = EditorGUILayout.IntField("Source Module1: ", mod1);
				break;
				case MODULES.Perlin:
					frequency = (double)EditorGUILayout.FloatField("Frequency: ", (float)frequency);
					lacunarity = (double)EditorGUILayout.FloatField("Lacunarity: ", (float)lacunarity);
					noiseQuality = EditorGUILayout.EnumPopup("NoiseQuality: ", noiseQuality);
					octaveCount = EditorGUILayout.IntField("OctaveCount: ", octaveCount);
					persistence = (double)EditorGUILayout.FloatField("Persistence: ", (float)persistence);
					seed = EditorGUILayout.IntField("Seed: ", seed);
				break;
				case MODULES.Power:
					mod0 = EditorGUILayout.IntField("Source Module0: ", mod0);
					mod1 = EditorGUILayout.IntField("Source Module1: ", mod1);
				break;
				case MODULES.RidgedMulti:
					frequency = (double)EditorGUILayout.FloatField("Frequency: ", (float)frequency);
					lacunarity = (double)EditorGUILayout.FloatField("Lacunarity: ", (float)lacunarity);
					noiseQuality = EditorGUILayout.EnumPopup("NoiseQuality: ", noiseQuality);
					octaveCount = EditorGUILayout.IntField("OctaveCount: ", octaveCount);
					seed = EditorGUILayout.IntField("Seed: ", seed);
				break;
				case MODULES.RotatePoint:
					mod0 = EditorGUILayout.IntField("Source Module0: ", mod0);
					tempVal1 = (double)EditorGUILayout.FloatField("X Angle: ", (float)tempVal1);
					tempVal2 = (double)EditorGUILayout.FloatField("Y Angle: ", (float)tempVal2);
					tempVal3 = (double)EditorGUILayout.FloatField("Z Angle: ", (float)tempVal3);
				break;
				case MODULES.ScaleBias:
					mod0 = EditorGUILayout.IntField("Source Module0: ", mod0);
					tempVal1 = (double)EditorGUILayout.FloatField("Scale: ", (float)tempVal1);
					tempVal2 = (double)EditorGUILayout.FloatField("Bias: ", (float)tempVal2);
				break;
				case MODULES.ScalePoint:
					mod0 = EditorGUILayout.IntField("Source Module0: ", mod0);
					tempVal1 = (double)EditorGUILayout.FloatField("X Scale: ", (float)tempVal1);
					tempVal2 = (double)EditorGUILayout.FloatField("Y Scale: ", (float)tempVal2);
					tempVal3 = (double)EditorGUILayout.FloatField("Z Scale: ", (float)tempVal3);
				break;
				case MODULES.Select:
					mod0 = EditorGUILayout.IntField("Source Module0: ", mod0);
					mod1 = EditorGUILayout.IntField("Source Module1: ", mod1);
					mod2 = EditorGUILayout.IntField("Control Module: ", mod2);
					tempVal1 = (double)EditorGUILayout.FloatField("Edge Falloff: ", (float)tempVal1);
					lowerBound = (double)EditorGUILayout.FloatField("Lower Bound: ", (float)lowerBound);
					upperBound = (double)EditorGUILayout.FloatField("Upper Bound: ", (float)upperBound);
				break;
				case MODULES.Spheres:
					frequency = (double)EditorGUILayout.FloatField("Frequency: ", (float)frequency);
				break;
				case MODULES.TranslatePoint:
					mod0 = EditorGUILayout.IntField("Source Module0: ", mod0);
					tempVal1 = (double)EditorGUILayout.FloatField("X Translation: ", (float)tempVal1);
					tempVal2 = (double)EditorGUILayout.FloatField("Y Translation: ", (float)tempVal2);
					tempVal3 = (double)EditorGUILayout.FloatField("Z Translation: ", (float)tempVal3);
				break;
				case MODULES.Turbulence:
					mod0 = EditorGUILayout.IntField("Source Module0: ", mod0);
					power = (double)EditorGUILayout.FloatField("Power: ", (float)power);
					frequency = (double)EditorGUILayout.FloatField("Frequency: ", (float)frequency);
					roughness = EditorGUILayout.IntField("Roughness: ", roughness);
					seed = EditorGUILayout.IntField("Seed: ", seed);
				break;
				case MODULES.Voronoi:
					frequency = (double)EditorGUILayout.FloatField("Frequency: ", (float)frequency);
					tempVal1 = (double)EditorGUILayout.FloatField("Displacement: ", (float)tempVal1);
					tempBool = EditorGUILayout.Toggle("Enable Distance: ", tempBool);
					seed = EditorGUILayout.IntField("Seed: ", seed);
				break;
				default:
					
				break;
			}
			GUILayout.BeginHorizontal();
			mod2 = EditorGUILayout.IntField("Module to Delete: ", mod2);
			if (GUILayout.Button("Delete Module")) {
				if(modules != null && modules.Count > mod2) {
					modules.RemoveAt(mod2);
				}
			}
			//Add to modules button
			if (GUILayout.Button("Add To List")) {
				if(modules != null) {
					switch((MODULES)popModules) {
						case MODULES.Abs:
							if(modules.Count > mod0) {
								selectedModule = new Abs();
								((Abs)selectedModule).Module0 = modules[mod0];
							}
						break;
						case MODULES.Add:
							if(modules.Count > mod0 && modules.Count > mod1) {
								selectedModule = new Add();
								((Add)selectedModule).Module0 = modules[mod0];
								((Add)selectedModule).Module1 = modules[mod1];
							}
						break;
						case MODULES.Billow:
							selectedModule = new Billow();
							((Billow)selectedModule).Frequency = frequency;
							((Billow)selectedModule).Lacunarity = lacunarity;
							((Billow)selectedModule).NoiseQuality = (NoiseQuality)noiseQuality;
							((Billow)selectedModule).OctaveCount = octaveCount;
							((Billow)selectedModule).Persistence = persistence;
							((Billow)selectedModule).Seed = seed;
						break;
						case MODULES.Blend:
							if(modules.Count > mod0 && modules.Count > mod1 && modules.Count > mod2) {
								selectedModule = new Blend();
								((Blend)selectedModule).Module0 = modules[mod0];
								((Blend)selectedModule).Module1 = modules[mod1];
								((Blend)selectedModule).ModuleA = modules[mod2];
							}
						break;
						case MODULES.CheckerBoard:
							selectedModule = new CheckerBoard();
						break;
						case MODULES.Clamp:
							if(modules.Count > mod0) {
								selectedModule = new Clamp();
								((Clamp)selectedModule).Module0 = modules[mod0];
								((Clamp)selectedModule).UpperBound = upperBound;
								((Clamp)selectedModule).LowerBound = lowerBound;
							}
						break;
						case MODULES.Const:
							selectedModule = new Const();
							((Const)selectedModule).ConstantValue = tempVal1;
						break;
						case MODULES.Cylinders:
							selectedModule = new Cylinders();
							((Cylinders)selectedModule).Frequency = frequency;
						break;
						case MODULES.Exponent:
							if(modules.Count > mod0) {
								selectedModule = new Exponent();
								((Exponent)selectedModule).Module0 = modules[mod0];
								((Exponent)selectedModule).ExponentVal = tempVal1;
							}
						break;
						case MODULES.Invert:
							if(modules.Count > mod0) {
								selectedModule = new Invert();
								((Invert)selectedModule).Module0 = modules[mod0];
							}
						break;
						case MODULES.Max:
							if(modules.Count > mod0 && modules.Count > mod1) {
								selectedModule = new Max();
								((Max)selectedModule).Module0 = modules[mod0];
								((Max)selectedModule).Module1 = modules[mod1];
							}
						break;
						case MODULES.Min:
							if(modules.Count > mod0 && modules.Count > mod1) {
								selectedModule = new Min();
								((Min)selectedModule).Module0 = modules[mod0];
								((Min)selectedModule).Module1 = modules[mod1];
							}
						break;
						case MODULES.Multiply:
							if(modules.Count > mod0 && modules.Count > mod1) {
								selectedModule = new Multiply();
								((Multiply)selectedModule).Module0 = modules[mod0];
								((Multiply)selectedModule).Module1 = modules[mod1];
							}
						break;
						case MODULES.Perlin:
							selectedModule = new Perlin();
							((Perlin)selectedModule).Frequency = frequency;
							((Perlin)selectedModule).Lacunarity = lacunarity;
							((Perlin)selectedModule).NoiseQuality = (NoiseQuality)noiseQuality;
							((Perlin)selectedModule).OctaveCount = octaveCount;
							((Perlin)selectedModule).Persistence = persistence;
							((Perlin)selectedModule).Seed = seed;
						break;
						case MODULES.Power:
							if(modules.Count > mod0 && modules.Count > mod1) {
								selectedModule = new Power();
								((Power)selectedModule).Module0 = modules[mod0];
								((Power)selectedModule).Module1 = modules[mod1];
							}
						break;
						case MODULES.RidgedMulti:
							selectedModule = new RidgedMulti();
							((RidgedMulti)selectedModule).Frequency = frequency;
							((RidgedMulti)selectedModule).Lacunarity = lacunarity;
							((RidgedMulti)selectedModule).NoiseQuality = (NoiseQuality)noiseQuality;
							((RidgedMulti)selectedModule).OctaveCount = octaveCount;
							((RidgedMulti)selectedModule).Seed = seed;
						break;
						case MODULES.RotatePoint:
							if(modules.Count > mod1) {
								selectedModule = new RotatePoint();
								((RotatePoint)selectedModule).Module0 = modules[mod0];
								((RotatePoint)selectedModule).SetAngles(tempVal1, tempVal2, tempVal3);
							}
						break;
						case MODULES.ScaleBias:
							if(modules.Count > mod1) {
								selectedModule = new ScaleBias();
								((ScaleBias)selectedModule).Module0 = modules[mod0];
								((ScaleBias)selectedModule).Scale = tempVal1;
								((ScaleBias)selectedModule).Bias = tempVal2;
							}
						break;
						case MODULES.ScalePoint:
							if(modules.Count > mod1) {
								selectedModule = new ScalePoint();
								((ScalePoint)selectedModule).Module0 = modules[mod0];
								((ScalePoint)selectedModule).XScale = tempVal1;
								((ScalePoint)selectedModule).YScale = tempVal2;
								((ScalePoint)selectedModule).ZScale = tempVal3;
							}
						break;
						case MODULES.Select:
							if(modules.Count > mod0 && modules.Count > mod1 && modules.Count > mod2) {
								selectedModule = new Select();
								((Select)selectedModule).Module0 = modules[mod0];
								((Select)selectedModule).Module1 = modules[mod1];
								((Select)selectedModule).ModuleC = modules[mod2];
								((Select)selectedModule).SetBounds(lowerBound, upperBound);
								((Select)selectedModule).SetEdgeFallOff(tempVal1);
							}
						break;
						case MODULES.Spheres:
							selectedModule = new Spheres();
							((Spheres)selectedModule).Frequency = frequency;
						break;
						case MODULES.TranslatePoint:
							if(modules.Count > mod0) {
								selectedModule = new TranslatePoint();
								((TranslatePoint)selectedModule).Module0 = modules[mod0];
								((TranslatePoint)selectedModule).XTranslation = tempVal1;
								((TranslatePoint)selectedModule).YTranslation = tempVal2;
								((TranslatePoint)selectedModule).ZTranslation = tempVal3;
							}
						break;
						case MODULES.Turbulence:
							if(modules.Count > mod0) {
								selectedModule = new Turbulence();
								((Turbulence)selectedModule).Module0 = modules[mod0];
								((Turbulence)selectedModule).Power = power;
								((Turbulence)selectedModule).Frequency = frequency;
								((Turbulence)selectedModule).Roughness = roughness;
								((Turbulence)selectedModule).Seed = seed;
							}
						break;
						case MODULES.Voronoi:
							selectedModule = new Voronoi();
							((Voronoi)selectedModule).Frequency = frequency;
							((Voronoi)selectedModule).Displacement = tempVal1;
							((Voronoi)selectedModule).EnableDistance = tempBool;
							((Voronoi)selectedModule).Seed = seed;
						break;
						default:
							selectedModule = null;
						break;
					}
				}
				if(selectedModule != null) {
					modules.Add(selectedModule);
					//Reset variables
					selectedModule = null;
					mod0 = 0;
					mod1 = 0;
					mod2 = 0;
					frequency = 1.0;
					lacunarity = 2.0;
					noiseQuality = NoiseQuality.Standard;
					octaveCount = 6;
					persistence = 0.5;
					seed = 0;
					lowerBound = -1.0;
					upperBound = 1.0;
					tempVal1 = 0;
					tempVal2 = 0;
					tempVal3 = 0;
					power = 1.0;
					roughness = 3;
					tempBool = true;
					//window.Repaint();
				}
			}
			GUILayout.EndHorizontal();
		}
		//Module Options End
		GUILayout.EndVertical();
		//Render Options Begin
		GUILayout.BeginVertical("box");
		togRenderOptions = EditorGUILayout.Foldout(togRenderOptions, "Render Options");
		if(togRenderOptions) {
			GUILayout.BeginHorizontal();
			lowerXBound = (double)EditorGUILayout.FloatField("LowerX Bound: ", (float)lowerXBound);
			upperXBound = (double)EditorGUILayout.FloatField("UpperX Bound: ", (float)upperXBound);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			lowerZBound = (double)EditorGUILayout.FloatField("LowerZ Bound: ", (float)lowerZBound);
			upperZBound = (double)EditorGUILayout.FloatField("UpperZ Bound: ", (float)upperZBound);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			lightEnabled = EditorGUILayout.Toggle("Lighting: ", lightEnabled);
			wrapEnabled = EditorGUILayout.Toggle("Texture Wrap: ", wrapEnabled);
			GUILayout.EndHorizontal();
			if(lightEnabled) {
				lightAzimuth = (double)EditorGUILayout.FloatField("Light Azimuth: ", (float)lightAzimuth);
				lightElevation = (double)EditorGUILayout.FloatField("Light Elevation: ", (float)lightElevation);
				lightBrightness = (double)EditorGUILayout.FloatField("Light Brightness: ", (float)lightBrightness);
				lightContrast = (double)EditorGUILayout.FloatField("Light Contrast: ", (float)lightContrast);
				lightIntensity = (double)EditorGUILayout.FloatField("Light Intensity: ", (float)lightIntensity);
				lightColor = EditorGUILayout.ColorField("Light Color: ", lightColor);
			}
			bumpHeight = (double)EditorGUILayout.FloatField("Bump Height: ", (float)bumpHeight);
			backgroundTexture = (Texture2D)EditorGUILayout.ObjectField("Background Texture: ", backgroundTexture, typeof(Texture2D), false);
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Render Colors")) {
				if(modules != null && modules.Count > 0) {
					finalTexture = RenderTex(modules[modules.Count-1]);
				}
			}
			if (GUILayout.Button("Render Normals")) {
				if(modules != null && modules.Count > 0) {
					finalTexture = RenderNorm(modules[modules.Count-1]);
				}
			}
			if (GUILayout.Button("Save Texture")) {
				if(finalTexture != null) {
					//You can save your texture here
					byte[] bytes = finalTexture.EncodeToPNG();
					string file = textureName + ".png";
					File.WriteAllBytes(Application.dataPath + "/" + file, bytes);
					AssetDatabase.Refresh();
					Debug.LogWarning("Texture is Saved.");
				}
			}
			if (GUILayout.Button("Set Terrain Heights")) {
				if(modules != null) {
					if(!terrain) {
						if(Selection.activeGameObject != null) {
							terrain = Selection.activeGameObject.GetComponent<Terrain>();
						}
						else if(Terrain.activeTerrain != null) {
							terrain = Terrain.activeTerrain;
						}
						else
							Debug.Log("You must select an active terrain from the Hierarchy.");
					}
					else {
						SetTerrainHeights(modules[modules.Count-1]);
					}
				}
			}
			GUILayout.EndHorizontal();
		}
		//Render Options End
		GUILayout.EndVertical();
		//Second Column End
		GUILayout.EndVertical();
		//Third Column Begin
		GUILayout.BeginVertical("box");
		GUILayout.Label(finalTexture, GUILayout.Width(textureWidth), GUILayout.Height(textureHeight), GUILayout.MaxWidth(512), GUILayout.MaxHeight(512));
		//Third Column End
		GUILayout.EndVertical();
		//Whole Window End
		GUILayout.EndHorizontal();
	}

	private Texture2D RenderTex(IModule module) {
		switch((MAPTYPE)popMapTypes) {
			case MAPTYPE.Cylindirical:
				NoiseMapBuilderCylinder buildCylindirical = new NoiseMapBuilderCylinder(textureWidth, textureHeight);
				buildCylindirical.SetBounds(lowerXBound, upperXBound, lowerZBound, upperZBound);
				buildCylindirical.Build(module);
				render.SourceNoiseMap = buildCylindirical.Map;
			break;
			case MAPTYPE.Spherical:
				NoiseMapBuilderSphere buildSpherical = new NoiseMapBuilderSphere(textureWidth, textureHeight);
				buildSpherical.SetBounds(lowerXBound, upperXBound, lowerZBound, upperZBound);
				buildSpherical.Build(module);
				render.SourceNoiseMap = buildSpherical.Map;
			break;
			case MAPTYPE.Planar:
				NoiseMapBuilderPlane buildPlanar = new NoiseMapBuilderPlane(textureWidth, textureHeight);
				buildPlanar.SetBounds(lowerXBound, upperXBound, lowerZBound, upperZBound);
				buildPlanar.IsSeamless = seamlessEnabled;
				buildPlanar.Build(module);
				render.SourceNoiseMap = buildPlanar.Map;
			break;
			default:
			break;
		}
		ImageMap backMap = null;
		if(backgroundTexture != null)
			backMap = new ImageMap(backgroundTexture, Color.gray);
		if(backMap != null)
			render.BackgroundImage = backMap;
		render.IsWrapEnabled = wrapEnabled;
		render.IsLightEnabled = lightEnabled;
		render.LightContrast = lightContrast;
		render.LightAzimuth = lightAzimuth;
		render.LightBrightness = lightBrightness;
		render.LightColor = lightColor;
		render.LightContrast = lightContrast;
		render.LightElev = lightElevation;
		render.LightIntensity = lightIntensity;
		render.Render();
		return render.GetTexture();
	}

	private Texture2D RenderNorm(IModule module) {
		RendererNormal render = new RendererNormal();
		switch((MAPTYPE)popMapTypes) {
			case MAPTYPE.Cylindirical:
				NoiseMapBuilderCylinder buildCylindirical = new NoiseMapBuilderCylinder(textureWidth, textureHeight);
				buildCylindirical.SetBounds(lowerXBound, upperXBound, lowerZBound, upperZBound);
				buildCylindirical.Build(module);
				render.SourceNoiseMap = buildCylindirical.Map;
			break;
			case MAPTYPE.Spherical:
				NoiseMapBuilderSphere buildSpherical = new NoiseMapBuilderSphere(textureWidth, textureHeight);
				buildSpherical.SetBounds(lowerXBound, upperXBound, lowerZBound, upperZBound);
				buildSpherical.Build(module);
				render.SourceNoiseMap = buildSpherical.Map;
			break;
			case MAPTYPE.Planar:
				NoiseMapBuilderPlane buildPlanar = new NoiseMapBuilderPlane(textureWidth, textureHeight);
				buildPlanar.SetBounds(lowerXBound, upperXBound, lowerZBound, upperZBound);
				buildPlanar.Build(module);
				render.SourceNoiseMap = buildPlanar.Map;
			break;
			default:
			break;
		}
		render.BumpHeight = bumpHeight;
		render.Render();
		return render.GetTexture();
	}

	private void SetTerrainHeights(IModule module) {
		TerrainData terData = terrain.terrainData;
		NoiseMapBuilderPlane buildPlanar = new NoiseMapBuilderPlane(terData.heightmapWidth, terData.heightmapHeight);
		buildPlanar.SetBounds(lowerXBound, upperXBound, lowerZBound, upperZBound);
		buildPlanar.Build(module);
		float[,] heights = new float[terData.heightmapWidth, terData.heightmapHeight];
		for (int z=0; z < terData.heightmapHeight; z++) {
			for (int x=0; x < terData.heightmapWidth; x++) {
				heights[x,z]  = ConvertRange01(buildPlanar.Map.GetValue(x, z));
				//heights[x,z] *= terData.size.y;
			}
		}
		terData.SetHeights(0, 0, heights);
		//terData.splatPrototypes[0].texture = finalTexture;
		Debug.Log("Water Level is: " + terData.size.y / 2.0f);
	}

	private float ConvertRange01(double val) {
		return Mathf.Clamp((float)((1.0 + val) * 0.5f), 0f, 1f);
	}
}