using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ProceduralMeshes
{
		private static readonly Rect[] DEFAULT_CUBIC_UVS = new Rect[] {
				new Rect (0.0f, 0.0f, 1.0f, 1.0f),
				new Rect (0.0f, 0.0f, 1.0f, 1.0f),
				new Rect (0.0f, 0.0f, 1.0f, 1.0f),
				new Rect (0.0f, 0.0f, 1.0f, 1.0f),
				new Rect (0.0f, 0.0f, 1.0f, 1.0f),
				new Rect (0.0f, 0.0f, 1.0f, 1.0f)
		};

		public static void CreateCube (ProceduralMesh mesh, float width, float height, float depth, int excludeFaces = 0)
		{
				CreateCube (mesh, width, height, depth, Vector3.zero, DEFAULT_CUBIC_UVS, excludeFaces);
		}
		
		public static void CreateCube (ProceduralMesh mesh, float width, float height, float depth, Vector3 center, int excludeFaces = 0)
		{
				CreateCube (mesh, width, height, depth, center, DEFAULT_CUBIC_UVS, excludeFaces);
		}

		public static void CreateCube (ProceduralMesh mesh, float width, float height, float depth, Vector3 center, Rect[] uvRect, int excludeFaces = 0)
		{
				float halfWidth = width * 0.5f;
				float halfHeight = height * 0.5f;
				float halfDepth = depth * 0.5f;
		
				bool hasTopFace = (excludeFaces & (int)Direction.TOP) == 0, 
				hasBottomFace = (excludeFaces & (int)Direction.BOTTOM) == 0, 
				hasLeftFace = (excludeFaces & (int)Direction.LEFT) == 0, 
				hasRightFace = (excludeFaces & (int)Direction.RIGHT) == 0,
				hasFrontFace = (excludeFaces & (int)Direction.FRONT) == 0,
				hasBackFace = (excludeFaces & (int)Direction.BACK) == 0;
		
				int i = mesh.vertices.Count;
		
				// back
				mesh.vertices.AddRange (new Vector3[] {
					center + new Vector3 (-halfWidth, halfHeight, -halfDepth),   
					center + new Vector3 (halfWidth, halfHeight, -halfDepth),   
					center + new Vector3 (halfWidth, -halfHeight, -halfDepth),   
					center + new Vector3 (-halfWidth, -halfHeight, -halfDepth)
				});
		
				// top
				mesh.vertices.AddRange (new Vector3[] {
					center + new Vector3 (-halfWidth, halfHeight, halfDepth),   
					center + new Vector3 (halfWidth, halfHeight, halfDepth),   
					center + new Vector3 (halfWidth, halfHeight, -halfDepth),   
					center + new Vector3 (-halfWidth, halfHeight, -halfDepth)
				});
		
				// right
				mesh.vertices.AddRange (new Vector3[] {
					center + new Vector3 (halfWidth, halfHeight, -halfDepth),    
					center + new Vector3 (halfWidth, halfHeight, halfDepth),    
					center + new Vector3 (halfWidth, -halfHeight, halfDepth),    
					center + new Vector3 (halfWidth, -halfHeight, -halfDepth)
				});
		
				// front
				mesh.vertices.AddRange (new Vector3[] {
					center + new Vector3 (halfWidth, halfHeight, halfDepth),   
					center + new Vector3 (-halfWidth, halfHeight, halfDepth),   
					center + new Vector3 (-halfWidth, -halfHeight, halfDepth),   
					center + new Vector3 (halfWidth, -halfHeight, halfDepth)
				});
		
				// bottom
				mesh.vertices.AddRange (new Vector3[] {
					center + new Vector3 (halfWidth, -halfHeight, -halfDepth),   
					center + new Vector3 (-halfWidth, -halfHeight, -halfDepth),   
					center + new Vector3 (-halfWidth, -halfHeight, halfDepth),   
					center + new Vector3 (halfWidth, -halfHeight, halfDepth)
				});
		
				// left
				mesh.vertices.AddRange (new Vector3[] {
					center + new Vector3 (-halfWidth, halfHeight, -halfDepth),   
					center + new Vector3 (-halfWidth, halfHeight, halfDepth),   
					center + new Vector3 (-halfWidth, -halfHeight, halfDepth),   
					center + new Vector3 (-halfWidth, -halfHeight, -halfDepth)
				});
		
				mesh.normals.AddRange (new Vector3[] {
					new Vector3 (0.0f, 0.0f, -1.0f),
					new Vector3 (0.0f, 0.0f, -1.0f),
					new Vector3 (0.0f, 0.0f, -1.0f),
					new Vector3 (0.0f, 0.0f, -1.0f)
				});
		
				mesh.normals.AddRange (new Vector3[] {        
					new Vector3 (0.0f, 1.0f, 0.0f),
					new Vector3 (0.0f, 1.0f, 0.0f),
					new Vector3 (0.0f, 1.0f, 0.0f),
					new Vector3 (0.0f, 1.0f, 0.0f)
				});
		
				mesh.normals.AddRange (new Vector3[] {
					new Vector3 (1.0f, 0.0f, 0.0f),
					new Vector3 (1.0f, 0.0f, 0.0f),
					new Vector3 (1.0f, 0.0f, 0.0f),
					new Vector3 (1.0f, 0.0f, 0.0f)
				});
		
				mesh.normals.AddRange (new Vector3[] {
					new Vector3 (0.0f, 0.0f, 1.0f),
					new Vector3 (0.0f, 0.0f, 1.0f),
					new Vector3 (0.0f, 0.0f, 1.0f),
					new Vector3 (0.0f, 0.0f, 1.0f)
				});
		
				mesh.normals.AddRange (new Vector3[] {
					new Vector3 (0.0f, -1.0f, 0.0f),
					new Vector3 (0.0f, -1.0f, 0.0f),
					new Vector3 (0.0f, -1.0f, 0.0f),
					new Vector3 (0.0f, -1.0f, 0.0f)
				});
		
				mesh.normals.AddRange (new Vector3[] {
					new Vector3 (-1.0f, 0.0f, 0.0f),
					new Vector3 (-1.0f, 0.0f, 0.0f),
					new Vector3 (-1.0f, 0.0f, 0.0f),
					new Vector3 (-1.0f, 0.0f, 0.0f)                        
				});

				mesh.uvs.AddRange (new Vector2[] {
					new Vector2 (uvRect [0].xMin, uvRect [0].yMin), 
					new Vector2 (uvRect [0].xMax, uvRect [0].yMin), 
					new Vector2 (uvRect [0].xMax, uvRect [0].yMax), 
					new Vector2 (uvRect [0].xMin, uvRect [0].yMax)
				});
		
				mesh.uvs.AddRange (new Vector2[] {
					new Vector2 (uvRect [1].xMin, uvRect [1].yMin), 
					new Vector2 (uvRect [1].xMax, uvRect [1].yMin), 
					new Vector2 (uvRect [1].xMax, uvRect [1].yMax), 
					new Vector2 (uvRect [1].xMin, uvRect [1].yMax), 
				});
		
				mesh.uvs.AddRange (new Vector2[] {
					new Vector2 (uvRect [2].xMin, uvRect [2].yMin), 
					new Vector2 (uvRect [2].xMax, uvRect [2].yMin), 
					new Vector2 (uvRect [2].xMax, uvRect [2].yMax), 
					new Vector2 (uvRect [2].xMin, uvRect [2].yMax)
				});
		
				mesh.uvs.AddRange (new Vector2[] {
					new Vector2 (uvRect [3].xMin, uvRect [3].yMin), 
					new Vector2 (uvRect [3].xMax, uvRect [3].yMin), 
					new Vector2 (uvRect [3].xMax, uvRect [3].yMax), 
					new Vector2 (uvRect [3].xMin, uvRect [3].yMax)
				});
		
				mesh.uvs.AddRange (new Vector2[] {
					new Vector2 (uvRect [4].xMin, uvRect [4].yMin), 
					new Vector2 (uvRect [4].xMax, uvRect [4].yMin), 
					new Vector2 (uvRect [4].xMax, uvRect [4].yMax), 
					new Vector2 (uvRect [4].xMin, uvRect [4].yMax)
				});
		
				mesh.uvs.AddRange (new Vector2[] {
					new Vector2 (uvRect [5].xMin, uvRect [5].yMin), 
					new Vector2 (uvRect [5].xMax, uvRect [5].yMin), 
					new Vector2 (uvRect [5].xMax, uvRect [5].yMax), 
					new Vector2 (uvRect [5].xMin, uvRect [5].yMax)
				});
		
				if (hasFrontFace) {
						mesh.indices.AddRange (new int[] {
							i + 12, i + 13, i + 14, i + 12, i + 14, i + 15
						});
				}
		
				if (hasTopFace) {
						mesh.indices.AddRange (new int[] {
							i + 4, i + 5, i + 6, i + 4, i + 6, i + 7
						});
				}
		
				if (hasRightFace) {
						mesh.indices.AddRange (new int[] {
							i + 8, i + 9, i + 10, i + 8, i + 10, i + 11
						});
				}
		
				if (hasBackFace) {
						mesh.indices.AddRange (new int[] {
							i, i + 1, i + 2, i, i + 2, i + 3
						});
				}
		
				if (hasBottomFace) {
						mesh.indices.AddRange (new int[] {
							i + 16, i + 18, i + 17, i + 16, i + 19, i + 18
						});
				}
		
				if (hasLeftFace) {
						mesh.indices.AddRange (new int[] {
							i + 20, i + 22, i + 21, i + 20, i + 23, i + 22
						});
				}
		}
	
		public static void CalculateTangents (Mesh mesh)
		{
				Vector4[] tangents = new Vector4 [mesh.vertices.Length];
				Vector3[] tangents1 = new Vector3 [mesh.vertices.Length];
				Vector3[] tangents2 = new Vector3 [mesh.vertices.Length];
		
				for (int i = 0; i < mesh.triangles.Length; i += 3) {
						int i1 = mesh.triangles [i];
						int i2 = mesh.triangles [i + 1];
						int i3 = mesh.triangles [i + 2];
			
						Vector3 v1 = mesh.vertices [i1];
						Vector3 v2 = mesh.vertices [i2];
						Vector3 v3 = mesh.vertices [i3];
			
						Vector2 w1 = mesh.uv [i1];
						Vector2 w2 = mesh.uv [i2];
						Vector2 w3 = mesh.uv [i3];
			
						float x1 = v2.x - v1.x;
						float x2 = v3.x - v1.x;
			
						float y1 = v2.y - v1.y;
						float y2 = v3.y - v1.y;
			
						float z1 = v2.z - v1.z;
						float z2 = v3.z - v1.z;
			
						float s1 = w2.x - w1.x;
						float s2 = w3.x - w1.x;
			
						float t1 = w2.y - w1.y;
						float t2 = w3.y - w1.y;
			
						float r = 1.0f / (s1 * t2 - s2 * t1);
			
						Vector3 sdir = new Vector3 ((t2 * x1 - t1 * x2) * r, 
			                            (t2 * y1 - t1 * y2) * r,
			                            (t2 * z1 - t1 * z2) * r);
			
						Vector3 tdir = new Vector3 ((s1 * x2 - s2 * x1) * r, 
			                            (s1 * y2 - s2 * y1) * r,
			                            (s1 * z2 - s2 * z1) * r);
			
						tangents1 [i1] += sdir;
						tangents1 [i2] += sdir;
						tangents1 [i3] += sdir;
			
						tangents2 [i1] += tdir;
						tangents2 [i2] += tdir;
						tangents2 [i3] += tdir;
				}
		
				for (int i = 0; i < mesh.vertices.Length; i++) {
						Vector3 normal = mesh.normals [i];
						Vector3 tangent = tangents1 [i];
			
						// gram-schmidt orthogonalization
						Vector3 tmp = Vector3.Normalize (tangent - normal * Vector3.Dot (normal, tangent));
			
						// calculate handedness
						float w = (Vector3.Dot (Vector3.Cross (normal, tangent), tangents2 [i]) < 0.0f) ? -1.0f : 1.0f;
			
						tangents [i] = new Vector4 (tmp.x, tmp.y, tmp.z, w);
				}
		
				mesh.tangents = tangents;
		}
}
