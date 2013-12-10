using UnityEngine;
using System;

public class VoxelChunk : MonoBehaviour
{
	private const float EPSILON = 0.00001f;
	private static readonly Rect[] EMPTY_UV_RECTS = new Rect[] { new Rect (0.0f, 0.0f, 0.0f, 0.0f), 
																	 new Rect (0.0f, 0.0f, 0.0f, 0.0f), 
																	 new Rect (0.0f, 0.0f, 0.0f, 0.0f),
																	 new Rect (0.0f, 0.0f, 0.0f, 0.0f),
																	 new Rect (0.0f, 0.0f, 0.0f, 0.0f),
																	 new Rect (0.0f, 0.0f, 0.0f, 0.0f) };
	private const int EXCLUDE_ALL_FACES = (int)Direction.TOP | (int)Direction.BOTTOM | (int)Direction.LEFT | (int)Direction.RIGHT | (int)Direction.FRONT | (int)Direction.BACK;
	public int x;
	public int y;
	public int z;
	public VoxelTerrain terrain;
	public int width;
	public int height;
	public int depth;
	public byte[] data;
	[SerializeField]
	private MeshFilter
		meshFilter;
	[SerializeField]
	private MeshCollider
		meshCollider;
	[SerializeField]
	private Mesh
		chunkMesh;
	[SerializeField]
	private Vector3
		offset;
	
	bool HasTop (int x, int y, int z1)
	{
		if (y == height - 1) {
			return false;
		}
		
		return data [(y + 1) * (depth * width) + z1 + x] > 0;
	}

	int GetTopVoxelIndex (int x, int y, int z1)
	{
		if (y == height - 1) {
			return -1;
		}
		
		return (y + 1) * (depth * width) + z1 + x;
	}
	
	bool HasBottom (int x, int y, int z1)
	{
		if (y == 0) {
			return false;
		}
		
		return data [(y - 1) * (depth * width) + z1 + x] > 0;
	}

	int GetBottomVoxelIndex (int x, int y, int z1)
	{
		if (y == 0) {
			return -1;
		}
		
		return (y - 1) * (depth * width) + z1 + x;
	}
	
	bool HasLeft (int x, int y1, int z1)
	{
		if (x == 0) {
			return false;
		}
		
		return data [y1 + z1 + x - 1] > 0;
	}

	int GetLeftVoxelIndex (int x, int y1, int z1)
	{
		if (x == 0) {
			return -1;
		}
		
		return y1 + z1 + x - 1;
	}
	
	bool HasRight (int x, int y1, int z1)
	{
		if (x == width - 1) {
			return false;
		}
		
		return data [y1 + z1 + x + 1] > 0;
	}

	int GetRightVoxelIndex (int x, int y1, int z1)
	{
		if (x == width - 1) {
			return -1;
		}
		
		return y1 + z1 + x + 1;
	}
	
	bool HasFront (int x, int y1, int z)
	{
		if (z == 0) {
			return false;
		}
		
		return data [y1 + (z - 1) * width + x] > 0;
	}

	int GetFrontVoxelIndex (int x, int y1, int z)
	{
		if (z == 0) {
			return -1;
		}
		
		return y1 + (z - 1) * width + x;
	}
	
	bool HasBack (int x, int y1, int z)
	{
		if (z == depth - 1) {
			return false;
		}
		
		return data [y1 + (z + 1) * width + x] > 0;
	}

	int GetBackVoxelIndex (int x, int y1, int z)
	{
		if (z == depth - 1) {
			return -1;
		}
		
		return y1 + (z + 1) * width + x;
	}

	void FindVoxel (Vector3 point, out int x, out int y, out int z)
	{
		Vector3 localPoint = transform.worldToLocalMatrix.MultiplyPoint3x4 (point);
		Vector3 voxelCoordinates = offset + (localPoint / VoxelEngine.instance.voxelSize);
		
		x = Mathf.FloorToInt (voxelCoordinates.x);
		y = Mathf.FloorToInt (voxelCoordinates.y);
		int cZ = Mathf.CeilToInt (voxelCoordinates.z);
		z = depth - cZ;

		// DEBUG:
		Debug.Log ("********** Finding voxel **********");
		Debug.Log ("1. Snapped voxel coordinates: (" + x + ", " + y + ", " + z + ")");

		int voxelIndex = y * (depth * width) + z * (width) + x;
		
		if (data [voxelIndex] != 0) {
			Debug.Log ("2. Voxel found, returning");
			return;
		}
		
		float xReminder = (voxelCoordinates.x - x);
		float yReminder = (voxelCoordinates.y - y);
		float zReminder = (voxelCoordinates.z - cZ);

		// DEBUG:
		Debug.Log ("3. Calculating coords. remainders (x=" + xReminder + ", y=" + yReminder + ", z=" + zReminder + ")");
		
		if (xReminder < EPSILON && xReminder > -EPSILON) {
			// DEBUG:
			Debug.Log ("4. Moving voxel coordenate left");
			x--;
		} else if (yReminder < EPSILON && yReminder > -EPSILON) {
			// DEBUG:
			Debug.Log ("4. Moving voxel coordenate down");
			y--;
		} else if (zReminder < EPSILON && zReminder > -EPSILON) {
			// DEBUG:
			Debug.Log ("4. Moving voxel coordenate forward");
			z--;
		}

		// DEBUG:
		Debug.Log ("5. New voxel coordinates: (" + x + ", " + y + ", " + z + ")");
	}
		
	void FindAdjacentEmptyVoxel (Vector3 point, out int x, out int y, out int z)
	{
		Vector3 localPoint = transform.worldToLocalMatrix.MultiplyPoint3x4 (point);
		Vector3 voxelCoordinates = offset + (localPoint / VoxelEngine.instance.voxelSize);

		x = Mathf.FloorToInt (voxelCoordinates.x);
		y = Mathf.FloorToInt (voxelCoordinates.y);
		int cZ = Mathf.CeilToInt (voxelCoordinates.z);
		z = depth - cZ;

		// DEBUG:
		Debug.Log ("********** Finding adjacent voxel **********");
		Debug.Log ("1. Snapped voxel coordinates: (" + x + ", " + y + ", " + z + ")");

		if (x == width || y == height || z == depth) {
			// DEBUG:
			Debug.Log ("2. Picking voxel at chunk's border");
			return;
		}

		// DEBUG:
		Debug.Log ("2. Picking internal chunk voxel");

		int voxelIndex = y * (depth * width) + z * (width) + x;

		if (data [voxelIndex] == 0) {
			Debug.Log ("3. Adjacent empty voxel found, returning");
			return;
		}

		// DEBUG:
		Debug.Log ("3. Occupied voxel found, supposed error in snapping");

		float xReminder = (voxelCoordinates.x - x);
		float yReminder = (voxelCoordinates.y - y);
		float zReminder = (voxelCoordinates.z - cZ);

		// DEBUG:
		Debug.Log ("4. Calculating coords. remainders (x=" + xReminder + ", y=" + yReminder + ", z=" + zReminder + ")");

		if (xReminder < EPSILON && xReminder > -EPSILON) {
			// DEBUG:
			Debug.Log ("5. Moving voxel coordenate left");
			x--;
		} else if (yReminder < EPSILON && yReminder > -EPSILON) {
			// DEBUG:
			Debug.Log ("5. Moving voxel coordenate down");
			y--;
		} else if (zReminder < EPSILON && zReminder > -EPSILON) {
			// DEBUG:
			Debug.Log ("5. Moving voxel coordenate forward");
			z--;
		}

		// DEBUG:
		Debug.Log ("6. New voxel coordinates: (" + x + ", " + y + ", " + z + ")");
	}
		
	public void AddVoxel (byte voxelId, Vector3 point)
	{
		int x, y, z;
		FindAdjacentEmptyVoxel (point, out x, out y, out z);
		AddVoxel (voxelId, x, y, z);
	}

	void AddVoxel (byte voxelId, int x, int y, int z)
	{
		// DEBUG:
		Debug.Log ("- Trying to add voxel [chunk=(" + this.x + ", " + this.y + ", " + this.z + "), voxel=(" + x + ", " + y + ", " + z + ")]");
		
		if (x < 0) {
			if (terrain == null) {
				// DEBUG:
				Debug.Log ("- Can't add voxel");
			} else {
				VoxelChunk chunk = terrain.GetChunkAtLeft (this.x, this.y, this.z);
				if (chunk != null) {
					// DEBUG:
					Debug.Log ("- Cross-boundaries voxel addition: passing command to chunk at left [from (" + this.x + ", " + this.y + ", " + this.z + ") to (" + chunk.x + ", " + chunk.y + ", " + chunk.z + ")]");
					chunk.AddVoxel (voxelId, chunk.width - 1, y, z);
				} else {
					// DEBUG:
					Debug.Log ("- Can't add voxel");
				}
			}
			
			return;
		} else if (x >= width) {
			if (terrain == null) {
				// DEBUG:
				Debug.Log ("- Can't add voxel");
			} else {
				VoxelChunk chunk = terrain.GetChunkAtRight (this.x, this.y, this.z);
				if (chunk != null) {
					// DEBUG:
					Debug.Log ("- Cross-boundaries voxel addition: passing command to chunk at right [from (" + this.x + ", " + this.y + ", " + this.z + ") to (" + chunk.x + ", " + chunk.y + ", " + chunk.z + ")]");
					chunk.AddVoxel (voxelId, 0, y, z);
				} else {
					// DEBUG:
					Debug.Log ("- Can't add voxel");
				}
			}
			
			return;
		}
		
		if (z < 0) {
			if (terrain == null) {
				// DEBUG:
				Debug.Log ("- Can't add voxel");
			} else {
				VoxelChunk chunk = terrain.GetChunkBehind (this.x, this.y, this.z);
				if (chunk != null) {
					// DEBUG:
					Debug.Log ("- Cross-boundaries voxel addition: passing command to chunk behind [from (" + this.x + ", " + this.y + ", " + this.z + ") to (" + chunk.x + ", " + chunk.y + ", " + chunk.z + ")]");
					chunk.AddVoxel (voxelId, x, y, chunk.depth - 1);
				} else {
					// DEBUG:
					Debug.Log ("- Can't add voxel");
				}
			}
			
			return;
		} else if (z >= depth) {
			if (terrain == null) {
				// DEBUG:
				Debug.Log ("- Can't add voxel");
			} else {
				VoxelChunk chunk = terrain.GetChunkInFront (this.x, this.y, this.z);
				if (chunk != null) {
					// DEBUG:
					Debug.Log ("- Cross-boundaries voxel addition: passing command to chunk in front [from (" + this.x + ", " + this.y + ", " + this.z + ") to (" + chunk.x + ", " + chunk.y + ", " + chunk.z + ")]");
					chunk.AddVoxel (voxelId, x, y, 0);
				} else {
					// DEBUG:
					Debug.Log ("- Can't add voxel");
				}
			}
			
			return;
		}
		
		if (y < 0) {
			if (terrain == null) {
				// DEBUG:
				Debug.Log ("- Can't add voxel");
			} else {
				VoxelChunk chunk = terrain.GetChunkBelow (this.x, this.y, this.z);
				if (chunk != null) {
					// DEBUG:
					Debug.Log ("- Cross-boundaries voxel addition: passing command to chunk below [from (" + this.x + ", " + this.y + ", " + this.z + ") to (" + chunk.x + ", " + chunk.y + ", " + chunk.z + ")]");
					chunk.AddVoxel (voxelId, x, chunk.height - 1, z);
				} else {
					// DEBUG:
					Debug.Log ("- Can't add voxel");
				}
			}
			
			return;
		} else if (y >= height) {
			if (terrain == null) {
				// DEBUG:
				Debug.Log ("- Can't add voxel");
			} else {
				VoxelChunk chunk = terrain.GetChunkAbove (this.x, this.y, this.z);
				if (chunk != null) {
					// DEBUG:
					Debug.Log ("- Cross-boundaries voxel addition: passing command to chunk above [from (" + this.x + ", " + this.y + ", " + this.z + ") to (" + chunk.x + ", " + chunk.y + ", " + chunk.z + ")]");
					chunk.AddVoxel (voxelId, x, 0, z);
				} else {
					// DEBUG:
					Debug.Log ("- Can't add voxel");
				}
			}
			
			return;
		}
		
		int y1 = y * (width * depth);
		int z1 = z * width;
		int voxelIndex = y1 + z1 + x;

		// FIXME: checking invariants
		if (data [voxelIndex] != 0) {
			throw new Exception ("data [voxelIndex] != 0 (" + data [voxelIndex] + ")");
		}
		
		data [voxelIndex] = voxelId;
		
		int baseVertex = voxelIndex * 24;
		int baseIndex = voxelIndex * 36;
		
		int[] indices = chunkMesh.triangles;
		Vector2[] uvs = chunkMesh.uv;
		
		bool hasTop;
		
		int neighborVoxelIndex;
		int neighborBaseVertex;
		int neighborBaseIndex;
		
		if ((neighborVoxelIndex = GetBackVoxelIndex (x, y1, z)) != -1 && data [neighborVoxelIndex] != 0) {
			// front
			neighborBaseVertex = neighborVoxelIndex * 24;
			neighborBaseIndex = neighborVoxelIndex * 36;
			indices [neighborBaseIndex + 18] = neighborBaseVertex + 12;
			indices [neighborBaseIndex + 19] = neighborBaseVertex + 13;
			indices [neighborBaseIndex + 20] = neighborBaseVertex + 14;
			indices [neighborBaseIndex + 21] = neighborBaseVertex + 12;
			indices [neighborBaseIndex + 22] = neighborBaseVertex + 14;
			indices [neighborBaseIndex + 23] = neighborBaseVertex + 15;			
		} else {
			// back
			indices [baseIndex] = baseVertex;
			indices [baseIndex + 1] = baseVertex + 1;
			indices [baseIndex + 2] = baseVertex + 2;
			indices [baseIndex + 3] = baseVertex;
			indices [baseIndex + 4] = baseVertex + 2;
			indices [baseIndex + 5] = baseVertex + 3;
		}
		
		if ((neighborVoxelIndex = GetTopVoxelIndex (x, y, z1)) != -1 && data [neighborVoxelIndex] != 0) {
			hasTop = true;
			// bottom
			neighborBaseVertex = neighborVoxelIndex * 24;
			neighborBaseIndex = neighborVoxelIndex * 36;
			indices [neighborBaseIndex + 24] = neighborBaseVertex + 16;
			indices [neighborBaseIndex + 25] = neighborBaseVertex + 18;
			indices [neighborBaseIndex + 26] = neighborBaseVertex + 17;
			indices [neighborBaseIndex + 27] = neighborBaseVertex + 16;
			indices [neighborBaseIndex + 28] = neighborBaseVertex + 19;
			indices [neighborBaseIndex + 29] = neighborBaseVertex + 18;			
		} else {
			// top
			hasTop = false;
			indices [baseIndex + 6] = baseVertex + 4;
			indices [baseIndex + 7] = baseVertex + 5;
			indices [baseIndex + 8] = baseVertex + 6;
			indices [baseIndex + 9] = baseVertex + 4;
			indices [baseIndex + 10] = baseVertex + 6;
			indices [baseIndex + 11] = baseVertex + 7;
		}
		
		if ((neighborVoxelIndex = GetRightVoxelIndex (x, y1, z1)) != -1 && data [neighborVoxelIndex] != 0) {
			// left
			neighborBaseVertex = neighborVoxelIndex * 24;
			neighborBaseIndex = neighborVoxelIndex * 36;
			indices [neighborBaseIndex + 30] = neighborBaseVertex + 20;
			indices [neighborBaseIndex + 31] = neighborBaseVertex + 22;
			indices [neighborBaseIndex + 32] = neighborBaseVertex + 21;
			indices [neighborBaseIndex + 33] = neighborBaseVertex + 20;
			indices [neighborBaseIndex + 34] = neighborBaseVertex + 23;
			indices [neighborBaseIndex + 35] = neighborBaseVertex + 22;
		} else {
			// right
			indices [baseIndex + 12] = baseVertex + 8;
			indices [baseIndex + 13] = baseVertex + 9;
			indices [baseIndex + 14] = baseVertex + 10;
			indices [baseIndex + 15] = baseVertex + 8;
			indices [baseIndex + 16] = baseVertex + 10;
			indices [baseIndex + 17] = baseVertex + 11;
		}
		
		if ((neighborVoxelIndex = GetFrontVoxelIndex (x, y1, z)) != -1 && data [neighborVoxelIndex] != 0) {
			// back
			neighborBaseVertex = neighborVoxelIndex * 24;
			neighborBaseIndex = neighborVoxelIndex * 36;
			indices [neighborBaseIndex] = neighborBaseVertex;
			indices [neighborBaseIndex + 1] = neighborBaseVertex + 1;
			indices [neighborBaseIndex + 2] = neighborBaseVertex + 2;
			indices [neighborBaseIndex + 3] = neighborBaseVertex;
			indices [neighborBaseIndex + 4] = neighborBaseVertex + 2;
			indices [neighborBaseIndex + 5] = neighborBaseVertex + 3;
		} else {
			// front
			indices [baseIndex + 18] = baseVertex + 12;
			indices [baseIndex + 19] = baseVertex + 13;
			indices [baseIndex + 20] = baseVertex + 14;
			indices [baseIndex + 21] = baseVertex + 12;
			indices [baseIndex + 22] = baseVertex + 14;
			indices [baseIndex + 23] = baseVertex + 15;
		}

		if ((neighborVoxelIndex = GetBottomVoxelIndex (x, y, z1)) != -1 && data [neighborVoxelIndex] != 0) {
			// top
			neighborBaseVertex = neighborVoxelIndex * 24;
			neighborBaseIndex = neighborVoxelIndex * 36;
			indices [neighborBaseIndex + 12] = neighborBaseVertex + 8;
			indices [neighborBaseIndex + 13] = neighborBaseVertex + 9;
			indices [neighborBaseIndex + 14] = neighborBaseVertex + 10;
			indices [neighborBaseIndex + 15] = neighborBaseVertex + 8;
			indices [neighborBaseIndex + 16] = neighborBaseVertex + 10;
			indices [neighborBaseIndex + 17] = neighborBaseVertex + 11;
		} else {
			// bottom
			indices [baseIndex + 24] = baseVertex + 16;
			indices [baseIndex + 25] = baseVertex + 18;
			indices [baseIndex + 26] = baseVertex + 17;
			indices [baseIndex + 27] = baseVertex + 16;
			indices [baseIndex + 28] = baseVertex + 19;
			indices [baseIndex + 29] = baseVertex + 18;
		}
		
		if ((neighborVoxelIndex = GetLeftVoxelIndex (x, y1, z1)) != -1 && data [neighborVoxelIndex] != 0) {
			// right
			neighborBaseVertex = neighborVoxelIndex * 24;
			neighborBaseIndex = neighborVoxelIndex * 36;
			indices [neighborBaseIndex + 12] = neighborBaseVertex + 8;
			indices [neighborBaseIndex + 13] = neighborBaseVertex + 9;
			indices [neighborBaseIndex + 14] = neighborBaseVertex + 10;
			indices [neighborBaseIndex + 15] = neighborBaseVertex + 8;
			indices [neighborBaseIndex + 16] = neighborBaseVertex + 10;
			indices [neighborBaseIndex + 17] = neighborBaseVertex + 11;
		} else {
			// left
			indices [baseIndex + 30] = baseVertex + 20;
			indices [baseIndex + 31] = baseVertex + 22;
			indices [baseIndex + 32] = baseVertex + 21;
			indices [baseIndex + 33] = baseVertex + 20;
			indices [baseIndex + 34] = baseVertex + 23;
			indices [baseIndex + 35] = baseVertex + 22;
		}

		Rect uvRect = VoxelEngine.instance.GetTileUv ((byte)((hasTop) ? 3 : 4));
		uvs [baseVertex] = new Vector2 (uvRect.xMin, uvRect.yMax);
		uvs [baseVertex + 1] = new Vector2 (uvRect.xMax, uvRect.yMax);
		uvs [baseVertex + 2] = new Vector2 (uvRect.xMax, uvRect.yMin);
		uvs [baseVertex + 3] = new Vector2 (uvRect.xMin, uvRect.yMin);
		
		uvRect = VoxelEngine.instance.GetTileUv ((byte)1);
		uvs [baseVertex + 4] = new Vector2 (uvRect.xMin, uvRect.yMax);
		uvs [baseVertex + 5] = new Vector2 (uvRect.xMax, uvRect.yMax);
		uvs [baseVertex + 6] = new Vector2 (uvRect.xMax, uvRect.yMin);
		uvs [baseVertex + 7] = new Vector2 (uvRect.xMin, uvRect.yMin);
		
		uvRect = VoxelEngine.instance.GetTileUv ((byte)((hasTop) ? 3 : 4));
		uvs [baseVertex + 8] = new Vector2 (uvRect.xMin, uvRect.yMax);
		uvs [baseVertex + 9] = new Vector2 (uvRect.xMax, uvRect.yMax);
		uvs [baseVertex + 10] = new Vector2 (uvRect.xMax, uvRect.yMin); 
		uvs [baseVertex + 11] = new Vector2 (uvRect.xMin, uvRect.yMin);		
		
		uvRect = VoxelEngine.instance.GetTileUv ((byte)3);
		uvs [baseVertex + 16] = new Vector2 (uvRect.xMin, uvRect.yMin);
		uvs [baseVertex + 17] = new Vector2 (uvRect.xMax, uvRect.yMin);
		uvs [baseVertex + 18] = new Vector2 (uvRect.xMax, uvRect.yMax);
		uvs [baseVertex + 19] = new Vector2 (uvRect.xMin, uvRect.yMax);
		
		uvRect = VoxelEngine.instance.GetTileUv ((byte)((hasTop) ? 3 : 4));
		uvs [baseVertex + 20] = new Vector2 (uvRect.xMin, uvRect.yMax);
		uvs [baseVertex + 21] = new Vector2 (uvRect.xMax, uvRect.yMax);
		uvs [baseVertex + 22] = new Vector2 (uvRect.xMax, uvRect.yMin); 
		uvs [baseVertex + 23] = new Vector2 (uvRect.xMin, uvRect.yMin);	
		
		uvRect = VoxelEngine.instance.GetTileUv ((byte)((hasTop) ? 3 : 4));
		uvs [baseVertex + 12] = new Vector2 (uvRect.xMin, uvRect.yMax);
		uvs [baseVertex + 13] = new Vector2 (uvRect.xMax, uvRect.yMax);
		uvs [baseVertex + 14] = new Vector2 (uvRect.xMax, uvRect.yMin);
		uvs [baseVertex + 15] = new Vector2 (uvRect.xMin, uvRect.yMin);
		
		chunkMesh.triangles = indices;
		chunkMesh.uv = uvs;
		//chunkMesh.RecalculateBounds ();
		meshCollider.sharedMesh = null;
		meshCollider.sharedMesh = chunkMesh;
	}

	public void RemoveVoxel (Vector3 point)
	{
		int x, y, z;
		FindVoxel (point, out x, out y, out z);
		RemoveVoxel (x, y, z);
	}

	void RemoveVoxel (int x, int y, int z)
	{
		// DEBUG:
		Debug.Log ("- Trying to remove voxel [chunk=(" + this.x + ", " + this.y + ", " + this.z + "), voxel=(" + x + ", " + y + ", " + z + ")]");
		
		if (x < 0 || x > width || y < 0 || y > height || z < 0 || z > depth) {
			// FIXME: checking invariants
			throw new Exception ("x < 0 || x > width || y < 0 || y > height || z < 0 || z > depth");
		}
		
		int y1 = y * (width * depth);
		int z1 = z * width;
		int voxelIndex = y1 + z1 + x;

		// FIXME: checking invariants
		if (data [voxelIndex] == 0) {
			throw new Exception ("data [voxelIndex] == 0");
		}
		
		data [voxelIndex] = 0;
		
		int baseIndex = voxelIndex * 36;
		
		int[] indices = chunkMesh.triangles;
		
		// back
		indices [baseIndex] = 0;
		indices [baseIndex + 1] = 0;
		indices [baseIndex + 2] = 0;
		indices [baseIndex + 3] = 0;
		indices [baseIndex + 4] = 0;
		indices [baseIndex + 5] = 0;
		
		// top
		indices [baseIndex + 6] = 0;
		indices [baseIndex + 7] = 0;
		indices [baseIndex + 8] = 0;
		indices [baseIndex + 9] = 0;
		indices [baseIndex + 10] = 0;
		indices [baseIndex + 11] = 0;
		
		// right
		indices [baseIndex + 12] = 0;
		indices [baseIndex + 13] = 0;
		indices [baseIndex + 14] = 0;
		indices [baseIndex + 15] = 0;
		indices [baseIndex + 16] = 0;
		indices [baseIndex + 17] = 0;
		
		// front
		indices [baseIndex + 18] = 0;
		indices [baseIndex + 19] = 0;
		indices [baseIndex + 20] = 0;
		indices [baseIndex + 21] = 0;
		indices [baseIndex + 22] = 0;
		indices [baseIndex + 23] = 0;
		
		// bottom
		indices [baseIndex + 24] = 0;
		indices [baseIndex + 25] = 0;
		indices [baseIndex + 26] = 0;
		indices [baseIndex + 27] = 0;
		indices [baseIndex + 28] = 0;
		indices [baseIndex + 29] = 0;
		
		// left
		indices [baseIndex + 30] = 0;
		indices [baseIndex + 31] = 0;
		indices [baseIndex + 32] = 0;
		indices [baseIndex + 33] = 0;
		indices [baseIndex + 34] = 0;
		indices [baseIndex + 35] = 0;

		int neighborVoxelIndex;
		int neighborBaseVertex;
		int neighborBaseIndex;
		
		// back
		if ((neighborVoxelIndex = GetFrontVoxelIndex (x, y1, z)) != -1 && data [neighborVoxelIndex] != 0) {
			neighborBaseVertex = neighborVoxelIndex * 24;
			neighborBaseIndex = neighborVoxelIndex * 36;
			indices [neighborBaseIndex] = neighborBaseVertex;
			indices [neighborBaseIndex + 1] = neighborBaseVertex + 1;
			indices [neighborBaseIndex + 2] = neighborBaseVertex + 2;
			indices [neighborBaseIndex + 3] = neighborBaseVertex;
			indices [neighborBaseIndex + 4] = neighborBaseVertex + 2;
			indices [neighborBaseIndex + 5] = neighborBaseVertex + 3;
		}
		
		// top
		if ((neighborVoxelIndex = GetBottomVoxelIndex (x, y, z1)) != -1 && data [neighborVoxelIndex] != 0) {
			neighborBaseVertex = neighborVoxelIndex * 24;
			neighborBaseIndex = neighborVoxelIndex * 36;
			indices [neighborBaseIndex + 6] = neighborBaseVertex + 4;
			indices [neighborBaseIndex + 7] = neighborBaseVertex + 5;
			indices [neighborBaseIndex + 8] = neighborBaseVertex + 6;
			indices [neighborBaseIndex + 9] = neighborBaseVertex + 4;
			indices [neighborBaseIndex + 10] = neighborBaseVertex + 6;
			indices [neighborBaseIndex + 11] = neighborBaseVertex + 7;
		}
		
		// right
		if ((neighborVoxelIndex = GetLeftVoxelIndex (x, y1, z1)) != -1 && data [neighborVoxelIndex] != 0) {
			neighborBaseVertex = neighborVoxelIndex * 24;
			neighborBaseIndex = neighborVoxelIndex * 36;
			indices [neighborBaseIndex + 12] = neighborBaseVertex + 8;
			indices [neighborBaseIndex + 13] = neighborBaseVertex + 9;
			indices [neighborBaseIndex + 14] = neighborBaseVertex + 10;
			indices [neighborBaseIndex + 15] = neighborBaseVertex + 8;
			indices [neighborBaseIndex + 16] = neighborBaseVertex + 10;
			indices [neighborBaseIndex + 17] = neighborBaseVertex + 11;
		}
		
		// front
		if ((neighborVoxelIndex = GetBackVoxelIndex (x, y1, z)) != -1 && data [neighborVoxelIndex] != 0) {
			neighborBaseVertex = neighborVoxelIndex * 24;
			neighborBaseIndex = neighborVoxelIndex * 36;
			indices [neighborBaseIndex + 18] = neighborBaseVertex + 12;
			indices [neighborBaseIndex + 19] = neighborBaseVertex + 13;
			indices [neighborBaseIndex + 20] = neighborBaseVertex + 14;
			indices [neighborBaseIndex + 21] = neighborBaseVertex + 12;
			indices [neighborBaseIndex + 22] = neighborBaseVertex + 14;
			indices [neighborBaseIndex + 23] = neighborBaseVertex + 15;
		}		
		
		// bottom
		if ((neighborVoxelIndex = GetTopVoxelIndex (x, y, z1)) != -1 && data [neighborVoxelIndex] != 0) {
			neighborBaseVertex = neighborVoxelIndex * 24;
			neighborBaseIndex = neighborVoxelIndex * 36;
			indices [neighborBaseIndex + 24] = neighborBaseVertex + 16;
			indices [neighborBaseIndex + 25] = neighborBaseVertex + 18;
			indices [neighborBaseIndex + 26] = neighborBaseVertex + 17;
			indices [neighborBaseIndex + 27] = neighborBaseVertex + 16;
			indices [neighborBaseIndex + 28] = neighborBaseVertex + 19;
			indices [neighborBaseIndex + 29] = neighborBaseVertex + 18;
		}

		// left
		if ((neighborVoxelIndex = GetRightVoxelIndex (x, y1, z1)) != -1 && data [neighborVoxelIndex] != 0) {
			neighborBaseVertex = neighborVoxelIndex * 24;
			neighborBaseIndex = neighborVoxelIndex * 36;
			indices [neighborBaseIndex + 30] = neighborBaseVertex + 20;
			indices [neighborBaseIndex + 31] = neighborBaseVertex + 22;
			indices [neighborBaseIndex + 32] = neighborBaseVertex + 21;
			indices [neighborBaseIndex + 33] = neighborBaseVertex + 20;
			indices [neighborBaseIndex + 34] = neighborBaseVertex + 23;
			indices [neighborBaseIndex + 35] = neighborBaseVertex + 22;
		}

		chunkMesh.triangles = indices;
		//chunkMesh.RecalculateBounds ();
		meshCollider.sharedMesh = null;
		meshCollider.sharedMesh = chunkMesh;
	}
	
	public void Build ()
	{
		if (width < 1 || height < 1 || depth < 1) {
			Debug.LogError ("width or height or depth < 1");
			enabled = false;
			return;
		}
				
		offset = new Vector3 (width * 0.5f * VoxelEngine.instance.voxelSize, 
		                      		  height * 0.5f * VoxelEngine.instance.voxelSize, 
		                      		  depth * 0.5f * VoxelEngine.instance.voxelSize);
		
		int numVoxels = data.Length;
		if (numVoxels > 1820) {
			Debug.LogError ("number of voxels cannot be > 1820");
			enabled = false;
			return;
		}
				
		ProceduralMesh mesh = new ProceduralMesh (numVoxels * 24, numVoxels * 36);
		Vector3 vStart = new Vector3 (0.0f, -height * 0.5f + VoxelEngine.instance.halfVoxelSize, 0.0f);
		Rect[] uvRects = new Rect[6];
		for (int y = 0; y < height; y++) {
			int y1 = y * (depth * width);
			Vector3 dStart = new Vector3 (0.0f, 0.0f, depth * 0.5f - VoxelEngine.instance.halfVoxelSize);
			for (int z = 0; z < depth; z++) {
				int z1 = z * width;
				Vector3 hStart = new Vector3 (-width * 0.5f + VoxelEngine.instance.halfVoxelSize, 0.0f, 0.0f);
				for (int x = 0; x < width; x++) {
					Vector3 center = vStart + dStart + hStart;
					byte b = data [y1 + z1 + x];
					if (b > 0) {
						int excludeFaces = 0;
						bool hasTop;
						if ((hasTop = HasTop (x, y, z1))) {
							excludeFaces |= (int)Direction.TOP;
						}
						
						if (HasBottom (x, y, z1)) {
							excludeFaces |= (int)Direction.BOTTOM;
						}
						
						if (HasLeft (x, y1, z1)) {
							excludeFaces |= (int)Direction.LEFT;
						}
						
						if (HasRight (x, y1, z1)) {
							excludeFaces |= (int)Direction.RIGHT;
						}
						
						if (HasFront (x, y1, z)) {
							excludeFaces |= (int)Direction.FRONT;
						}
						
						if (HasBack (x, y1, z)) {
							excludeFaces |= (int)Direction.BACK;
						}

						uvRects [0] = VoxelEngine.instance.GetTileUv ((byte)((hasTop) ? 3 : 4));
						uvRects [1] = VoxelEngine.instance.GetTileUv ((byte)1);
						uvRects [2] = VoxelEngine.instance.GetTileUv ((byte)((hasTop) ? 3 : 4));
						uvRects [3] = VoxelEngine.instance.GetTileUv ((byte)((hasTop) ? 3 : 4));
						uvRects [4] = VoxelEngine.instance.GetTileUv ((byte)3);
						uvRects [5] = VoxelEngine.instance.GetTileUv ((byte)((hasTop) ? 3 : 4));

						ProceduralMeshes.CreateCube (mesh, VoxelEngine.instance.voxelSize, VoxelEngine.instance.voxelSize, VoxelEngine.instance.voxelSize, center, uvRects, excludeFaces);
					} else {
						ProceduralMeshes.CreateCube (mesh, VoxelEngine.instance.voxelSize, VoxelEngine.instance.voxelSize, VoxelEngine.instance.voxelSize, center, EMPTY_UV_RECTS, EXCLUDE_ALL_FACES);
					}
					hStart += VoxelEngine.instance.right;
				}
				dStart += VoxelEngine.instance.back;
			}
			vStart += VoxelEngine.instance.up;
		}
		chunkMesh = mesh.Build ();
		chunkMesh.MarkDynamic ();
		
		if ((meshFilter = gameObject.GetComponent<MeshFilter> ()) == null) {
			meshFilter = gameObject.AddComponent<MeshFilter> ();
		}
		meshFilter.sharedMesh = chunkMesh;
				
		if ((meshCollider = gameObject.GetComponent<MeshCollider> ()) == null) {
			meshCollider = gameObject.AddComponent<MeshCollider> ();
		}
		meshCollider.sharedMesh = chunkMesh;
		
		MeshRenderer meshRenderer;
		if ((meshRenderer = gameObject.GetComponent<MeshRenderer> ()) == null) {
			meshRenderer = gameObject.AddComponent<MeshRenderer> ();
		}
		meshRenderer.sharedMaterial = VoxelEngine.instance.atlas;
	}
}