using UnityEngine;
using System.Collections.Generic;

public class ProceduralMesh
{
		private Vector3[] _vertices;
		private Vector3[] _normals;
		private Vector2[] _uvs;
		private int[] _indices;
		private int _numVertices;
		private int _numIndices;
		private int _lastVertexIndex;
		private int _lastNormalIndex;
		private int _lastUvIndex;
		private int _lastIndexIndex;
		
		public int numVertices {
				get {
						return _numVertices;
				}
		}
		
		public int numIndices {
				get {
						return _numIndices;
				}
		}
		
		public int vertexCount {
				get {
						return _lastVertexIndex;
				}
		}
		
		public ProceduralMesh (int numVertices, int numIndices)
		{
				_numVertices = numVertices;
				_numIndices = numIndices;
				_vertices = new Vector3[_numVertices];
				_normals = new Vector3[_numVertices];
				_uvs = new Vector2[_numVertices];
				_indices = new int[_numIndices];
				_lastVertexIndex = 0;
				_lastNormalIndex = 0;
				_lastUvIndex = 0;
				_lastIndexIndex = 0;
		}
		
		public void AddVertex (Vector3 vertex)
		{
				_vertices [_lastVertexIndex++] = vertex;
		}
		
		public void AddNormal (Vector3 normal)
		{
				_normals [_lastNormalIndex++] = normal;
		}
	
		public void AddUv (Vector2 uv)
		{
				_uvs [_lastUvIndex++] = uv;
		}
	
		public void AddIndex (int index)
		{
				_indices [_lastIndexIndex++] = index;
		}
	
		public Mesh Build ()
		{
				Mesh mesh = new Mesh ();
	
				mesh.vertices = _vertices;
				mesh.normals = _normals;
				mesh.uv = _uvs;
				mesh.triangles = _indices;
				mesh.RecalculateBounds ();
		
				return mesh;
		}
	
}
