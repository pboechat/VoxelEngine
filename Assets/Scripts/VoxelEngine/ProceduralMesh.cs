using UnityEngine;
using System.Collections.Generic;

public class ProceduralMesh
{
		public List<Vector3> vertices = new List<Vector3> ();
		public List<Vector3> normals = new List<Vector3> ();
		public List<Vector2> uvs = new List<Vector2> ();
		public List<int> indices = new List<int> ();
	
		public Mesh Build ()
		{
				Mesh mesh = new Mesh ();
	
				mesh.vertices = vertices.ToArray ();
				mesh.normals = normals.ToArray ();
				mesh.uv = uvs.ToArray ();
				mesh.triangles = indices.ToArray ();
				ProceduralMeshes.CalculateTangents (mesh);
				mesh.RecalculateBounds ();
		
				return mesh;
		}
	
}
