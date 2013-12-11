using UnityEngine;
using System.Collections.Generic;

public class VoxelRemover : MonoBehaviour
{
	private struct VoxelPosition
	{
		public int x;
		public int y;
		public int z;

		public VoxelPosition (int x, int y, int z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}
	}

	[SerializeField]
	public int
		queryWidth;
	[SerializeField]
	public int
		queryHeight;
	[SerializeField]
	public int
		queryDepth;
	[SerializeField]
	public bool[]
		queryMask;
	[SerializeField]
	public VoxelTerrain
		terrain;
	[SerializeField]
	private GameObject
		area = null;
	private VoxelQuery query;
	private Dictionary<VoxelChunk, List<VoxelPosition>> queryResults;
	private static Material voxelRemoverMaterial = null;
		
	Material GetVoxelRemoverMaterial ()
	{
		if (voxelRemoverMaterial == null) {
			voxelRemoverMaterial = new Material (Shader.Find ("Diffuse"));
		}
		return voxelRemoverMaterial;
	}

	void Start ()
	{
		BuildQuery ();
	}
	
	public void DisplayArea ()
	{
		if (area != null) {
			DestroyImmediate (area);
		}
		
		area = new GameObject ("Area");
		area.hideFlags = HideFlags.HideInHierarchy;
		area.transform.parent = transform;
		area.transform.localPosition = Vector3.zero;
		area.transform.localRotation = Quaternion.identity;
		
		int numVoxels = queryMask.Length;
		if (numVoxels > 1820) {
			Debug.LogError ("number of voxels cannot be > 1820");
			enabled = false;
			return;
		}
		
		ProceduralMesh mesh = new ProceduralMesh (numVoxels * 24, numVoxels * 36);
		float yOffset = (-queryHeight * 0.5f) * VoxelEngine.instance.voxelSize + VoxelEngine.instance.halfVoxelSize;
		float zOffset = (-queryDepth * 0.5f) * VoxelEngine.instance.voxelSize + VoxelEngine.instance.halfVoxelSize;
		float xOffset = (-queryWidth * 0.5f) * VoxelEngine.instance.voxelSize + VoxelEngine.instance.halfVoxelSize;
		Vector3 yStart = new Vector3 (0.0f, yOffset, 0.0f);
		int queryDepth_x_queryWidth = queryDepth * queryWidth;
		for (int y = 0; y < queryHeight; y++) {
			int y1 = y * queryDepth_x_queryWidth;
			Vector3 zStart = new Vector3 (0.0f, 0.0f, zOffset);
			for (int z = 0; z < queryDepth; z++) {
				int z1 = z * queryWidth;
				Vector3 xStart = new Vector3 (xOffset, 0.0f, 0.0f);
				for (int x = 0; x < queryWidth; x++) {
					Vector3 center = yStart + zStart + xStart;
					if (queryMask [y1 + z1 + x]) {
						ProceduralMeshes.CreateCube (mesh, VoxelEngine.instance.voxelSize, VoxelEngine.instance.voxelSize, VoxelEngine.instance.voxelSize, center);
					}
					xStart += VoxelEngine.instance.right;
				}
				zStart += VoxelEngine.instance.forward;
			}
			yStart += VoxelEngine.instance.up;
		}
		Mesh areaMesh = mesh.Build ();
		
		MeshFilter meshFilter;
		if ((meshFilter = area.GetComponent<MeshFilter> ()) == null) {
			meshFilter = area.AddComponent<MeshFilter> ();
		}
		meshFilter.sharedMesh = areaMesh;
		
		MeshRenderer meshRenderer;
		if ((meshRenderer = area.GetComponent<MeshRenderer> ()) == null) {
			meshRenderer = area.AddComponent<MeshRenderer> ();
		}
		meshRenderer.sharedMaterial = GetVoxelRemoverMaterial ();
	}

	void BuildQuery ()
	{
		query = new VoxelQuery (transform.position, queryWidth, queryHeight, queryDepth, queryMask, ExecuteQueryCallback);
		query.prepareCallback = PrepareQueryCallback;
		query.disposeCallback = DisposeQueryCallback;
	}
	
	public void Execute ()
	{
		if (terrain == null) {
			Debug.LogError ("terrain == null");
			return;
		}
	
		BuildQuery ();
		terrain.ExecuteQuery (query);
	}

	void PrepareQueryCallback ()
	{
		queryResults = new Dictionary<VoxelChunk, List<VoxelPosition>> ();
	}
	
	bool ExecuteQueryCallback (VoxelChunk chunk, int voxelX, int voxelY, int voxelZ)
	{
		// DEBUG:
		//Debug.Log ("- Removing voxel [chunk=(" + chunk.x + ", " + chunk.y + ", " + chunk.z + "), voxel=(" + voxelX + ", " + voxelY + ", " + voxelZ + ")]");
		List<VoxelPosition> voxelPositions;
		if (!queryResults.TryGetValue (chunk, out voxelPositions)) {
			voxelPositions = new List<VoxelPosition> ();
			queryResults.Add (chunk, voxelPositions);
		}
		voxelPositions.Add (new VoxelPosition (voxelX, voxelY, voxelZ));
		return true;
	}

	void DisposeQueryCallback ()
	{
		foreach (VoxelChunk chunk in queryResults.Keys) {
			chunk.StartBatchMode ();
		}

		foreach (KeyValuePair<VoxelChunk, List<VoxelPosition>> queryResult in queryResults) {
			VoxelChunk chunk = queryResult.Key;
			foreach (VoxelPosition voxelPosition in queryResult.Value) {
				chunk.RemoveVoxel (voxelPosition.x, voxelPosition.y, voxelPosition.z);
			}
		}

		foreach (VoxelChunk chunk in queryResults.Keys) {
			chunk.EndBatchMode ();
		}

		queryResults.Clear ();
		queryResults = null;
	}

}
