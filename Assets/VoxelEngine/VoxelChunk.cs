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
	private const int EXCLUDE_ALL_FACES = (int)FaceDirection.TOP | (int)FaceDirection.BOTTOM | (int)FaceDirection.LEFT | (int)FaceDirection.RIGHT | (int)FaceDirection.FRONT | (int)FaceDirection.BACK;
	public int x;
	public int y;
	public int z;
	public VoxelTerrain terrain;
	public int width;
	public int height;
	public int depth;
	public Voxel[] data;
	[SerializeField]
	private int
		depth_x_width;
	[SerializeField]
	private bool
		built = false;
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
	private bool batchMode = false;
	private int[] temporaryIndices;
	private Vector2[] temporaryUvs;

	void Start ()
	{
		if (!built) {
			Build ();
		}
	}

	public void StartBatchMode ()
	{
		// FIXME: checking invariants
		if (!built) {
			throw new Exception ("!built");
		}

		if (batchMode) {
			throw new Exception ("already in batch mode");
		}

		batchMode = true;
		temporaryIndices = chunkMesh.triangles;
		temporaryUvs = chunkMesh.uv;
	}

	public void EndBatchMode ()
	{
		// FIXME: checking invariants
		if (!built) {
			throw new Exception ("!built");
		}

		if (!batchMode) {
			throw new Exception ("not in batch mode");
		}

		FlushMeshChanges (temporaryIndices, temporaryUvs);

		batchMode = false;
	}
	
	void FlushMeshChanges (int[] indices, Vector2[] uvs = null)
	{
		chunkMesh.triangles = indices;
		if (uvs != null) {
			chunkMesh.uv = uvs;
		}
		// necessary to update mesh collider in Unity 4.x
		meshCollider.sharedMesh = null;
		meshCollider.sharedMesh = chunkMesh;
	}
	
	bool HasFrontNeighbor (int x, int y1, int z)
	{
		if (z == 0) {
			return false;
		}
		
		return data [y1 + (z - 1) * width + x].id > 0;
	}
	
	int GetFrontNeighborIndex (int x, int y1, int z)
	{
		if (z == 0) {
			return -1;
		}
		
		return y1 + (z - 1) * width + x;
	}

	bool HasTopNeighbor (int x, int y, int z1)
	{
		if (y == height - 1) {
			return false;
		}
		
		return data [(y + 1) * depth_x_width + z1 + x].id > 0;
	}

	int GetTopNeighborIndex (int x, int y, int z1)
	{
		if (y == height - 1) {
			return -1;
		}
		
		return (y + 1) * depth_x_width + z1 + x;
	}
	
	bool HasRightNeighbor (int x, int y1, int z1)
	{
		if (x == width - 1) {
			return false;
		}
		
		return data [y1 + z1 + x + 1].id > 0;
	}

	int GetRightNeighborIndex (int x, int y1, int z1)
	{
		if (x == width - 1) {
			return -1;
		}
		
		return y1 + z1 + x + 1;
	}
	
	bool HasBackNeighbor (int x, int y1, int z)
	{
		if (z == depth - 1) {
			return false;
		}
		
		return data [y1 + (z + 1) * width + x].id > 0;
	}

	int GetBackNeighborIndex (int x, int y1, int z)
	{
		if (z == depth - 1) {
			return -1;
		}
		
		return y1 + (z + 1) * width + x;
	}
	
	bool HasBottomNeighbor (int x, int y, int z1)
	{
		if (y == 0) {
			return false;
		}
		
		return data [(y - 1) * depth_x_width + z1 + x].id > 0;
	}
	
	int GetBottomNeighborIndex (int x, int y, int z1)
	{
		if (y == 0) {
			return -1;
		}
		
		return (y - 1) * depth_x_width + z1 + x;
	}
	
	bool HasLeftNeighbor (int x, int y1, int z1)
	{
		if (x == 0) {
			return false;
		}
		
		return data [y1 + z1 + x - 1].id > 0;
	}
	
	int GetLeftNeighborIndex (int x, int y1, int z1)
	{
		if (x == 0) {
			return -1;
		}
		
		return y1 + z1 + x - 1;
	}

	public bool QueryVoxel (int x, int y, int z)
	{
		return (data [y * depth_x_width + z * width + x].id != 0);
	}

	void FindVoxel (Vector3 point, out int x, out int y, out int z)
	{
		Vector3 localPoint = transform.worldToLocalMatrix.MultiplyPoint3x4 (point);
		Vector3 voxelCoordinates = offset + (localPoint / VoxelEngine.instance.voxelSize);
		
		x = Mathf.FloorToInt (voxelCoordinates.x);
		y = Mathf.FloorToInt (voxelCoordinates.y);
		z = Mathf.FloorToInt (voxelCoordinates.z);

		// DEBUG:
		// Debug.Log ("********** Finding voxel **********");
		// Debug.Log ("1. Snapped voxel coordinates: (" + x + ", " + y + ", " + z + ")");
		
		int voxelIndex = y * depth_x_width + z * width + x;
		
		if (data [voxelIndex].id != 0) {
			// DEBUG:
			// Debug.Log ("2. Voxel found, returning");
			return;
		}
		
		float xReminder = (voxelCoordinates.x - x);
		float yReminder = (voxelCoordinates.y - y);
		float zReminder = (voxelCoordinates.z - z);

		// DEBUG:
		//// Debug.Log ("3. Calculating coords. remainders (x=" + xReminder + ", y=" + yReminder + ", z=" + zReminder + ")");
		
		if (xReminder < EPSILON && xReminder > -EPSILON) {
			// DEBUG:
			// Debug.Log ("4. Moving voxel coordenate left");
			x = Mathf.Max (0, x - 1);
		} else if (yReminder < EPSILON && yReminder > -EPSILON) {
			// DEBUG:
			// Debug.Log ("4. Moving voxel coordenate down");
			y = Mathf.Max (0, y - 1);
		} else if (zReminder < EPSILON && zReminder > -EPSILON) {
			// DEBUG:
			// Debug.Log ("4. Moving voxel coordenate forward");
			z = Mathf.Max (0, z - 1);
		}

		// DEBUG:
		// Debug.Log ("5. New voxel coordinates: (" + x + ", " + y + ", " + z + ")");
	}
		
	void FindAdjacentEmptyVoxel (Vector3 point, out int x, out int y, out int z)
	{
		Vector3 localPoint = transform.worldToLocalMatrix.MultiplyPoint3x4 (point);
		Vector3 voxelCoordinates = offset + (localPoint / VoxelEngine.instance.voxelSize);

		x = Mathf.FloorToInt (voxelCoordinates.x);
		y = Mathf.FloorToInt (voxelCoordinates.y);
		z = Mathf.FloorToInt (voxelCoordinates.z);

		// DEBUG:
		// Debug.Log ("********** Finding adjacent voxel **********");
		// Debug.Log ("1. Snapped voxel coordinates: (" + x + ", " + y + ", " + z + ")");

		if (x == width || y == height || z == depth) {
			// DEBUG:
			// Debug.Log ("2. Picking voxel at chunk's border");
			return;
		}

		// DEBUG:
		// Debug.Log ("2. Picking internal chunk voxel");

		int voxelIndex = y * depth_x_width + z * width + x;

		if (data [voxelIndex].id == 0) {
			// DEBUG:
			// Debug.Log ("3. Adjacent empty voxel found, returning");
			return;
		}

		// DEBUG:
		// Debug.Log ("3. Occupied voxel found, supposed error in snapping");

		float xReminder = (voxelCoordinates.x - x);
		float yReminder = (voxelCoordinates.y - y);
		float zReminder = (voxelCoordinates.z - z);

		// DEBUG:
		// Debug.Log ("4. Calculating coords. remainders (x=" + xReminder + ", y=" + yReminder + ", z=" + zReminder + ")");

		if (xReminder < EPSILON && xReminder > -EPSILON) {
			// DEBUG:
			// Debug.Log ("5. Moving voxel coordenate left");
			x--;
		} else if (yReminder < EPSILON && yReminder > -EPSILON) {
			// DEBUG:
			// Debug.Log ("5. Moving voxel coordenate down");
			y--;
		} else if (zReminder < EPSILON && zReminder > -EPSILON) {
			// DEBUG:
			// Debug.Log ("5. Moving voxel coordenate forward");
			z--;
		}

		// DEBUG:
		// Debug.Log ("6. New voxel coordinates: (" + x + ", " + y + ", " + z + ")");
	}
		
	public void AddVoxel (byte voxelId, Vector3 point)
	{
		// FIXME: checking invariants
		if (!built) {
			throw new Exception ("!built");
		}

		int x, y, z;
		FindAdjacentEmptyVoxel (point, out x, out y, out z);
		AddVoxel (voxelId, x, y, z);
	}

	public void AddVoxel (byte voxelId, int x, int y, int z)
	{
		// FIXME: checking invariants
		if (!built) {
			throw new Exception ("!built");
		}

		// DEBUG:
		// Debug.Log ("- Trying to add voxel [chunk=(" + this.x + ", " + this.y + ", " + this.z + "), voxel=(" + x + ", " + y + ", " + z + ")]");
		
		if (x < 0) {
			if (terrain == null) {
				throw new Exception ("can't add voxel");
			} else {
				VoxelChunk chunk = terrain.GetChunkAtLeft (this.x, this.y, this.z);
				if (chunk != null) {
					// DEBUG:
					// Debug.Log ("- Cross-boundaries voxel addition: passing command to chunk at left [from (" + this.x + ", " + this.y + ", " + this.z + ") to (" + chunk.x + ", " + chunk.y + ", " + chunk.z + ")]");
					chunk.AddVoxel (voxelId, chunk.width - 1, y, z);
				}
			}
			
			return;
		} else if (x >= width) {
			if (terrain == null) {
				throw new Exception ("can't add voxel");
			} else {
				VoxelChunk chunk = terrain.GetChunkAtRight (this.x, this.y, this.z);
				if (chunk != null) {
					// DEBUG:
					// Debug.Log ("- Cross-boundaries voxel addition: passing command to chunk at right [from (" + this.x + ", " + this.y + ", " + this.z + ") to (" + chunk.x + ", " + chunk.y + ", " + chunk.z + ")]");
					chunk.AddVoxel (voxelId, 0, y, z);
				}
			}
			
			return;
		}
		
		if (z < 0) {
			if (terrain == null) {
				throw new Exception ("can't add voxel");
			} else {
				VoxelChunk chunk = terrain.GetChunkInFront (this.x, this.y, this.z);
				if (chunk != null) {
					// DEBUG:
					// Debug.Log ("- Cross-boundaries voxel addition: passing command to chunk in front [from (" + this.x + ", " + this.y + ", " + this.z + ") to (" + chunk.x + ", " + chunk.y + ", " + chunk.z + ")]");
					chunk.AddVoxel (voxelId, x, y, chunk.depth - 1);
				}
			}
			
			return;
		} else if (z >= depth) {
			if (terrain == null) {
				throw new Exception ("can't add voxel");
			} else {
				VoxelChunk chunk = terrain.GetChunkBehind (this.x, this.y, this.z);
				if (chunk != null) {
					// DEBUG:
					// Debug.Log ("- Cross-boundaries voxel addition: passing command to chunk behind [from (" + this.x + ", " + this.y + ", " + this.z + ") to (" + chunk.x + ", " + chunk.y + ", " + chunk.z + ")]");
					chunk.AddVoxel (voxelId, x, y, 0);
				}
			}
			
			return;
		}
		
		if (y < 0) {
			if (terrain == null) {
				throw new Exception ("can't add voxel");
			} else {
				VoxelChunk chunk = terrain.GetChunkBelow (this.x, this.y, this.z);
				if (chunk != null) {
					// DEBUG:
					// Debug.Log ("- Cross-boundaries voxel addition: passing command to chunk below [from (" + this.x + ", " + this.y + ", " + this.z + ") to (" + chunk.x + ", " + chunk.y + ", " + chunk.z + ")]");
					chunk.AddVoxel (voxelId, x, chunk.height - 1, z);
				}
			}
			
			return;
		} else if (y >= height) {
			if (terrain == null) {
				throw new Exception ("can't add voxel");
			} else {
				VoxelChunk chunk = terrain.GetChunkAbove (this.x, this.y, this.z);
				if (chunk != null) {
					// DEBUG:
					// Debug.Log ("- Cross-boundaries voxel addition: passing command to chunk above [from (" + this.x + ", " + this.y + ", " + this.z + ") to (" + chunk.x + ", " + chunk.y + ", " + chunk.z + ")]");
					chunk.AddVoxel (voxelId, x, 0, z);
				}
			}
			
			return;
		}
		
		int y1 = y * (width * depth);
		int z1 = z * width;
		int voxelIndex = y1 + z1 + x;

		// FIXME: checking invariants
		if (data [voxelIndex].id != 0) {
			// Debug.LogWarning ("data [voxelIndex].id != 0");
			return;
		}
		
		data [voxelIndex].id = voxelId;
		
		int baseVertex = voxelIndex * 24;
		int baseIndex = voxelIndex * 36;
		
		int[] indices = (batchMode) ? temporaryIndices : chunkMesh.triangles;
		Vector2[] uvs = (batchMode) ? temporaryUvs : chunkMesh.uv;
		
		bool hasTopNeighbor;
		
		int neighborIndex;
		int neighborBaseIndex;
		
		if ((neighborIndex = GetFrontNeighborIndex (x, y1, z)) != -1 && data [neighborIndex].id != 0) {
			// remove back face from front neighbor
			neighborBaseIndex = neighborIndex * 36;
			indices [neighborBaseIndex + 18] = 0;
			indices [neighborBaseIndex + 19] = 0;
			indices [neighborBaseIndex + 20] = 0;
			indices [neighborBaseIndex + 21] = 0;
			indices [neighborBaseIndex + 22] = 0;
			indices [neighborBaseIndex + 23] = 0;			
		} else {
			// add front face to new voxel
			indices [baseIndex] = baseVertex;
			indices [baseIndex + 1] = baseVertex + 1;
			indices [baseIndex + 2] = baseVertex + 2;
			indices [baseIndex + 3] = baseVertex;
			indices [baseIndex + 4] = baseVertex + 2;
			indices [baseIndex + 5] = baseVertex + 3;
		}
		
		if ((neighborIndex = GetTopNeighborIndex (x, y, z1)) != -1 && data [neighborIndex].id != 0) {
			hasTopNeighbor = true;
			// remove bottom face from top neighbor
			neighborBaseIndex = neighborIndex * 36;
			indices [neighborBaseIndex + 24] = 0;
			indices [neighborBaseIndex + 25] = 0;
			indices [neighborBaseIndex + 26] = 0;
			indices [neighborBaseIndex + 27] = 0;
			indices [neighborBaseIndex + 28] = 0;
			indices [neighborBaseIndex + 29] = 0;			
		} else {
			// add top face to new voxel
			hasTopNeighbor = false;
			indices [baseIndex + 6] = baseVertex + 4;
			indices [baseIndex + 7] = baseVertex + 5;
			indices [baseIndex + 8] = baseVertex + 6;
			indices [baseIndex + 9] = baseVertex + 4;
			indices [baseIndex + 10] = baseVertex + 6;
			indices [baseIndex + 11] = baseVertex + 7;
		}
		
		if ((neighborIndex = GetRightNeighborIndex (x, y1, z1)) != -1 && data [neighborIndex].id != 0) {
			// remove left face from right neighbor
			neighborBaseIndex = neighborIndex * 36;
			indices [neighborBaseIndex + 30] = 0;
			indices [neighborBaseIndex + 31] = 0;
			indices [neighborBaseIndex + 32] = 0;
			indices [neighborBaseIndex + 33] = 0;
			indices [neighborBaseIndex + 34] = 0;
			indices [neighborBaseIndex + 35] = 0;
		} else {
			// add right face to new voxel
			indices [baseIndex + 12] = baseVertex + 8;
			indices [baseIndex + 13] = baseVertex + 9;
			indices [baseIndex + 14] = baseVertex + 10;
			indices [baseIndex + 15] = baseVertex + 8;
			indices [baseIndex + 16] = baseVertex + 10;
			indices [baseIndex + 17] = baseVertex + 11;
		}
		
		if ((neighborIndex = GetBackNeighborIndex (x, y1, z)) != -1 && data [neighborIndex].id != 0) {
			// remove front face from back neighbor
			neighborBaseIndex = neighborIndex * 36;
			indices [neighborBaseIndex] = 0;
			indices [neighborBaseIndex + 1] = 0;
			indices [neighborBaseIndex + 2] = 0;
			indices [neighborBaseIndex + 3] = 0;
			indices [neighborBaseIndex + 4] = 0;
			indices [neighborBaseIndex + 5] = 0;
		} else {
			// add back face to new voxel
			indices [baseIndex + 18] = baseVertex + 12;
			indices [baseIndex + 19] = baseVertex + 13;
			indices [baseIndex + 20] = baseVertex + 14;
			indices [baseIndex + 21] = baseVertex + 12;
			indices [baseIndex + 22] = baseVertex + 14;
			indices [baseIndex + 23] = baseVertex + 15;
		}

		if ((neighborIndex = GetBottomNeighborIndex (x, y, z1)) != -1 && data [neighborIndex].id != 0) {
			// remove top face from bottom neighbor
			neighborBaseIndex = neighborIndex * 36;
			indices [neighborBaseIndex + 6] = 0;
			indices [neighborBaseIndex + 7] = 0;
			indices [neighborBaseIndex + 8] = 0;
			indices [neighborBaseIndex + 9] = 0;
			indices [neighborBaseIndex + 10] = 0;
			indices [neighborBaseIndex + 11] = 0;
		} else {
			// add bottom face to new voxel
			indices [baseIndex + 24] = baseVertex + 16;
			indices [baseIndex + 25] = baseVertex + 18;
			indices [baseIndex + 26] = baseVertex + 17;
			indices [baseIndex + 27] = baseVertex + 16;
			indices [baseIndex + 28] = baseVertex + 19;
			indices [baseIndex + 29] = baseVertex + 18;
		}
		
		if ((neighborIndex = GetLeftNeighborIndex (x, y1, z1)) != -1 && data [neighborIndex].id != 0) {
			// remove right face from left neighbor
			neighborBaseIndex = neighborIndex * 36;
			indices [neighborBaseIndex + 12] = 0;
			indices [neighborBaseIndex + 13] = 0;
			indices [neighborBaseIndex + 14] = 0;
			indices [neighborBaseIndex + 15] = 0;
			indices [neighborBaseIndex + 16] = 0;
			indices [neighborBaseIndex + 17] = 0;
		} else {
			// add left face to new voxel
			indices [baseIndex + 30] = baseVertex + 20;
			indices [baseIndex + 31] = baseVertex + 22;
			indices [baseIndex + 32] = baseVertex + 21;
			indices [baseIndex + 33] = baseVertex + 20;
			indices [baseIndex + 34] = baseVertex + 23;
			indices [baseIndex + 35] = baseVertex + 22;
		}

		int frontFaceTileId, topFaceTileId, rightFaceTileId, backFaceTileId, bottomFaceTileId, leftFaceTileId;
		VoxelEngine.instance.GetVoxelFaceMapping (voxelId, out frontFaceTileId, out topFaceTileId, out rightFaceTileId, out backFaceTileId, out bottomFaceTileId, out leftFaceTileId, hasTopNeighbor);

		// front
		Rect uvRect = VoxelEngine.instance.GetTileUv (frontFaceTileId);
		uvs [baseVertex] = new Vector2 (uvRect.xMin, uvRect.yMax);
		uvs [baseVertex + 1] = new Vector2 (uvRect.xMax, uvRect.yMax);
		uvs [baseVertex + 2] = new Vector2 (uvRect.xMax, uvRect.yMin);
		uvs [baseVertex + 3] = new Vector2 (uvRect.xMin, uvRect.yMin);
		
		// top
		uvRect = VoxelEngine.instance.GetTileUv (topFaceTileId);
		uvs [baseVertex + 4] = new Vector2 (uvRect.xMin, uvRect.yMax);
		uvs [baseVertex + 5] = new Vector2 (uvRect.xMax, uvRect.yMax);
		uvs [baseVertex + 6] = new Vector2 (uvRect.xMax, uvRect.yMin);
		uvs [baseVertex + 7] = new Vector2 (uvRect.xMin, uvRect.yMin);
		
		// right
		uvRect = VoxelEngine.instance.GetTileUv (rightFaceTileId);
		uvs [baseVertex + 8] = new Vector2 (uvRect.xMin, uvRect.yMax);
		uvs [baseVertex + 9] = new Vector2 (uvRect.xMax, uvRect.yMax);
		uvs [baseVertex + 10] = new Vector2 (uvRect.xMax, uvRect.yMin); 
		uvs [baseVertex + 11] = new Vector2 (uvRect.xMin, uvRect.yMin);		
		
		// back
		uvRect = VoxelEngine.instance.GetTileUv (backFaceTileId);
		uvs [baseVertex + 12] = new Vector2 (uvRect.xMin, uvRect.yMin);
		uvs [baseVertex + 13] = new Vector2 (uvRect.xMax, uvRect.yMin);
		uvs [baseVertex + 14] = new Vector2 (uvRect.xMax, uvRect.yMax);
		uvs [baseVertex + 15] = new Vector2 (uvRect.xMin, uvRect.yMax);
		
		// bottom
		uvRect = VoxelEngine.instance.GetTileUv (bottomFaceTileId);
		uvs [baseVertex + 16] = new Vector2 (uvRect.xMin, uvRect.yMax);
		uvs [baseVertex + 17] = new Vector2 (uvRect.xMax, uvRect.yMax);
		uvs [baseVertex + 18] = new Vector2 (uvRect.xMax, uvRect.yMin); 
		uvs [baseVertex + 19] = new Vector2 (uvRect.xMin, uvRect.yMin);	
		
		// left
		uvRect = VoxelEngine.instance.GetTileUv (leftFaceTileId);
		uvs [baseVertex + 20] = new Vector2 (uvRect.xMin, uvRect.yMax);
		uvs [baseVertex + 21] = new Vector2 (uvRect.xMax, uvRect.yMax);
		uvs [baseVertex + 22] = new Vector2 (uvRect.xMax, uvRect.yMin);
		uvs [baseVertex + 23] = new Vector2 (uvRect.xMin, uvRect.yMin);

		if (!batchMode) {
			FlushMeshChanges (indices, uvs);
		}
	}

	public void RemoveVoxel (Vector3 point)
	{
		// FIXME: checking invariants
		if (!built) {
			throw new Exception ("!built");
		}

		int x, y, z;
		FindVoxel (point, out x, out y, out z);
		RemoveVoxel (x, y, z);
	}

	public void RemoveVoxel (int x, int y, int z)
	{
		// FIXME: checking invariants
		if (!built) {
			throw new Exception ("!built");
		}

		// DEBUG:
		// Debug.Log ("- Trying to remove voxel [chunk=(" + this.x + ", " + this.y + ", " + this.z + "), voxel=(" + x + ", " + y + ", " + z + ")]");
		
		if (x < 0 || x > width || y < 0 || y > height || z < 0 || z > depth) {
			// FIXME: checking invariants
			throw new Exception ("x < 0 || x > width || y < 0 || y > height || z < 0 || z > depth");
		}
		
		int y1 = y * (width * depth);
		int z1 = z * width;
		int voxelIndex = y1 + z1 + x;

		// FIXME: checking invariants
		if (data [voxelIndex].id == 0) {
			// Debug.LogWarning ("data [voxelIndex].id == 0");
			return;
		}
		
		data [voxelIndex].id = 0;
		
		int baseIndex = voxelIndex * 36;
		
		int[] indices = (batchMode) ? temporaryIndices : chunkMesh.triangles;
		
		// remove front face from current voxel
		indices [baseIndex] = 0;
		indices [baseIndex + 1] = 0;
		indices [baseIndex + 2] = 0;
		indices [baseIndex + 3] = 0;
		indices [baseIndex + 4] = 0;
		indices [baseIndex + 5] = 0;
		
		// remove top face from current voxel
		indices [baseIndex + 6] = 0;
		indices [baseIndex + 7] = 0;
		indices [baseIndex + 8] = 0;
		indices [baseIndex + 9] = 0;
		indices [baseIndex + 10] = 0;
		indices [baseIndex + 11] = 0;
		
		// remove right face from current voxel
		indices [baseIndex + 12] = 0;
		indices [baseIndex + 13] = 0;
		indices [baseIndex + 14] = 0;
		indices [baseIndex + 15] = 0;
		indices [baseIndex + 16] = 0;
		indices [baseIndex + 17] = 0;
		
		// remove back face from current voxel
		indices [baseIndex + 18] = 0;
		indices [baseIndex + 19] = 0;
		indices [baseIndex + 20] = 0;
		indices [baseIndex + 21] = 0;
		indices [baseIndex + 22] = 0;
		indices [baseIndex + 23] = 0;
		
		// remove bottom face from current voxel
		indices [baseIndex + 24] = 0;
		indices [baseIndex + 25] = 0;
		indices [baseIndex + 26] = 0;
		indices [baseIndex + 27] = 0;
		indices [baseIndex + 28] = 0;
		indices [baseIndex + 29] = 0;
		
		// remove left face from current voxel
		indices [baseIndex + 30] = 0;
		indices [baseIndex + 31] = 0;
		indices [baseIndex + 32] = 0;
		indices [baseIndex + 33] = 0;
		indices [baseIndex + 34] = 0;
		indices [baseIndex + 35] = 0;

		int neighborIndex;
		int neighborBaseVertex;
		int neighborBaseIndex;
		
		// add front face to back neighbor
		if ((neighborIndex = GetBackNeighborIndex (x, y1, z)) != -1 && data [neighborIndex].id != 0) {
			neighborBaseVertex = neighborIndex * 24;
			neighborBaseIndex = neighborIndex * 36;
			indices [neighborBaseIndex] = neighborBaseVertex;
			indices [neighborBaseIndex + 1] = neighborBaseVertex + 1;
			indices [neighborBaseIndex + 2] = neighborBaseVertex + 2;
			indices [neighborBaseIndex + 3] = neighborBaseVertex;
			indices [neighborBaseIndex + 4] = neighborBaseVertex + 2;
			indices [neighborBaseIndex + 5] = neighborBaseVertex + 3;
		}
		
		// add top face to bottom neighbor
		if ((neighborIndex = GetBottomNeighborIndex (x, y, z1)) != -1 && data [neighborIndex].id != 0) {
			neighborBaseVertex = neighborIndex * 24;
			neighborBaseIndex = neighborIndex * 36;
			indices [neighborBaseIndex + 6] = neighborBaseVertex + 4;
			indices [neighborBaseIndex + 7] = neighborBaseVertex + 5;
			indices [neighborBaseIndex + 8] = neighborBaseVertex + 6;
			indices [neighborBaseIndex + 9] = neighborBaseVertex + 4;
			indices [neighborBaseIndex + 10] = neighborBaseVertex + 6;
			indices [neighborBaseIndex + 11] = neighborBaseVertex + 7;
		}
		
		// add right face to left neighbor
		if ((neighborIndex = GetLeftNeighborIndex (x, y1, z1)) != -1 && data [neighborIndex].id != 0) {
			neighborBaseVertex = neighborIndex * 24;
			neighborBaseIndex = neighborIndex * 36;
			indices [neighborBaseIndex + 12] = neighborBaseVertex + 8;
			indices [neighborBaseIndex + 13] = neighborBaseVertex + 9;
			indices [neighborBaseIndex + 14] = neighborBaseVertex + 10;
			indices [neighborBaseIndex + 15] = neighborBaseVertex + 8;
			indices [neighborBaseIndex + 16] = neighborBaseVertex + 10;
			indices [neighborBaseIndex + 17] = neighborBaseVertex + 11;
		}
		
		// add back face to front neighbor
		if ((neighborIndex = GetFrontNeighborIndex (x, y1, z)) != -1 && data [neighborIndex].id != 0) {
			neighborBaseVertex = neighborIndex * 24;
			neighborBaseIndex = neighborIndex * 36;
			indices [neighborBaseIndex + 18] = neighborBaseVertex + 12;
			indices [neighborBaseIndex + 19] = neighborBaseVertex + 13;
			indices [neighborBaseIndex + 20] = neighborBaseVertex + 14;
			indices [neighborBaseIndex + 21] = neighborBaseVertex + 12;
			indices [neighborBaseIndex + 22] = neighborBaseVertex + 14;
			indices [neighborBaseIndex + 23] = neighborBaseVertex + 15;
		}		
		
		// add bottom face to top neighbor
		if ((neighborIndex = GetTopNeighborIndex (x, y, z1)) != -1 && data [neighborIndex].id != 0) {
			neighborBaseVertex = neighborIndex * 24;
			neighborBaseIndex = neighborIndex * 36;
			indices [neighborBaseIndex + 24] = neighborBaseVertex + 16;
			indices [neighborBaseIndex + 25] = neighborBaseVertex + 18;
			indices [neighborBaseIndex + 26] = neighborBaseVertex + 17;
			indices [neighborBaseIndex + 27] = neighborBaseVertex + 16;
			indices [neighborBaseIndex + 28] = neighborBaseVertex + 19;
			indices [neighborBaseIndex + 29] = neighborBaseVertex + 18;
		}

		// add left face to right neighbor
		if ((neighborIndex = GetRightNeighborIndex (x, y1, z1)) != -1 && data [neighborIndex].id != 0) {
			neighborBaseVertex = neighborIndex * 24;
			neighborBaseIndex = neighborIndex * 36;
			indices [neighborBaseIndex + 30] = neighborBaseVertex + 20;
			indices [neighborBaseIndex + 31] = neighborBaseVertex + 22;
			indices [neighborBaseIndex + 32] = neighborBaseVertex + 21;
			indices [neighborBaseIndex + 33] = neighborBaseVertex + 20;
			indices [neighborBaseIndex + 34] = neighborBaseVertex + 23;
			indices [neighborBaseIndex + 35] = neighborBaseVertex + 22;
		}

		if (!batchMode) {
			FlushMeshChanges (indices);
		}
	}
	
	public void Build ()
	{
		if (width < 1 || height < 1 || depth < 1) {
			// Debug.LogError ("width or height or depth < 1");
			enabled = false;
			return;
		}
		
		if (data == null) {
			// Debug.LogError ("data == null");
			enabled = false;
			return;
		}
		
		int numVoxels = data.Length;
		if (numVoxels > 1820) {
			// Debug.LogError ("number of voxels cannot be > 1820");
			enabled = false;
			return;
		}
		
		depth_x_width = depth * width;
		
		float halfWidth = width * 0.5f;
		float halfHeight = height * 0.5f;
		float halfDepth = depth * 0.5f;
		
		offset = new Vector3 (halfWidth * VoxelEngine.instance.voxelSize, halfHeight * VoxelEngine.instance.voxelSize, halfDepth * VoxelEngine.instance.voxelSize);
				
		ProceduralMesh mesh = new ProceduralMesh (numVoxels * 24, numVoxels * 36);
		float yOffset = -halfHeight * VoxelEngine.instance.voxelSize + VoxelEngine.instance.halfVoxelSize;
		float zOffset = -halfDepth * VoxelEngine.instance.voxelSize + VoxelEngine.instance.halfVoxelSize;
		float xOffset = -halfWidth * VoxelEngine.instance.voxelSize + VoxelEngine.instance.halfVoxelSize;
		Vector3 yStart = new Vector3 (0.0f, yOffset, 0.0f);
		Rect[] uvRects = new Rect[6];
		for (int y = 0; y < height; y++) {
			int y1 = y * depth_x_width;
			Vector3 zStart = new Vector3 (0.0f, 0.0f, zOffset);
			for (int z = 0; z < depth; z++) {
				int z1 = z * width;
				Vector3 xStart = new Vector3 (xOffset, 0.0f, 0.0f);
				for (int x = 0; x < width; x++) {
					Vector3 center = yStart + zStart + xStart;
					byte voxelId = data [y1 + z1 + x].id;
					if (voxelId > 0) {
						int excludeFaces = 0;
						
						if (HasFrontNeighbor (x, y1, z)) {
							excludeFaces |= (int)FaceDirection.FRONT;
						}						
						
						bool hasTopNeighbor;
						if ((hasTopNeighbor = HasTopNeighbor (x, y, z1))) {
							excludeFaces |= (int)FaceDirection.TOP;
						}
						
						if (HasRightNeighbor (x, y1, z1)) {
							excludeFaces |= (int)FaceDirection.RIGHT;
						}						
						
						if (HasBottomNeighbor (x, y, z1)) {
							excludeFaces |= (int)FaceDirection.BOTTOM;
						}
						
						if (HasBackNeighbor (x, y1, z)) {
							excludeFaces |= (int)FaceDirection.BACK;
						}
						
						if (HasLeftNeighbor (x, y1, z1)) {
							excludeFaces |= (int)FaceDirection.LEFT;
						}

						int frontFaceTileId, topFaceTileId, rightFaceTileId, backFaceTileId, bottomFaceTileId, leftFaceTileId;
						VoxelEngine.instance.GetVoxelFaceMapping (voxelId, out frontFaceTileId, out topFaceTileId, out rightFaceTileId, out backFaceTileId, out bottomFaceTileId, out leftFaceTileId, hasTopNeighbor);

						// front
						uvRects [0] = VoxelEngine.instance.GetTileUv (frontFaceTileId);
						// top
						uvRects [1] = VoxelEngine.instance.GetTileUv (topFaceTileId);
						// right
						uvRects [2] = VoxelEngine.instance.GetTileUv (rightFaceTileId);
						// back
						uvRects [3] = VoxelEngine.instance.GetTileUv (backFaceTileId);
						// bottom
						uvRects [4] = VoxelEngine.instance.GetTileUv (bottomFaceTileId);
						// left
						uvRects [5] = VoxelEngine.instance.GetTileUv (leftFaceTileId);

						ProceduralMeshes.CreateCube (mesh, VoxelEngine.instance.voxelSize, VoxelEngine.instance.voxelSize, VoxelEngine.instance.voxelSize, center, uvRects, excludeFaces);
					} else {
						ProceduralMeshes.CreateCube (mesh, VoxelEngine.instance.voxelSize, VoxelEngine.instance.voxelSize, VoxelEngine.instance.voxelSize, center, EMPTY_UV_RECTS, EXCLUDE_ALL_FACES);
					}
					xStart += VoxelEngine.instance.right;
				}
				zStart += VoxelEngine.instance.forward;
			}
			yStart += VoxelEngine.instance.up;
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

		built = true;
	}
}