using UnityEngine;
using System;
using System.Collections.Generic;
using Aubergine.Noise.Module;
using Aubergine.Noise.NoiseUtils;

public class VoxelTerrain : MonoBehaviour
{
	public enum ResizeDirection
	{
		NONE,
		HORIZONTAL,
		VERTICAL,
		HORIZONTAL_AND_VERTICAL
	}

	public bool useSeed;
	public int seed;
	public int width;
	public int depth;
	public int height;
	public int minimumHeight;
	public int maximumHeight;
	public float scale;
	public float smoothness;
	public bool buildOnStart;
	public ResizeDirection resizeDirection;
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
		gridCenter;
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
	
	public void ExecuteQuery (VoxelQuery query)
	{
		Vector3 localPosition = transform.worldToLocalMatrix.MultiplyPoint3x4 (query.position);
		Vector3 queryCenter = new Vector3 (Mathf.FloorToInt (localPosition.x) + VoxelEngine.instance.halfVoxelSize,
		                                   Mathf.FloorToInt (localPosition.y) + VoxelEngine.instance.halfVoxelSize,
		                                   Mathf.FloorToInt (localPosition.z) + VoxelEngine.instance.halfVoxelSize);
		int gridDepth_x_gridWidth = gridDepth * gridWidth;
		int i = 0;
		float queryYOffset = (-query.height * 0.5f) * VoxelEngine.instance.voxelSize;
		float queryZOffset = (-query.depth * 0.5f) * VoxelEngine.instance.voxelSize;
		float queryXOffset = (-query.width * 0.5f) * VoxelEngine.instance.voxelSize;
		float chunkSide = VoxelEngine.instance.voxelSize * 12;
		Vector3 gridOffset = new Vector3 (width * 0.5f, 0.0f, depth * 0.5f);
		Vector3 yStart = new Vector3 (0.0f, queryYOffset, 0.0f);
		for (int y = 0; y < query.height; y++) {
			Vector3 zStart = new Vector3 (0.0f, 0.0f, queryZOffset);
			for (int z = 0; z < query.depth; z++) {
				Vector3 xStart = new Vector3 (queryXOffset, 0.0f, 0.0f);
				for (int x = 0; x < query.width; x++) {
					if (query.mask [i++]) {
						Vector3 voxelCenter = queryCenter + yStart + zStart + xStart;
						Vector3 gridPosition = gridOffset + voxelCenter;
						int chunkX = Mathf.FloorToInt (gridPosition.x / chunkSide);
						int chunkY = Mathf.FloorToInt (gridPosition.y / chunkSide);
						int chunkZ = Mathf.FloorToInt (gridPosition.z / chunkSide);
						
						// ignore queries outside of terrain bounds
						if (chunkX < 0 || chunkX >= gridWidth || chunkY < 0 || chunkY >= gridHeight || chunkZ < 0 || chunkZ >= gridDepth) {
							continue;
						}
						
						VoxelChunk chunk = grid [chunkY * gridDepth_x_gridWidth + chunkZ * gridWidth + chunkX];
						if (chunk != null) {
							int voxelX, voxelY, voxelZ;
							if (chunk.QueryVoxel (voxelCenter, out voxelX, out voxelY, out voxelZ)) {
								query.Execute (chunk, voxelX, voxelY, voxelZ);
							}
						}
					}
					xStart += VoxelEngine.instance.right;
				}
				zStart += VoxelEngine.instance.forward;
			}
			yStart += VoxelEngine.instance.up;
		}
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
		
	VoxelChunk BuildChunk (int x, int y, int z, Vector3 position, byte[] data)
	{
		GameObject gameObject = new GameObject ("Chunk (" + x + ", " + y + ", " + z + ")");
		gameObject.hideFlags = HideFlags.HideInHierarchy;
		gameObject.transform.parent = transform;
		gameObject.transform.localPosition = position;
		gameObject.transform.localRotation = Quaternion.identity;
		VoxelChunk chunk = gameObject.AddComponent<VoxelChunk> ();
		chunk.terrain = this;
		chunk.x = x;
		chunk.y = y;
		chunk.z = z;
		chunk.width = 12;
		chunk.depth = 12;
		chunk.height = 12;
		chunk.data = data;
		chunk.Build ();
		return chunk;
	}
	
	public void Build ()
	{
		if (width < 1 || depth < 1 || height < 1 || maximumHeight < 1) {
			Debug.LogError ("width or depth or height or maximumHeight < 1");
			enabled = false;
			return;
		}
		
		Clear ();
				
		float naturalScale = 1.0f / Mathf.Sqrt ((float)width * depth);
				
		gridWidth = (width + 11) / 12;
		gridDepth = (depth + 11) / 12;
		gridHeight = (height + 11) / 12;
				
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
				
		gridCenter = new Vector3 (-width * 0.5f, 0.0f, -depth * 0.5f);
		
		float chunkSide = 12 * VoxelEngine.instance.voxelSize;
		float halfChunkSide = chunkSide * 0.5f;
		
		grid = new VoxelChunk[gridHeight * gridDepth * gridWidth];
		int gridDepth_x_gridWidth = gridWidth * gridDepth;
		for (int chunkZ = 0; chunkZ < gridDepth; chunkZ++) {
			int chunkZ1 = chunkZ * gridWidth;
			int chunkDepth = chunkZ * 12;
			for (int chunkX = 0; chunkX < gridWidth; chunkX++) {
				int chunkWidth = chunkX * 12;
				for (int chunkY = 0, chunkHeight = 0; chunkY < gridHeight; chunkY++, chunkHeight += 12) {
					byte[] data = new byte[1728];
					bool filled = false;
					for (int z = 0; z < 12; z++) {
						int currentZ = chunkDepth + z;
						for (int x = 0; x < 12; x++) {
							int currentX = chunkWidth + x;
							
							if (currentZ >= depth || currentX >= width) {
								FillColumns (data, x, z, 0);
								continue;
							}
							
							int terrainHeight = GetHeight (currentX, currentZ);
							int fillHeight = Mathf.Max (0, terrainHeight - chunkHeight);
							if (fillHeight > 0 && !filled) {
								filled = true;
							}
							FillColumns (data, x, z, fillHeight);
						}
					}
					
					VoxelChunk chunk;
					if (filled) {
						Vector3 chunkPosition = gridCenter + new Vector3 (chunkWidth * VoxelEngine.instance.voxelSize + halfChunkSide, chunkY * chunkSide + halfChunkSide, chunkDepth * VoxelEngine.instance.voxelSize + halfChunkSide);
						chunk = BuildChunk (chunkX, chunkY, chunkZ, chunkPosition, data);
					} else {
						chunk = null;
					}
					grid [chunkY * gridDepth_x_gridWidth + chunkZ1 + chunkX] = chunk;
				}
			}
		}
	}
		
	void ResizeGrid (int newGridWidth, int newGridHeight, int newGridDepth)
	{
		// DEBUG:
		//Debug.Log ("- Resizing grid: from (" + gridWidth + ", " + gridHeight + ", " + gridDepth + ") to (" + newGridWidth + ", " + newGridHeight + ", " + newGridDepth + ")");

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
  
	public VoxelChunk GetChunkAtRight (int xGrid, int yGrid, int zGrid)
	{
		int xGrid2 = xGrid + 1;
		int gridWidthBounds = xGrid2 + 1;
		if (gridWidthBounds > gridWidth) {
			if (resizeDirection == ResizeDirection.HORIZONTAL || resizeDirection == ResizeDirection.HORIZONTAL_AND_VERTICAL) {
				ResizeGrid (gridWidthBounds, gridHeight, gridDepth);
			} else {
				// DEBUG:
				//Debug.Log ("- Querying beyond grid limits (horizontal resize not allowed)");
				return null;
			}
		}
		
		int i = yGrid * gridWidth * gridDepth + zGrid * gridWidth + xGrid2;
		VoxelChunk chunk = grid [i];
		if (chunk == null) {
			byte[] data = new byte[1728];
			Array.Clear (data, 0, 1728);
			float chunkSide = 12 * VoxelEngine.instance.voxelSize;
			float halfChunkSide = chunkSide * 0.5f;
			Vector3 position = gridCenter + new Vector3 (xGrid2 * chunkSide + halfChunkSide, yGrid * chunkSide + halfChunkSide, zGrid * chunkSide + halfChunkSide);
			// DEBUG:
			//Debug.Log ("- Dynamically creating new chunk: (" + xGrid2 + ", " + yGrid + ", " + zGrid + ")");
			chunk = BuildChunk (xGrid2, yGrid, zGrid, position, data);
			grid [i] = chunk;
		}
		return chunk;
	}

	public VoxelChunk GetChunkAtLeft (int xGrid, int yGrid, int zGrid)
	{
		int xGrid2 = xGrid - 1;
		if (xGrid2 < 0) {
			// DEBUG:
			//Debug.Log ("- Querying beyond grid limits");
			return null;
		}
		
		int i = yGrid * gridWidth * gridDepth + zGrid * gridWidth + xGrid2;
		VoxelChunk chunk = grid [i];
		if (chunk == null) {
			byte[] data = new byte[1728];
			Array.Clear (data, 0, 1728);
			float chunkSide = 12 * VoxelEngine.instance.voxelSize;
			float halfChunkSide = chunkSide * 0.5f;
			Vector3 position = gridCenter + new Vector3 (xGrid2 * chunkSide + halfChunkSide, yGrid * chunkSide + halfChunkSide, zGrid * chunkSide + halfChunkSide);
			// DEBUG:
			//Debug.Log ("- Dynamically creating new chunk: (" + xGrid2 + ", " + yGrid + ", " + zGrid + ")");
			chunk = BuildChunk (xGrid2, yGrid, zGrid, position, data);
			grid [i] = chunk;
		}
		return chunk;
	}

	public VoxelChunk GetChunkBehind (int xGrid, int yGrid, int zGrid)
	{
		int zGrid2 = zGrid + 1;
		int gridDepthBound = zGrid2 + 1;
		if (gridDepthBound > gridDepth) {
			if (resizeDirection == ResizeDirection.HORIZONTAL || resizeDirection == ResizeDirection.HORIZONTAL_AND_VERTICAL) {
				ResizeGrid (gridWidth, gridHeight, gridDepthBound);
			} else {
				// DEBUG:
				//Debug.Log ("- Querying beyond grid limits (horizontal resize not allowed)");
				return null;
			}
		}
		
		int i = yGrid * gridWidth * gridDepth + zGrid2 * gridWidth + xGrid;
		VoxelChunk chunk = grid [i];
		if (chunk == null) {
			byte[] data = new byte[1728];
			Array.Clear (data, 0, 1728);
			float chunkSide = 12 * VoxelEngine.instance.voxelSize;
			float halfChunkSide = chunkSide * 0.5f;
			Vector3 position = gridCenter + new Vector3 (xGrid * chunkSide + halfChunkSide, yGrid * chunkSide + halfChunkSide, zGrid2 * chunkSide + halfChunkSide);
			// DEBUG:
			//Debug.Log ("- Dynamically creating new chunk: (" + xGrid + ", " + yGrid + ", " + zGrid2 + ")");
			chunk = BuildChunk (xGrid, yGrid, zGrid2, position, data);
			grid [i] = chunk;
		}
		return chunk;
	}

	public VoxelChunk GetChunkInFront (int xGrid, int yGrid, int zGrid)
	{
		int zGrid2 = zGrid - 1;
		if (zGrid2 < 0) {
			//Debug.Log ("- Querying beyond grid limits");
			return null;
		}
		
		int i = yGrid * gridWidth * gridDepth + zGrid2 * gridWidth + xGrid;
		VoxelChunk chunk = grid [i];
		if (chunk == null) {
			byte[] data = new byte[1728];
			Array.Clear (data, 0, 1728);
			float chunkSide = 12 * VoxelEngine.instance.voxelSize;
			float halfChunkSide = chunkSide * 0.5f;
			Vector3 position = gridCenter + new Vector3 (xGrid * chunkSide + halfChunkSide, yGrid * chunkSide + halfChunkSide, zGrid2 * chunkSide + halfChunkSide);
			// DEBUG:
			//Debug.Log ("- Dynamically creating new chunk: (" + xGrid + ", " + yGrid + ", " + zGrid2 + ")");
			chunk = BuildChunk (xGrid, yGrid, zGrid2, position, data);
			grid [i] = chunk;
		}
		return chunk;
	}

	public VoxelChunk GetChunkAbove (int xGrid, int yGrid, int zGrid)
	{
		int yGrid2 = yGrid + 1;
		int gridHorizontalBound = yGrid2 + 1;
		if (gridHorizontalBound > gridHeight) {
			if (resizeDirection == ResizeDirection.VERTICAL || resizeDirection == ResizeDirection.HORIZONTAL_AND_VERTICAL) {
				ResizeGrid (gridWidth, gridHorizontalBound, gridDepth);
			} else {
				// DEBUG:
				//Debug.Log ("- Querying above grid limits (vertical resize not allowed)");
				return null;
			}
		}
				
		int i = yGrid2 * gridWidth * gridDepth + zGrid * gridWidth + xGrid;
		VoxelChunk chunk = grid [i];
		if (chunk == null) {
			byte[] data = new byte[1728];
			Array.Clear (data, 0, 1728);
			float chunkSide = 12 * VoxelEngine.instance.voxelSize;
			float halfChunkSide = chunkSide * 0.5f;
			Vector3 position = gridCenter + new Vector3 (xGrid * chunkSide + halfChunkSide, yGrid2 * chunkSide + halfChunkSide, zGrid * chunkSide + halfChunkSide);
			// DEBUG:
			//Debug.Log ("- Dynamically creating new chunk: (" + xGrid + ", " + yGrid2 + ", " + zGrid + ")");
			chunk = BuildChunk (xGrid, yGrid2, zGrid, position, data);
			grid [i] = chunk;
		}
		return chunk;
	}

	public VoxelChunk GetChunkBelow (int xGrid, int yGrid, int zGrid)
	{
		int yGrid2 = yGrid - 1;
		if (yGrid2 < 0) {
			// DEBUG:
			//Debug.Log ("- Querying below grid limits");
			return null;
		}
		
		int i = yGrid2 * gridWidth * gridDepth + zGrid * gridWidth + xGrid;
		VoxelChunk chunk = grid [i];
		if (chunk == null) {
			byte[] data = new byte[1728];
			Array.Clear (data, 0, 1728);
			float chunkSide = 12 * VoxelEngine.instance.voxelSize;
			float halfChunkSide = chunkSide * 0.5f;
			Vector3 position = gridCenter + new Vector3 (xGrid * chunkSide + halfChunkSide, yGrid2 * chunkSide + halfChunkSide, zGrid * chunkSide + halfChunkSide);
			// DEBUG:
			//Debug.Log ("- Dynamically creating new chunk: (" + xGrid + ", " + yGrid2 + ", " + zGrid + ")");
			chunk = BuildChunk (xGrid, yGrid2, zGrid, position, data);
			grid [i] = chunk;
		}
		return chunk;
	}
}
