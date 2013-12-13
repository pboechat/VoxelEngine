using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
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
	public bool loadFromFile = false;
	public string filePath;
	public bool executeOnStart = true;
	public bool hideChunks = true;
	public ResizeDirection resizeDirection;
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
		if (executeOnStart) {
			if (loadFromFile) {
				LoadFromFile ();
			} else {
				Generate ();
			}
		}
	}
	
	void FillColumns (Voxel[] data, byte voxelId, int x, int z, int height)
	{
		for (int y = 0; y < 12; y++) {
			data [y * 144 + z * 12 + x].id = (y < height) ? voxelId : (byte)0;
		}
	}
	
	public void ExecuteQuery (VoxelQuery query)
	{
		query.Prepare ();
		
		// cached calculations
		Vector3 localPosition = transform.worldToLocalMatrix.MultiplyPoint3x4 (query.position);
		Vector3 queryCenter = new Vector3 (Mathf.FloorToInt (localPosition.x) + VoxelEngine.instance.halfVoxelSize,
		                                   Mathf.FloorToInt (localPosition.y) + VoxelEngine.instance.halfVoxelSize,
		                                   Mathf.FloorToInt (localPosition.z) + VoxelEngine.instance.halfVoxelSize);
		int gridDepth_x_gridWidth = gridDepth * gridWidth;
		float queryYOffset = (-query.height * 0.5f) * VoxelEngine.instance.voxelSize;
		float queryZOffset = (-query.depth * 0.5f) * VoxelEngine.instance.voxelSize;
		float queryXOffset = (-query.width * 0.5f) * VoxelEngine.instance.voxelSize;
		
		float chunkSide = VoxelEngine.instance.voxelSize * 12;
		Vector3 gridOffset = new Vector3 (width * 0.5f, 0.0f, depth * 0.5f);
		
		// find chunk and voxel starting bounds
		
		int cX1 = -1; // chunk starting x bound
		int cY1 = -1; // chunk starting y bound
		int cZ1 = -1; // chunk starting z bound
		int vX1 = -1; // voxel starting x bound
		int vY1 = -1; // voxel starting y bound
		int vZ1 = -1; // voxel starting z bound
		VoxelChunk chunk;
		int x1 = 0, y1 = 0, z1 = 0;
		Vector3 yStart = new Vector3 (0.0f, queryYOffset, 0.0f);
		for (y1 = 0; y1 < query.height; y1++) {
			Vector3 zStart = new Vector3 (0.0f, 0.0f, queryZOffset);
			for (z1 = 0; z1 < query.depth; z1++) {
				Vector3 xStart = new Vector3 (queryXOffset, 0.0f, 0.0f);
				for (x1 = 0; x1 < query.width; x1++) {
					Vector3 voxelCenter = queryCenter + yStart + zStart + xStart;
					Vector3 gridPosition = gridOffset + voxelCenter;
					
					cX1 = Mathf.FloorToInt (gridPosition.x / chunkSide);
					cY1 = Mathf.FloorToInt (gridPosition.y / chunkSide);
					cZ1 = Mathf.FloorToInt (gridPosition.z / chunkSide);
					
					if (cX1 >= 0 && cX1 < gridWidth && cY1 >= 0 && cY1 < gridHeight && cZ1 >= 0 && cZ1 < gridDepth) {
						// valid grid position found, calculate voxel starting bounds and break
						Vector3 chunkStart = new Vector3 (cX1 * chunkSide, cY1 * chunkSide, cZ1 * chunkSide);
						Vector3 voxelPosition = (gridPosition - chunkStart);
						vX1 = Mathf.FloorToInt (voxelPosition.x);
						vY1 = Mathf.FloorToInt (voxelPosition.y);
						vZ1 = Mathf.FloorToInt (voxelPosition.z);
						goto ChunkAndVoxelBoundsFound;
					}
					xStart += VoxelEngine.instance.right;
				}
				zStart += VoxelEngine.instance.forward;
			}
			yStart += VoxelEngine.instance.up;
		}
		
		ChunkAndVoxelBoundsFound:
		if (cX1 == -1 && cY1 == -1 && cZ1 == -1 && vX1 == -1 && vY1 == -1 && vZ1 == -1) {
			// no valid grid position found for query, exit
			query.Dispose ();
			return;
		}
		
		// FIXME: checkin invariants
		if (!(cX1 > -1 && cY1 > -1 && cZ1 > -1 && vX1 > -1 && vY1 > -1 && vZ1 > -1)) {
			throw new Exception ("!(chunkX > -1 && chunkY > -1 && chunkZ > -1 && voxelX > -1 && voxelY > -1 && voxelZ > -1)");
		}
		
		int cX2, cY2, cZ2; // current chunk x,y,z bounds
		int vX2, vY2, vZ2; // current voxel x,y,z bounds
		int i = y1 * query.depth * query.width + z1 * query.width + x1;
		// set current y bounds to start
		cY2 = cY1;
		vY2 = vY1;
		int cZ2_x_gridWidth;
		for (int y2 = y1; y2 < query.height; y2++) {
			// set current z bounds to start
			cZ2 = cZ1;
			cZ2_x_gridWidth = cZ2 * gridWidth;
			vZ2 = vZ1;
			for (int z2 = z1; z2 < query.depth; z2++) {
				// set current x bounds to start
				cX2 = cX1;
				vX2 = vX1;
				for (int x2 = x1; x2 < query.width; x2++) {
					if (query.mask [i++]) {
						chunk = grid [cY2 * gridDepth_x_gridWidth + cZ2_x_gridWidth + cX2];
						// if chunk is filled and voxel space is not empty, execute query
						if (chunk != null && chunk.QueryVoxel (vX2, vY2, vZ2)) {
							query.Execute (chunk, vX2, vY2, vZ2);
						}
					}
					if (++vX2 >= 12) {
						// leaving chunk bounds, reset current voxel x bound and increment current chunk x bound
						vX2 = 0;
						if (++cX2 >= gridWidth) {
							// leaving grid grounds, break current loop
							break;
						}
					}
				}
				if (++vZ2 >= 12) {
					// leaving chunk bounds, reset current voxel z bound and increment current chunk z bound
					vZ2 = 0;
					if (++cZ2 >= gridDepth) {
						// leaving grid grounds, break current loop
						break;
					} else {
						cZ2_x_gridWidth = cZ2 * gridWidth;
					}
				}
			}
			if (++vY2 >= 12) {
				// leaving chunk bounds, reset current voxel y bound and increment current chunk y bound
				vY2 = 0;
				if (++cY2 >= gridHeight) {
					// leaving grid grounds, break current loop
					break;
				}
			}
		}
		
		query.Dispose ();
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
	
	VoxelChunk BuildChunk (int x, int y, int z, Vector3 position, Voxel[] data)
	{
		GameObject chunkGameObject = new GameObject ("Chunk (" + x + ", " + y + ", " + z + ")");
		chunkGameObject.isStatic = gameObject.isStatic;
		if (hideChunks) {
			chunkGameObject.hideFlags = HideFlags.HideInHierarchy;
		}
		chunkGameObject.transform.parent = transform;
		chunkGameObject.transform.localPosition = position;
		chunkGameObject.transform.localRotation = Quaternion.identity;
		VoxelChunk chunk = chunkGameObject.AddComponent<VoxelChunk> ();
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
	
	public void LoadFromFile ()
	{
		if (!File.Exists (filePath)) {
			throw new Exception ("terrain file doesn't exist: " + filePath);
		}
		
		MemoryStream memoryStream = new MemoryStream (File.ReadAllBytes (filePath));
		BinaryReader reader = new BinaryReader (memoryStream);
		
		width = reader.ReadInt32 ();
		height = reader.ReadInt32 ();
		depth = reader.ReadInt32 ();
		
		if (width < 1 || depth < 1 || height < 1) {
			reader.Close ();
			memoryStream.Close ();
			throw new Exception ("width or depth or height < 1");
		}
		
		gridWidth = (width + 11) / 12;
		gridDepth = (depth + 11) / 12;
		gridHeight = (height + 11) / 12;
		
		gridCenter = new Vector3 (-width * 0.5f, 0.0f, -depth * 0.5f);
		
		float chunkSide = 12 * VoxelEngine.instance.voxelSize;
		float halfChunkSide = chunkSide * 0.5f;
		
		grid = new VoxelChunk[gridHeight * gridDepth * gridWidth];
		
		// TODO: optimize
		int gridDepth_x_gridWidth = gridWidth * gridDepth;
		for (int cZ = 0; cZ < gridDepth; cZ++) {
			int chunkDepth = cZ * 12;
			int cZ1 = cZ * gridWidth;
			for (int cX = 0; cX < gridWidth; cX++) {
				int chunkWidth = cX * 12;
				for (int cY = 0; cY < gridHeight; cY++) {
					Voxel[] data = new Voxel[1728]; // 20736 bytes
					for (int vY = 0, i = 0; vY < 12; vY++) {
						for (int vZ = 0; vZ < 12; vZ++) {
							for (int vX = 0; vX < 12; vX++, i++) {
								Voxel voxel = new Voxel ();
								voxel.id = reader.ReadByte ();
								voxel.attr1 = reader.ReadByte ();
								voxel.attr2 = reader.ReadByte ();
								voxel.attr3 = reader.ReadByte ();
								voxel.attr4 = reader.ReadInt16 ();
								voxel.attr5 = reader.ReadInt16 ();
								voxel.attr6 = reader.ReadInt32 ();
								data [i] = voxel;
							}
						}
					}
					Vector3 chunkPosition = gridCenter + new Vector3 (chunkWidth * VoxelEngine.instance.voxelSize + halfChunkSide, cY * chunkSide + halfChunkSide, chunkDepth * VoxelEngine.instance.voxelSize + halfChunkSide);
					VoxelChunk chunk = BuildChunk (cX, cY, cZ, chunkPosition, data);
					grid [cY * gridDepth_x_gridWidth + cZ1 + cX] = chunk;
				}
			}
		}
		
		reader.Close ();
		memoryStream.Close ();
	}
	
	public void SaveToFile (string filePath)
	{
		FileStream fileStream;
		if (File.Exists (filePath)) {
			fileStream = File.OpenWrite (filePath);
		} else {
			fileStream = File.Create (filePath);
		}
		BinaryWriter writer = new BinaryWriter (fileStream);
		
		writer.Write (width);
		writer.Write (height);
		writer.Write (depth);
		
		byte[] emptyChunk = new byte[20736];
		Array.Clear (emptyChunk, 0, 20736);
		int gridDepth_x_gridWidth = gridWidth * gridDepth;
		for (int cZ = 0; cZ < gridDepth; cZ++) {
			int cZ1 = cZ * gridWidth;
			for (int cX = 0; cX < gridWidth; cX++) {
				for (int cY = 0; cY < gridHeight; cY++) {
					VoxelChunk chunk = grid [cY * gridDepth_x_gridWidth + cZ1 + cX];
					if (chunk == null) {
						writer.Write (emptyChunk);
						continue;
					}
					for (int vY = 0, i = 0; vY < 12; vY++) {
						for (int vZ = 0; vZ < 12; vZ++) {
							for (int vX = 0; vX < 12; vX++, i++) {
								Voxel voxel = chunk.data [i];
								writer.Write (voxel.id);
								writer.Write (voxel.attr1);
								writer.Write (voxel.attr2);
								writer.Write (voxel.attr3);
								writer.Write (voxel.attr4);
								writer.Write (voxel.attr5);
								writer.Write (voxel.attr6);
							}
						}
					}
				}
			}
		}
		
		writer.Close ();
		fileStream.Close ();
		
		// DEBUG:
		// Debug.Log ("- File saved successfully: " + filePath);
	}
	
	public void Generate ()
	{
		if (width < 1 || depth < 1 || height < 1 || maximumHeight < 1) {
			throw new Exception ("width or depth or height or maximumHeight < 1");
		}
		
		Clear ();
		
		gridWidth = (width + 11) / 12;
		gridDepth = (depth + 11) / 12;
		gridHeight = (height + 11) / 12;
		
		int realWidth = gridWidth * 12;
		int realDepth = gridDepth * 12;
		
		float naturalScale = 1.0f / Mathf.Sqrt ((float)width * depth);
		
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
		
		NoiseMap noiseMap = heightMapBuilder.Map;
		float halfAmplitude = (maximumHeight - minimumHeight) * 0.5f;
		
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
					Voxel[] data = new Voxel[1728]; // 20736 bytes
					bool filled = false;
					for (int z = 0; z < 12; z++) {
						int currentZ = chunkDepth + z;
						for (int x = 0; x < 12; x++) {
							int currentX = chunkWidth + x;
							
							if (currentZ >= depth || currentX >= width) {
								FillColumns (data, (byte)1, x, z, 0);
								continue;
							}
							
							int terrainHeight = Mathf.RoundToInt (noiseMap.GetValue (currentX, currentZ) * halfAmplitude + halfAmplitude + minimumHeight);
							int fillHeight = Mathf.Max (0, terrainHeight - chunkHeight);
							if (fillHeight > 0 && !filled) {
								filled = true;
							}
							FillColumns (data, (byte)1, x, z, fillHeight);
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
		// Debug.Log ("- Resizing grid: from (" + gridWidth + ", " + gridHeight + ", " + gridDepth + ") to (" + newGridWidth + ", " + newGridHeight + ", " + newGridDepth + ")");
		
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
				// Debug.Log ("- Querying beyond grid limits (horizontal resize not allowed)");
				return null;
			}
		}
		
		int i = yGrid * gridWidth * gridDepth + zGrid * gridWidth + xGrid2;
		VoxelChunk chunk = grid [i];
		if (chunk == null) {
			Voxel[] data = new Voxel[1728]; // 20736 bytes
			Array.Clear (data, 0, 1728);
			float chunkSide = 12 * VoxelEngine.instance.voxelSize;
			float halfChunkSide = chunkSide * 0.5f;
			Vector3 position = gridCenter + new Vector3 (xGrid2 * chunkSide + halfChunkSide, yGrid * chunkSide + halfChunkSide, zGrid * chunkSide + halfChunkSide);
			// DEBUG:
			// Debug.Log ("- Dynamically creating new chunk: (" + xGrid2 + ", " + yGrid + ", " + zGrid + ")");
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
			// Debug.Log ("- Querying beyond grid limits");
			return null;
		}
		
		int i = yGrid * gridWidth * gridDepth + zGrid * gridWidth + xGrid2;
		VoxelChunk chunk = grid [i];
		if (chunk == null) {
			Voxel[] data = new Voxel[1728];
			Array.Clear (data, 0, 1728);
			float chunkSide = 12 * VoxelEngine.instance.voxelSize;
			float halfChunkSide = chunkSide * 0.5f;
			Vector3 position = gridCenter + new Vector3 (xGrid2 * chunkSide + halfChunkSide, yGrid * chunkSide + halfChunkSide, zGrid * chunkSide + halfChunkSide);
			// DEBUG:
			// Debug.Log ("- Dynamically creating new chunk: (" + xGrid2 + ", " + yGrid + ", " + zGrid + ")");
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
				// Debug.Log ("- Querying beyond grid limits (horizontal resize not allowed)");
				return null;
			}
		}
		
		int i = yGrid * gridWidth * gridDepth + zGrid2 * gridWidth + xGrid;
		VoxelChunk chunk = grid [i];
		if (chunk == null) {
			Voxel[] data = new Voxel[1728];
			Array.Clear (data, 0, 1728);
			float chunkSide = 12 * VoxelEngine.instance.voxelSize;
			float halfChunkSide = chunkSide * 0.5f;
			Vector3 position = gridCenter + new Vector3 (xGrid * chunkSide + halfChunkSide, yGrid * chunkSide + halfChunkSide, zGrid2 * chunkSide + halfChunkSide);
			// DEBUG:
			// Debug.Log ("- Dynamically creating new chunk: (" + xGrid + ", " + yGrid + ", " + zGrid2 + ")");
			chunk = BuildChunk (xGrid, yGrid, zGrid2, position, data);
			grid [i] = chunk;
		}
		return chunk;
	}
	
	public VoxelChunk GetChunkInFront (int xGrid, int yGrid, int zGrid)
	{
		int zGrid2 = zGrid - 1;
		if (zGrid2 < 0) {
			// DEBUG:
			// Debug.Log ("- Querying beyond grid limits");
			return null;
		}
		
		int i = yGrid * gridWidth * gridDepth + zGrid2 * gridWidth + xGrid;
		VoxelChunk chunk = grid [i];
		if (chunk == null) {
			Voxel[] data = new Voxel[1728];
			Array.Clear (data, 0, 1728);
			float chunkSide = 12 * VoxelEngine.instance.voxelSize;
			float halfChunkSide = chunkSide * 0.5f;
			Vector3 position = gridCenter + new Vector3 (xGrid * chunkSide + halfChunkSide, yGrid * chunkSide + halfChunkSide, zGrid2 * chunkSide + halfChunkSide);
			// DEBUG:
			// Debug.Log ("- Dynamically creating new chunk: (" + xGrid + ", " + yGrid + ", " + zGrid2 + ")");
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
				// Debug.Log ("- Querying above grid limits (vertical resize not allowed)");
				return null;
			}
		}
		
		int i = yGrid2 * gridWidth * gridDepth + zGrid * gridWidth + xGrid;
		VoxelChunk chunk = grid [i];
		if (chunk == null) {
			Voxel[] data = new Voxel[1728];
			Array.Clear (data, 0, 1728);
			float chunkSide = 12 * VoxelEngine.instance.voxelSize;
			float halfChunkSide = chunkSide * 0.5f;
			Vector3 position = gridCenter + new Vector3 (xGrid * chunkSide + halfChunkSide, yGrid2 * chunkSide + halfChunkSide, zGrid * chunkSide + halfChunkSide);
			// DEBUG:
			// Debug.Log ("- Dynamically creating new chunk: (" + xGrid + ", " + yGrid2 + ", " + zGrid + ")");
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
			// Debug.Log ("- Querying below grid limits");
			return null;
		}
		
		int i = yGrid2 * gridWidth * gridDepth + zGrid * gridWidth + xGrid;
		VoxelChunk chunk = grid [i];
		if (chunk == null) {
			Voxel[] data = new Voxel[1728];
			Array.Clear (data, 0, 1728);
			float chunkSide = 12 * VoxelEngine.instance.voxelSize;
			float halfChunkSide = chunkSide * 0.5f;
			Vector3 position = gridCenter + new Vector3 (xGrid * chunkSide + halfChunkSide, yGrid2 * chunkSide + halfChunkSide, zGrid * chunkSide + halfChunkSide);
			// DEBUG:
			// Debug.Log ("- Dynamically creating new chunk: (" + xGrid + ", " + yGrid2 + ", " + zGrid + ")");
			chunk = BuildChunk (xGrid, yGrid2, zGrid, position, data);
			grid [i] = chunk;
		}
		return chunk;
	}
}