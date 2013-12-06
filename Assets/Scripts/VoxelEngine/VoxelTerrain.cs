using UnityEngine;
using Aubergine.Noise.Module;
using Aubergine.Noise.NoiseUtils;

public class VoxelTerrain : MonoBehaviour
{
		public bool useSeed;
		public int seed;
		public int width;
		public int depth;
		public int minimumHeight;
		public int maximumHeight;
		public Material material;
		private GameObject singleChunk;
		private NoiseMap noiseMap;
		private float halfAmplitude;
	
		void FillColumns (byte[] data, int x, int z, int height)
		{
				for (int y = 0; y < maximumHeight; y++) {
						data [y * (width * depth) + z * width + x] = (y < height) ? (byte)1 : (byte)0;
				}
		}
		
		int GetHeight (int x, int z)
		{
				return Mathf.RoundToInt (noiseMap.GetValue (x, z) * halfAmplitude + halfAmplitude + minimumHeight);
		}
	
		public void Clear ()
		{
				if (singleChunk != null) {
						DestroyImmediate (singleChunk);
				}
		}
	
		public void Build ()
		{
				if (width < 1 || depth < 1 || maximumHeight < 1) {
						Debug.LogError ("width or depth or maximumHeight < 1");
						enabled = false;
						return;
				}
		
				float halfWidth = width * 0.5f;
				float halfDepth = depth * 0.5f;
				Perlin perlin = new Perlin ();
				perlin.Frequency = 0.5;
				perlin.Persistence = 0.25;
				if (useSeed) {
						perlin.Seed = seed;
				}
				NoiseMapBuilderPlane heightMapBuilder = new NoiseMapBuilderPlane (width, depth);
				heightMapBuilder.SetBounds (-halfWidth, halfWidth, -halfDepth, halfDepth);
				heightMapBuilder.Build (perlin);
				noiseMap = heightMapBuilder.Map;
				halfAmplitude = (maximumHeight - minimumHeight) * 0.5f;
				byte[] data = new byte[maximumHeight * depth * width];
				for (int z = 0; z < depth; z++) {
						for (int x = 0; x < width; x++) {
								FillColumns (data, x, z, GetHeight (x, z));
						}
				}
				
				Clear ();
				
				// TODO: optimize
				singleChunk = new GameObject ("Single Chunk");
				singleChunk.hideFlags = HideFlags.HideInHierarchy;
				singleChunk.transform.parent = transform;
				singleChunk.transform.localPosition = Vector3.zero;
				singleChunk.transform.localRotation = Quaternion.identity;
				VoxelChunk chunk = singleChunk.AddComponent<VoxelChunk> ();
				chunk.width = width;
				chunk.depth = depth;
				chunk.height = maximumHeight;
				chunk.material = material;
				chunk.data = data;
				chunk.Build ();
		}
	
}
