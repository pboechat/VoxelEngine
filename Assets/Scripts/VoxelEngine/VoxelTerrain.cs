using UnityEngine;
using System;
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
				
		sectorOffset = new Vector3 (-gridWidth / 2 * 12 * VoxelEngine.instance.voxelSize, 0.0f, -gridDepth / 2 * 12 * VoxelEngine.instance.voxelSize);
				
		grid = new VoxelChunk[gridHeight * gridDepth * gridWidth];
		for (int zGrid = 0; zGrid < gridDepth; zGrid++) {
			int zOffset = zGrid * 12;
			for (int xGrid = 0; xGrid < gridWidth; xGrid++) {
				int xOffset = xGrid * 12;
				for (int yGrid = 0, minSectorHeight = 0; yGrid < gridHeight; yGrid++, minSectorHeight += 12) {
					byte[] data = new byte[1728];
					bool filled = false;
					for (int z = 0; z < 12; z++) {
						for (int x = 0; x < 12; x++) {
							int terrainHeight = GetHeight (xOffset + x, zOffset + z);
							int usedHeight = Mathf.Max (0, terrainHeight - minSectorHeight);
							if (usedHeight > 0 && !filled) {
								filled = true;
							}
							FillColumns (data, x, z, usedHeight);
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
		// DEBUG:
		Debug.Log ("- Resizing grid: from (" + gridWidth + ", " + gridHeight + ", " + gridDepth + ") to (" + newGridWidth + ", " + newGridHeight + ", " + newGridDepth + ")");

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
				Debug.Log ("- Querying beyond grid limits (horizontal resize not allowed)");
				return null;
			}
		}
		
		int i = yGrid * gridWidth * gridDepth + zGrid * gridWidth + xGrid2;
		VoxelChunk chunk = grid [i];
		if (chunk == null) {
			byte[] data = new byte[1728];
			Array.Clear (data, 0, 1728);
			Vector3 position = sectorOffset + new Vector3 (xGrid2 * 12 * VoxelEngine.instance.voxelSize, yGrid * 12 * VoxelEngine.instance.voxelSize, zGrid * 12 * VoxelEngine.instance.voxelSize);
			// DEBUG:
			Debug.Log ("- Dynamically creating new chunk: (" + xGrid2 + ", " + yGrid + ", " + zGrid + ")");
			chunk = CreateChunk (xGrid2, yGrid, zGrid, position, data);
			grid [i] = chunk;
		}
		return chunk;
	}

	public VoxelChunk GetChunkAtLeft (int xGrid, int yGrid, int zGrid)
	{
		int xGrid2 = xGrid - 1;
		if (xGrid2 < 0) {
			// DEBUG:
			Debug.Log ("- Querying beyond grid limits");
			return null;
		}
		
		int i = yGrid * gridWidth * gridDepth + zGrid * gridWidth + xGrid2;
		VoxelChunk chunk = grid [i];
		if (chunk == null) {
			byte[] data = new byte[1728];
			Array.Clear (data, 0, 1728);
			Vector3 position = sectorOffset + new Vector3 (xGrid2 * 12 * VoxelEngine.instance.voxelSize, yGrid * 12 * VoxelEngine.instance.voxelSize, zGrid * 12 * VoxelEngine.instance.voxelSize);
			// DEBUG:
			Debug.Log ("- Dynamically creating new chunk: (" + xGrid2 + ", " + yGrid + ", " + zGrid + ")");
			chunk = CreateChunk (xGrid2, yGrid, zGrid, position, data);
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
				Debug.Log ("- Querying beyond grid limits (horizontal resize not allowed)");
				return null;
			}
		}
		
		int i = yGrid * gridWidth * gridDepth + zGrid2 * gridWidth + xGrid;
		VoxelChunk chunk = grid [i];
		if (chunk == null) {
			byte[] data = new byte[1728];
			Array.Clear (data, 0, 1728);
			Vector3 position = sectorOffset + new Vector3 (xGrid * 12 * VoxelEngine.instance.voxelSize, yGrid * 12 * VoxelEngine.instance.voxelSize, zGrid2 * 12 * VoxelEngine.instance.voxelSize);
			// DEBUG:
			Debug.Log ("- Dynamically creating new chunk: (" + xGrid + ", " + yGrid + ", " + zGrid2 + ")");
			chunk = CreateChunk (xGrid, yGrid, zGrid2, position, data);
			grid [i] = chunk;
		}
		return chunk;
	}

	public VoxelChunk GetChunkInFront (int xGrid, int yGrid, int zGrid)
	{
		int zGrid2 = zGrid - 1;
		if (zGrid2 < 0) {
			Debug.Log ("- Querying beyond grid limits");
			return null;
		}
		
		int i = yGrid * gridWidth * gridDepth + zGrid2 * gridWidth + xGrid;
		VoxelChunk chunk = grid [i];
		if (chunk == null) {
			byte[] data = new byte[1728];
			Array.Clear (data, 0, 1728);
			Vector3 position = sectorOffset + new Vector3 (xGrid * 12 * VoxelEngine.instance.voxelSize, yGrid * 12 * VoxelEngine.instance.voxelSize, zGrid2 * 12 * VoxelEngine.instance.voxelSize);
			// DEBUG:
			Debug.Log ("- Dynamically creating new chunk: (" + xGrid + ", " + yGrid + ", " + zGrid2 + ")");
			chunk = CreateChunk (xGrid, yGrid, zGrid2, position, data);
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
				Debug.Log ("- Querying above grid limits (vertical resize not allowed)");
				return null;
			}
		}
				
		int i = yGrid2 * gridWidth * gridDepth + zGrid * gridWidth + xGrid;
		VoxelChunk chunk = grid [i];
		if (chunk == null) {
			byte[] data = new byte[1728];
			Array.Clear (data, 0, 1728);
			Vector3 position = sectorOffset + new Vector3 (xGrid * 12 * VoxelEngine.instance.voxelSize, yGrid2 * 12 * VoxelEngine.instance.voxelSize, zGrid * 12 * VoxelEngine.instance.voxelSize);
			// DEBUG:
			Debug.Log ("- Dynamically creating new chunk: (" + xGrid + ", " + yGrid2 + ", " + zGrid + ")");
			chunk = CreateChunk (xGrid, yGrid2, zGrid, position, data);
			grid [i] = chunk;
		}
		return chunk;
	}

	public VoxelChunk GetChunkBelow (int xGrid, int yGrid, int zGrid)
	{
		int yGrid2 = yGrid - 1;
		if (yGrid2 < 0) {
			// DEBUG:
			Debug.Log ("- Querying below grid limits");
			return null;
		}
		
		int i = yGrid2 * gridWidth * gridDepth + zGrid * gridWidth + xGrid;
		VoxelChunk chunk = grid [i];
		if (chunk == null) {
			byte[] data = new byte[1728];
			Array.Clear (data, 0, 1728);
			Vector3 position = sectorOffset + new Vector3 (xGrid * 12 * VoxelEngine.instance.voxelSize, yGrid2 * 12 * VoxelEngine.instance.voxelSize, zGrid * 12 * VoxelEngine.instance.voxelSize);
			// DEBUG:
			Debug.Log ("- Dynamically creating new chunk: (" + xGrid + ", " + yGrid2 + ", " + zGrid + ")");
			chunk = CreateChunk (xGrid, yGrid2, zGrid, position, data);
			grid [i] = chunk;
		}
		return chunk;
	}
}
