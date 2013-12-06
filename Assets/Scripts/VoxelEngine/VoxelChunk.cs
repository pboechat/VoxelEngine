using UnityEngine;
using System;

[RequireComponent(typeof(VoxelCollider))]
public class VoxelChunk : MonoBehaviour
{
		public int width;
		public int height;
		public int depth;
		public byte[] data;
		public Material material;

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

		public void Build ()
		{
				if (width < 1 || height < 1 || depth < 1) {
						Debug.LogError ("width or height or depth < 1");
						enabled = false;
						return;
				}

				MeshRenderer meshRenderer;
				if ((meshRenderer = gameObject.GetComponent<MeshRenderer> ()) == null) {
						meshRenderer = gameObject.AddComponent<MeshRenderer> ();
				}
				meshRenderer.sharedMaterial = material;
				
				ProceduralMesh mesh = new ProceduralMesh ();
				Vector3 vStart = new Vector3 (0.0f, -height * 0.5f + VoxelEngine.instance.halfVoxelSize, 0.0f);
				for (int y = 0; y < height; y++) {
						int y1 = y * (depth * width);
						Vector3 dStart = new Vector3 (0.0f, 0.0f, depth * 0.5f - VoxelEngine.instance.halfVoxelSize);
						for (int z = 0; z < depth; z++) {
								int z1 = z * width;
								Vector3 hStart = new Vector3 (-width * 0.5f + VoxelEngine.instance.halfVoxelSize, 0.0f, 0.0f);
								for (int x = 0; x < width; x++) {
										byte b = data [y1 + z1 + x];
										if (b > 0) {
												int excludeFaces = 0;
												if (HasTop (x, y, z1)) {
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
												
												Vector3 center = vStart + dStart + hStart;
												ProceduralMeshes.CreateCube (mesh, VoxelEngine.instance.voxelSize, VoxelEngine.instance.voxelSize, VoxelEngine.instance.voxelSize, center, excludeFaces);
										}
										hStart += VoxelEngine.instance.right;
								}
								dStart += VoxelEngine.instance.back;
						}
						vStart += VoxelEngine.instance.up;
				}
				
				MeshFilter meshFilter;
				if ((meshFilter = gameObject.GetComponent<MeshFilter> ()) == null) {
						meshFilter = gameObject.AddComponent<MeshFilter> ();
				}
				meshFilter.sharedMesh = mesh.Build ();
		}
}
