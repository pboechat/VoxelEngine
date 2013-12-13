using UnityEngine;
using System;
using System.Collections.Generic;

public class VoxelEngine : Singleton<VoxelEngine>
{
	[Serializable]
	public class VoxelFaceMapping
	{
		public int voxelId;
		public int frontFaceTileId;
		public int topFaceTileId;
		public int rightFaceTileId;
		public int backFaceTileId;
		public int bottomFaceTileId;
		public int leftFaceTileId;
		public int frontFaceTileIdWithoutTopNeighbor;
		public int rightFaceTileIdWithoutTopNeighbor;
		public int backFaceTileIdWithoutTopNeighbor;
		public int leftFaceTileIdWithoutTopNeighbor;

	}

	public float voxelSize;
	public Material atlas;
	public int tileSize;
	public VoxelFaceMapping[] voxelFaceMappings;
	[SerializeField]
	private float
		_halfVoxelSize;
	[SerializeField]
	private Vector3
		_up;
	[SerializeField]
	private Vector3
		_down;
	[SerializeField]
	private Vector3
		_left;
	[SerializeField]
	private Vector3
		_right;
	[SerializeField]
	private Vector3
		_back;
	[SerializeField]
	private Vector3
		_forward;
	[SerializeField]
	private Rect[]
		_tileUvsCache;
	private Dictionary<byte, int[]>
		_voxelFaceMappingCache;
		
	public Vector3 up {
		get {
			return _up;
		}
	}
	
	public Vector3 down {
		get {
			return _down;
		}
	}
	
	public Vector3 left {
		get {
			return _left;
		}
	}
	
	public Vector3 right {
		get {
			return _right;
		}
	}
	
	public Vector3 back {
		get {
			return _back;
		}
	}
	
	public Vector3 forward {
		get {
			return _forward;
		}
	}
		
	public float halfVoxelSize {
		get { 
			return _halfVoxelSize; 
		}
	}
	
	void Awake ()
	{
		Update ();
	}

	void CalculateCachedFields ()
	{
		// cached fields
		_halfVoxelSize = voxelSize * 0.5f;
		_up = Vector3.up * voxelSize;
		_down = Vector3.down * voxelSize;
		_right = Vector3.right * voxelSize;
		_left = Vector3.left * voxelSize;
		_back = Vector3.back * voxelSize;
		_forward = Vector3.forward * voxelSize;
	}
	
	public void Update ()
	{
		CalculateCachedFields ();
		BuildTileUvsCache ();
		BuildVoxelFaceMappingCache ();
	}

	void BuildTileUvsCache ()
	{
		Texture2D atlasTexture = (Texture2D)atlas.mainTexture;
		int width = atlasTexture.width / tileSize;
		int height = atlasTexture.height / tileSize;
		Vector2 xOffset = Vector3.right * (1.0f / width);
		Vector2 yOffset = Vector3.up * (1.0f / height);
		_tileUvsCache = new Rect[width * height];
		int i = 0;
		Vector2 yStart = (height - 1) * yOffset;
		for (int y = height - 1; y >= 0; y--) {
			Vector2 xStart = Vector2.zero;
			for (int x = 0; x < width; x++) {
				Vector2 uv = xStart + yStart;
				_tileUvsCache [i++] = new Rect (uv.x, uv.y, xOffset.x, yOffset.y);
				xStart += xOffset;
			}
			yStart -= yOffset;
		}
	}

	public void BuildVoxelFaceMappingCache ()
	{
		_voxelFaceMappingCache = new Dictionary<byte, int[]> ();
		foreach (VoxelFaceMapping voxelFaceMapping in voxelFaceMappings) {
			int[] faceMapping = new int[10];
			faceMapping [0] = voxelFaceMapping.frontFaceTileId;
			faceMapping [1] = voxelFaceMapping.topFaceTileId;
			faceMapping [2] = voxelFaceMapping.rightFaceTileId;
			faceMapping [3] = voxelFaceMapping.backFaceTileId;
			faceMapping [4] = voxelFaceMapping.bottomFaceTileId;
			faceMapping [5] = voxelFaceMapping.leftFaceTileId;
			faceMapping [6] = voxelFaceMapping.frontFaceTileIdWithoutTopNeighbor;
			faceMapping [7] = voxelFaceMapping.rightFaceTileIdWithoutTopNeighbor;
			faceMapping [8] = voxelFaceMapping.backFaceTileIdWithoutTopNeighbor;
			faceMapping [9] = voxelFaceMapping.leftFaceTileIdWithoutTopNeighbor;
			byte voxelId = (byte)voxelFaceMapping.voxelId;
			if (_voxelFaceMappingCache.ContainsKey (voxelId)) {
				throw new Exception ("duplicate face mapping for voxel id: " + voxelId);
			}
			_voxelFaceMappingCache.Add (voxelId, faceMapping);
		}
	}

	public Rect GetTileUv (int tileId)
	{
		return _tileUvsCache [tileId - 1];
	}

	public void GetVoxelFaceMapping (byte voxelId, out int frontFaceTileId, out int topFaceTileId, out int rightFaceTileId, out int backFaceTileId, out int bottomFaceTileId, out int leftFaceTileId, bool hasTopNeighbor)
	{
		int[] faceMapping;
		if (!_voxelFaceMappingCache.TryGetValue (voxelId, out faceMapping)) {
			throw new Exception ("unmapped voxel id: " + voxelId);
		}
		frontFaceTileId = (hasTopNeighbor) ? faceMapping [0] : faceMapping [6];
		topFaceTileId = faceMapping [1];
		rightFaceTileId = (hasTopNeighbor) ? faceMapping [2] : faceMapping [7];
		backFaceTileId = (hasTopNeighbor) ? faceMapping [3] : faceMapping [8];
		bottomFaceTileId = faceMapping [4];
		leftFaceTileId = (hasTopNeighbor) ? faceMapping [5] : faceMapping [9];
	}

}
