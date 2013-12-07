using UnityEngine;
using System;

[RequireComponent(typeof(VoxelCollider))]
public class VoxelChunk : MonoBehaviour
{
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
	
		bool HasBottom (int x, int y, int z1)
		{
				if (y == 0) {
						return false;
				}
		
				return data [(y - 1) * (depth * width) + z1 + x] > 0;
		}
	
		bool HasLeft (int x, int y1, int z1)
		{
				if (x == 0) {
						return false;
				}
		
				return data [y1 + z1 + x - 1] > 0;
		}
	
		bool HasRight (int x, int y1, int z1)
		{
				if (x == width - 1) {
						return false;
				}
		
				return data [y1 + z1 + x + 1] > 0;
		}
	
		bool HasFront (int x, int y1, int z)
		{
				if (z == 0) {
						return false;
				}
		
				return data [y1 + (z - 1) * width + x] > 0;
		}
	
		bool HasBack (int x, int y1, int z)
		{
				if (z == depth - 1) {
						return false;
				}
		
				return data [y1 + (z + 1) * width + x] > 0;
		}
		
		void GetVoxelCoordinates (Vector3 point, out int x, out int y, out int z)
		{
				Vector3 localPoint = transform.worldToLocalMatrix.MultiplyPoint3x4 (point);
				Vector3 voxelCoordinates = offset + (localPoint / VoxelEngine.instance.voxelSize);
		
				x = Mathf.RoundToInt (voxelCoordinates.x);
				y = Mathf.FloorToInt (voxelCoordinates.y);
				z = depth - Mathf.RoundToInt (voxelCoordinates.z);
		}
		
		public void AddVoxelAt (Vector3 point, byte tileId)
		{
				int x, y, z;
				GetVoxelCoordinates (point, out x, out y, out z);
				AddVoxelAt (x, y, z, tileId);
		}
		
		void AddVoxelAt (int x, int y, int z, byte tileId)
		{
				if (x >= width) {
						Debug.LogError ("x >= width");
						return;
				}
			    
				if (z >= depth) {
						Debug.LogError ("z >= depth");
						return;
				}
		
				if (y >= height) {
						if (terrain == null) {
								Debug.LogError ("y >= height");
						} else {
								terrain.GetChunkAbove (this.x, this.y, this.z).AddVoxelAt (x, 0, z, tileId);
						}
						return;
				}
				
				int y1 = y * (width * depth);
				int z1 = z * width;
				int voxelIndex = y1 + z1 + x;
				data [voxelIndex] = tileId;
				int baseVertex = voxelIndex * 24;
				int baseIndex = voxelIndex * 36;
				int[] indices = chunkMesh.triangles;
				Vector2[] uvs = chunkMesh.uv;
				bool hasTop = HasTop (x, y, z1);
				if (!hasTop) {
						indices [baseIndex++] = baseVertex + 4;
						indices [baseIndex++] = baseVertex + 5;
						indices [baseIndex++] = baseVertex + 6;
						indices [baseIndex++] = baseVertex + 4;
						indices [baseIndex++] = baseVertex + 6;
						indices [baseIndex++] = baseVertex + 7;
						Rect uvRect = VoxelEngine.instance.GetTileUv ((byte)1);
						uvs [baseVertex + 4] = new Vector2 (uvRect.xMin, uvRect.yMax);
						uvs [baseVertex + 5] = new Vector2 (uvRect.xMax, uvRect.yMax);
						uvs [baseVertex + 6] = new Vector2 (uvRect.xMax, uvRect.yMin);
						uvs [baseVertex + 7] = new Vector2 (uvRect.xMin, uvRect.yMin);
				}
				
				if (!HasBottom (x, y, z1)) {
						indices [baseIndex++] = baseVertex + 16;
						indices [baseIndex++] = baseVertex + 18;
						indices [baseIndex++] = baseVertex + 17;
						indices [baseIndex++] = baseVertex + 16;
						indices [baseIndex++] = baseVertex + 19;
						indices [baseIndex++] = baseVertex + 18;
						Rect uvRect = VoxelEngine.instance.GetTileUv ((byte)3);
						uvs [baseVertex + 16] = new Vector2 (uvRect.xMin, uvRect.yMin);
						uvs [baseVertex + 17] = new Vector2 (uvRect.xMax, uvRect.yMin);
						uvs [baseVertex + 18] = new Vector2 (uvRect.xMax, uvRect.yMax);
						uvs [baseVertex + 19] = new Vector2 (uvRect.xMin, uvRect.yMax);
				}
				
				if (!HasLeft (x, y1, z1)) {
						indices [baseIndex++] = baseVertex + 20;
						indices [baseIndex++] = baseVertex + 22;
						indices [baseIndex++] = baseVertex + 21;
						indices [baseIndex++] = baseVertex + 20;
						indices [baseIndex++] = baseVertex + 23;
						indices [baseIndex++] = baseVertex + 22;
						Rect uvRect = VoxelEngine.instance.GetTileUv ((byte)((hasTop) ? 3 : 4));
						uvs [baseVertex + 20] = new Vector2 (uvRect.xMin, uvRect.yMax);
						uvs [baseVertex + 21] = new Vector2 (uvRect.xMax, uvRect.yMax);
						uvs [baseVertex + 22] = new Vector2 (uvRect.xMax, uvRect.yMin); 
						uvs [baseVertex + 23] = new Vector2 (uvRect.xMin, uvRect.yMin);					
				}
				
				if (!HasRight (x, y1, z1)) {
						indices [baseIndex++] = baseVertex + 8;
						indices [baseIndex++] = baseVertex + 9;
						indices [baseIndex++] = baseVertex + 10;
						indices [baseIndex++] = baseVertex + 8;
						indices [baseIndex++] = baseVertex + 10;
						indices [baseIndex++] = baseVertex + 11;
						Rect uvRect = VoxelEngine.instance.GetTileUv ((byte)((hasTop) ? 3 : 4));
						uvs [baseVertex + 8] = new Vector2 (uvRect.xMin, uvRect.yMax);
						uvs [baseVertex + 9] = new Vector2 (uvRect.xMax, uvRect.yMax);
						uvs [baseVertex + 10] = new Vector2 (uvRect.xMax, uvRect.yMin); 
						uvs [baseVertex + 11] = new Vector2 (uvRect.xMin, uvRect.yMin);					
				}
		
				if (!HasFront (x, y1, z)) {
						indices [baseIndex++] = baseVertex + 12;
						indices [baseIndex++] = baseVertex + 13;
						indices [baseIndex++] = baseVertex + 14;
						indices [baseIndex++] = baseVertex + 12;
						indices [baseIndex++] = baseVertex + 14;
						indices [baseIndex++] = baseVertex + 15;
						Rect uvRect = VoxelEngine.instance.GetTileUv ((byte)((hasTop) ? 3 : 4));
						uvs [baseVertex + 12] = new Vector2 (uvRect.xMin, uvRect.yMax);
						uvs [baseVertex + 13] = new Vector2 (uvRect.xMax, uvRect.yMax);
						uvs [baseVertex + 14] = new Vector2 (uvRect.xMax, uvRect.yMin);
						uvs [baseVertex + 15] = new Vector2 (uvRect.xMin, uvRect.yMin);
				}
				
				if (!HasBack (x, y1, z)) {
						indices [baseIndex++] = baseVertex;
						indices [baseIndex++] = baseVertex + 1;
						indices [baseIndex++] = baseVertex + 2;
						indices [baseIndex++] = baseVertex;
						indices [baseIndex++] = baseVertex + 2;
						indices [baseIndex++] = baseVertex + 3;
						Rect uvRect = VoxelEngine.instance.GetTileUv ((byte)((hasTop) ? 3 : 4));
						uvs [baseVertex] = new Vector2 (uvRect.xMin, uvRect.yMax);
						uvs [baseVertex + 1] = new Vector2 (uvRect.xMax, uvRect.yMax);
						uvs [baseVertex + 2] = new Vector2 (uvRect.xMax, uvRect.yMin);
						uvs [baseVertex + 3] = new Vector2 (uvRect.xMin, uvRect.yMin);					
				}
				
				chunkMesh.triangles = indices;
				chunkMesh.uv = uvs;
				meshCollider.sharedMesh = null;
				meshCollider.sharedMesh = chunkMesh;
		}
	
		public void Build (bool collider = false)
		{
				if (width < 1 || height < 1 || depth < 1) {
						Debug.LogError ("width or height or depth < 1");
						enabled = false;
						return;
				}
				
				offset = new Vector3 (width * 0.5f * VoxelEngine.instance.voxelSize, 
		                      		  height * 0.5f * VoxelEngine.instance.voxelSize, 
		                      		  depth * 0.5f * VoxelEngine.instance.voxelSize);
		
				MeshRenderer meshRenderer;
				if ((meshRenderer = gameObject.GetComponent<MeshRenderer> ()) == null) {
						meshRenderer = gameObject.AddComponent<MeshRenderer> ();
				}
				meshRenderer.sharedMaterial = VoxelEngine.instance.atlas;
		
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
												bool hasTop = HasTop (x, y, z1);
												if (hasTop) {
														excludeFaces |= (int)Direction.TOP;
												} else {
														uvRects [1] = VoxelEngine.instance.GetTileUv ((byte)1);
												}
						
												if (HasBottom (x, y, z1)) {
														excludeFaces |= (int)Direction.BOTTOM;
												} else {
														uvRects [4] = VoxelEngine.instance.GetTileUv ((byte)3);
												}
						
												if (HasLeft (x, y1, z1)) {
														excludeFaces |= (int)Direction.LEFT;
												} else {
														uvRects [5] = VoxelEngine.instance.GetTileUv ((byte)((hasTop) ? 3 : 4));
												}
						
												if (HasRight (x, y1, z1)) {
														excludeFaces |= (int)Direction.RIGHT;
												} else {
														uvRects [2] = VoxelEngine.instance.GetTileUv ((byte)((hasTop) ? 3 : 4));
												}
						
												if (HasFront (x, y1, z)) {
														excludeFaces |= (int)Direction.FRONT;
												} else {
														uvRects [0] = VoxelEngine.instance.GetTileUv ((byte)((hasTop) ? 3 : 4));
												}
						
												if (HasBack (x, y1, z)) {
														excludeFaces |= (int)Direction.BACK;
												} else {
														uvRects [3] = VoxelEngine.instance.GetTileUv ((byte)((hasTop) ? 3 : 4));
												}
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
		}
}