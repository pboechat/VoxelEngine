using UnityEngine;
using System;
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
		public float scale;
		public float smoothness;
		public bool buildOnStart;
		private NoiseMap noiseMap;
		private float halfAmplitude;
		[SerializeField]
		private int
				realWidth;
		[SerializeField]
		private int
				realDepth;
		[SerializeField]
		private int
				gridWidth;
		[SerializeField]
		private int
				gridDepth;
		[SerializeField]
		private int
				gridHeight;
		[SerializeField]
		private Vector3
				sectorOffset;
		[SerializeField]
		private VoxelChunk[]
				grid;
	
		void Start ()
		{
				if (buildOnStart) {
						Build ();
				}
		}
	
		void FillColumns (byte[] data, int x, int z, int height)
		{
				for (int y = 0; y < 12; y++) {
						data [y * 144 + z * 12 + x] = (y < height) ? (byte)1 : (byte)0;
				}
		}
		
		int GetHeight (int x, int z)
		{
				return Mathf.RoundToInt (noiseMap.GetValue (x, z) * halfAmplitude + halfAmplitude + minimumHeight);
		}
	
		public void Clear ()
		{
				if (grid != null) {
						for (int i = 0; i < grid.Length; i++) {
								if (grid [i] == null) {
										continue;
								}
								DestroyImmediate (grid [i].gameObject);
						}
						grid = null;
				}
		}
		
		VoxelChunk CreateChunk (int xGrid, int yGrid, int zGrid, Vector3 position, byte[] data)
		{
				GameObject sector = new GameObject ("Sector (" + xGrid + ", " + yGrid + ", " + zGrid + ")");
				//sector.hideFlags = HideFlags.HideInHierarchy;
				sector.transform.parent = transform;
				sector.transform.localPosition = position; //sectorOffset + new Vector3 (xOffset * VoxelEngine.instance.voxelSize, yGrid * 12 * VoxelEngine.instance.voxelSize, zOffset * VoxelEngine.instance.voxelSize);
				sector.transform.localRotation = Quaternion.identity;
				VoxelChunk chunk = sector.AddComponent<VoxelChunk> ();
				chunk.terrain = this;
				chunk.x = xGrid;
				chunk.y = yGrid;
				chunk.z = zGrid;
				chunk.width = 12;
				chunk.depth = 12;
				chunk.height = 12;
				chunk.data = data;
				chunk.Build (true);
				return chunk;
		}
	
		public void Build ()
		{
				if (width < 1 || depth < 1 || maximumHeight < 1) {
						Debug.LogError ("width or depth or maximumHeight < 1");
						enabled = false;
						return;
				}
				
				Clear ();
				
				float naturalScale = 1.0f / Mathf.Sqrt ((float)width * depth);
				
				gridWidth = (width + 11) / 12;
				gridDepth = (depth + 11) / 12;
				gridHeight = (maximumHeight + 11) / 12;
				
				realWidth = gridWidth * 12;
				realDepth = gridDepth * 12;
		
				RidgedMulti mountainTerrain = new RidgedMulti ();
				mountainTerrain.Frequency = naturalScale * scale;
				Billow baseFlatTerrain = new Billow ();
				baseFlatTerrain.Frequency = naturalScale * scale * 2.0f;
				ScaleBias flatTerrain = new ScaleBias (baseFlatTerrain, 0.125, -0.75);
				Perlin terrainType = new Perlin ();
				terrainType.Frequency = naturalScale * scale * 0.5f;
				terrainType.Persistence = 0.25;
				if (!useSeed) {
						seed = (int)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
				}
				terrainType.Seed = seed;
				Select finalTerrain = new Select (flatTerrain, mountainTerrain, terrainType);
				finalTerrain.SetBounds (0.0, 1000.0);
				finalTerrain.SetEdgeFallOff (smoothness);
				
				NoiseMapBuilderPlane heightMapBuilder = new NoiseMapBuilderPlane (realWidth, realDepth);
				heightMapBuilder.SetBounds (0, realWidth, 0, realDepth);
				heightMapBuilder.Build (finalTerrain);
				
				noiseMap = heightMapBuilder.Map;
				
				halfAmplitude = (maximumHeight - minimumHeight) * 0.5f;
				
				sectorOffset = new Vector3 (-gridWidth / 2 * 12 * VoxelEngine.instance.voxelSize, 0.0f, -gridDepth / 2 * 12 * VoxelEngine.instance.voxelSize);
				
				grid = new VoxelChunk[gridHeight * gridDepth * gridWidth];
				for (int zGrid = 0; zGrid < gridDepth; zGrid++) {
						int zOffset = zGrid * 12;
						for (int xGrid = 0; xGrid < gridWidth; xGrid++) {
								int xOffset = xGrid * 12;
								for (int yGrid = 0, minHeight = 0; yGrid < gridHeight; yGrid++, minHeight += 12) {
										byte[] data = new byte[1728];
										bool filled = false;
										for (int z = 0; z < 12; z++) {
												for (int x = 0; x < 12; x++) {
														int height = GetHeight (xOffset + x, zOffset + z);
														int sectorHeight = Mathf.Max (0, height - minHeight);
														if (sectorHeight > 0 && !filled) {
																filled = true;
														}
														FillColumns (data, x, z, sectorHeight);
												}
										}
									
										VoxelChunk chunk;
										if (filled) {
												Vector3 position = sectorOffset + new Vector3 (xOffset * VoxelEngine.instance.voxelSize, yGrid * 12 * VoxelEngine.instance.voxelSize, zOffset * VoxelEngine.instance.voxelSize);
												chunk = CreateChunk (xGrid, yGrid, zGrid, position, data);
										} else {
												chunk = null;
										}
										grid [yGrid * gridWidth * gridDepth + zGrid * gridWidth + xGrid] = chunk;
								}
						}
				}
		}
		
		void ResizeGrid (int newGridWidth, int newGridHeight, int newGridDepth)
		{
				VoxelChunk[] newGrid = new VoxelChunk[newGridHeight * newGridDepth * newGridWidth]; 
				int i = 0;
				for (int y = 0; y < gridHeight; y++) {
						int y1 = y * newGridDepth * newGridWidth;
						for (int z = 0; z < gridDepth; z++) {
								int z1 = z * newGridWidth;
								for (int x = 0; x < gridWidth; x++) {
										newGrid [y1 + z1 + x] = grid [i++];
								}
						}
				}
				gridWidth = newGridWidth;
				gridHeight = newGridHeight;
				gridDepth = newGridDepth;
				grid = null;
				grid = newGrid;
		}
  
		public VoxelChunk GetChunkAbove (int xGrid, int yGrid, int zGrid)
		{
				int yGrid2 = yGrid + 1;
				int height = yGrid2 + 1;
				if (height > gridHeight) {
						ResizeGrid (gridWidth, height, gridDepth);
				}
				
				int i = yGrid2 * gridWidth * gridDepth + zGrid * gridWidth + xGrid;
				VoxelChunk chunk = grid [i];
				if (chunk == null) {
						byte[] data = new byte[1728];
						Array.Clear (data, 0, 1728);
						Vector3 position = sectorOffset + new Vector3 (xGrid * 12 * VoxelEngine.instance.voxelSize, yGrid2 * 12 * VoxelEngine.instance.voxelSize, zGrid * 12 * VoxelEngine.instance.voxelSize);
						chunk = CreateChunk (xGrid, yGrid2, zGrid, position, data);
						grid [i] = chunk;
				}
				return chunk;
		}
}
