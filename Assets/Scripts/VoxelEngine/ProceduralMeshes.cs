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
		
				int i = mesh.vertexCount;
		
				// back
				mesh.AddVertex (center + new Vector3 (-halfWidth, halfHeight, -halfDepth)); 
				mesh.AddVertex (center + new Vector3 (halfWidth, halfHeight, -halfDepth));
				mesh.AddVertex (center + new Vector3 (halfWidth, -halfHeight, -halfDepth));
				mesh.AddVertex (center + new Vector3 (-halfWidth, -halfHeight, -halfDepth));
		
				// top
				mesh.AddVertex (center + new Vector3 (-halfWidth, halfHeight, halfDepth));
				mesh.AddVertex (center + new Vector3 (halfWidth, halfHeight, halfDepth)); 
				mesh.AddVertex (center + new Vector3 (halfWidth, halfHeight, -halfDepth));   
				mesh.AddVertex (center + new Vector3 (-halfWidth, halfHeight, -halfDepth));
		
				// right
				mesh.AddVertex (center + new Vector3 (halfWidth, halfHeight, -halfDepth));
				mesh.AddVertex (center + new Vector3 (halfWidth, halfHeight, halfDepth));
				mesh.AddVertex (center + new Vector3 (halfWidth, -halfHeight, halfDepth));
				mesh.AddVertex (center + new Vector3 (halfWidth, -halfHeight, -halfDepth));
		
				// front
				mesh.AddVertex (center + new Vector3 (halfWidth, halfHeight, halfDepth));
				mesh.AddVertex (center + new Vector3 (-halfWidth, halfHeight, halfDepth));
				mesh.AddVertex (center + new Vector3 (-halfWidth, -halfHeight, halfDepth));
				mesh.AddVertex (center + new Vector3 (halfWidth, -halfHeight, halfDepth));
		
				// bottom
				mesh.AddVertex (center + new Vector3 (halfWidth, -halfHeight, -halfDepth));
				mesh.AddVertex (center + new Vector3 (-halfWidth, -halfHeight, -halfDepth));
				mesh.AddVertex (center + new Vector3 (-halfWidth, -halfHeight, halfDepth));
				mesh.AddVertex (center + new Vector3 (halfWidth, -halfHeight, halfDepth));
		
				// left
				mesh.AddVertex (center + new Vector3 (-halfWidth, halfHeight, -halfDepth));
				mesh.AddVertex (center + new Vector3 (-halfWidth, halfHeight, halfDepth));
				mesh.AddVertex (center + new Vector3 (-halfWidth, -halfHeight, halfDepth)); 
				mesh.AddVertex (center + new Vector3 (-halfWidth, -halfHeight, -halfDepth));
		
				mesh.AddNormal (new Vector3 (0.0f, 0.0f, -1.0f));
				mesh.AddNormal (new Vector3 (0.0f, 0.0f, -1.0f));
				mesh.AddNormal (new Vector3 (0.0f, 0.0f, -1.0f));
				mesh.AddNormal (new Vector3 (0.0f, 0.0f, -1.0f));
		
				mesh.AddNormal (new Vector3 (0.0f, 1.0f, 0.0f));
				mesh.AddNormal (new Vector3 (0.0f, 1.0f, 0.0f));
				mesh.AddNormal (new Vector3 (0.0f, 1.0f, 0.0f));
				mesh.AddNormal (new Vector3 (0.0f, 1.0f, 0.0f));
		
				mesh.AddNormal (new Vector3 (1.0f, 0.0f, 0.0f));
				mesh.AddNormal (new Vector3 (1.0f, 0.0f, 0.0f));
				mesh.AddNormal (new Vector3 (1.0f, 0.0f, 0.0f));
				mesh.AddNormal (new Vector3 (1.0f, 0.0f, 0.0f));
		
				mesh.AddNormal (new Vector3 (0.0f, 0.0f, 1.0f));
				mesh.AddNormal (new Vector3 (0.0f, 0.0f, 1.0f));
				mesh.AddNormal (new Vector3 (0.0f, 0.0f, 1.0f));
				mesh.AddNormal (new Vector3 (0.0f, 0.0f, 1.0f));
		
				mesh.AddNormal (new Vector3 (0.0f, -1.0f, 0.0f));
				mesh.AddNormal (new Vector3 (0.0f, -1.0f, 0.0f));
				mesh.AddNormal (new Vector3 (0.0f, -1.0f, 0.0f));
				mesh.AddNormal (new Vector3 (0.0f, -1.0f, 0.0f));
		
				mesh.AddNormal (new Vector3 (-1.0f, 0.0f, 0.0f));
				mesh.AddNormal (new Vector3 (-1.0f, 0.0f, 0.0f));
				mesh.AddNormal (new Vector3 (-1.0f, 0.0f, 0.0f));
				mesh.AddNormal (new Vector3 (-1.0f, 0.0f, 0.0f));

				mesh.AddUv (new Vector2 (uvRect [0].xMin, uvRect [0].yMin));
				mesh.AddUv (new Vector2 (uvRect [0].xMax, uvRect [0].yMin));
				mesh.AddUv (new Vector2 (uvRect [0].xMax, uvRect [0].yMax));
				mesh.AddUv (new Vector2 (uvRect [0].xMin, uvRect [0].yMax));
				
				mesh.AddUv (new Vector2 (uvRect [1].xMin, uvRect [1].yMin));
				mesh.AddUv (new Vector2 (uvRect [1].xMax, uvRect [1].yMin));
				mesh.AddUv (new Vector2 (uvRect [1].xMax, uvRect [1].yMax));
				mesh.AddUv (new Vector2 (uvRect [1].xMin, uvRect [1].yMax));
				
				mesh.AddUv (new Vector2 (uvRect [2].xMin, uvRect [2].yMin));
				mesh.AddUv (new Vector2 (uvRect [2].xMax, uvRect [2].yMin));
				mesh.AddUv (new Vector2 (uvRect [2].xMax, uvRect [2].yMax)); 
				mesh.AddUv (new Vector2 (uvRect [2].xMin, uvRect [2].yMax));
				
				mesh.AddUv (new Vector2 (uvRect [3].xMin, uvRect [3].yMin));
				mesh.AddUv (new Vector2 (uvRect [3].xMax, uvRect [3].yMin));
				mesh.AddUv (new Vector2 (uvRect [3].xMax, uvRect [3].yMax));
				mesh.AddUv (new Vector2 (uvRect [3].xMin, uvRect [3].yMax));
				
				mesh.AddUv (new Vector2 (uvRect [4].xMin, uvRect [4].yMin));
				mesh.AddUv (new Vector2 (uvRect [4].xMax, uvRect [4].yMin));
				mesh.AddUv (new Vector2 (uvRect [4].xMax, uvRect [4].yMax));
				mesh.AddUv (new Vector2 (uvRect [4].xMin, uvRect [4].yMax));
				
				mesh.AddUv (new Vector2 (uvRect [5].xMin, uvRect [5].yMin));
				mesh.AddUv (new Vector2 (uvRect [5].xMax, uvRect [5].yMin));
				mesh.AddUv (new Vector2 (uvRect [5].xMax, uvRect [5].yMax)); 
				mesh.AddUv (new Vector2 (uvRect [5].xMin, uvRect [5].yMax));
				
				if (hasFrontFace) {
						mesh.AddIndex (i + 12);
						mesh.AddIndex (i + 13);
						mesh.AddIndex (i + 14);
						mesh.AddIndex (i + 12);
						mesh.AddIndex (i + 14);
						mesh.AddIndex (i + 15);
				}
		
				if (hasTopFace) {
						mesh.AddIndex (i + 4);
						mesh.AddIndex (i + 5);
						mesh.AddIndex (i + 6);
						mesh.AddIndex (i + 4);
						mesh.AddIndex (i + 6);
						mesh.AddIndex (i + 7);
				}
		
				if (hasRightFace) {
						mesh.AddIndex (i + 8);
						mesh.AddIndex (i + 9);
						mesh.AddIndex (i + 10);
						mesh.AddIndex (i + 8);
						mesh.AddIndex (i + 10);
						mesh.AddIndex (i + 11);
				}
		
				if (hasBackFace) {
						mesh.AddIndex (i);
						mesh.AddIndex (i + 1);
						mesh.AddIndex (i + 2);
						mesh.AddIndex (i);
						mesh.AddIndex (i + 2);
						mesh.AddIndex (i + 3);
				}
		
				if (hasBottomFace) {
						mesh.AddIndex (i + 16); 
						mesh.AddIndex (i + 18); 
						mesh.AddIndex (i + 17); 
						mesh.AddIndex (i + 16); 
						mesh.AddIndex (i + 19); 
						mesh.AddIndex (i + 18);
				}
		
				if (hasLeftFace) {
						mesh.AddIndex (i + 20); 
						mesh.AddIndex (i + 22); 
						mesh.AddIndex (i + 21); 
						mesh.AddIndex (i + 20); 
						mesh.AddIndex (i + 23); 
						mesh.AddIndex (i + 22);
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
