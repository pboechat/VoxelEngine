using UnityEngine;
using System;
using System.Collections.Generic;

public class VoxelEngine : Singleton<VoxelEngine>
{
	[Serializable]
	public class VoxelIdFaceMapping
	{
		public int voxelId;
		public int frontFaceTileId;
		public int topFaceTileId;
		public int rightFaceTileId;
		public int backFaceTileId;
		public int bottomFaceTileId;
		public int leftFaceTileId;

	}

	[SerializeField]
	public Material
		atlas;
	[SerializeField]
	private int
		_tileSize;
	[SerializeField]
	private float
		_voxelSize;
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
		_tileUvs;
	[SerializeField]
	private VoxelIdFaceMapping[]
		_voxelIdsFaceMappings;
	[SerializeField]
	private Dictionary<byte, int[]>
		_voxelIdFaceMappingCache;
		
	public int tileSize {
		get {
			return _tileSize;
		}
	}
		
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
	
	public float voxelSize {
		get { 
			return _voxelSize;
		}
	}
		
	public float halfVoxelSize {
		get { 
			return _halfVoxelSize; 
		}
	}

	void Awake ()
	{
		BuildVoxelIdMappingCache ();
	}
		
	public void SetVoxelSize (float voxelSize)
	{
		// cached fields
		_voxelSize = voxelSize;
		_halfVoxelSize = _voxelSize * 0.5f;
		_up = Vector3.up * _voxelSize;
		_down = Vector3.down * _voxelSize;
		_right = Vector3.right * _voxelSize;
		_left = Vector3.left * _voxelSize;
		_back = Vector3.back * _voxelSize;
		_forward = Vector3.forward * _voxelSize;
	}
		
	public void SetTileSize (int tileSize)
	{
		_tileSize = tileSize;
				
		Texture2D atlasTexture = (Texture2D)atlas.mainTexture;
				
		if (atlasTexture.width % tileSize != 0 || atlasTexture.height % tileSize != 0) {
			Debug.LogError ("atlas dimensions must be multiple of tile size");
			enabled = false;
			return;
		}
				
		int width = atlasTexture.width / tileSize;
		int height = atlasTexture.height / tileSize;
				
		Vector2 xOffset = Vector3.right * (1.0f / width);
		Vector2 yOffset = Vector3.up * (1.0f / height);
		Vector2 vPos = (height - 1) * yOffset;
		_tileUvs = new Rect[width * height];
		int i = 0;
		for (int y = height - 1; y > 0; y--) {
			Vector2 hPos = Vector2.zero;
			for (int x = 0; x < width; x++) {
				Vector2 uv = hPos + vPos;
				_tileUvs [i++] = new Rect (uv.x, uv.y, xOffset.x, yOffset.y);
				hPos += xOffset;	
			}
			vPos -= yOffset;
		}
	}

	public void BuildVoxelIdMappingCache ()
	{
		_voxelIdFaceMappingCache = new Dictionary<byte, int[]> ();
		foreach (VoxelIdFaceMapping voxelIdFaceMapping in _voxelIdsFaceMappings) {
			int[] faceMappings = new int[6];
			faceMappings [0] = voxelIdFaceMapping.frontFaceTileId;
			faceMappings [1] = voxelIdFaceMapping.topFaceTileId;
			faceMappings [2] = voxelIdFaceMapping.rightFaceTileId;
			faceMappings [3] = voxelIdFaceMapping.backFaceTileId;
			faceMappings [4] = voxelIdFaceMapping.bottomFaceTileId;
			faceMappings [5] = voxelIdFaceMapping.leftFaceTileId;
			byte voxelId = (byte)voxelIdFaceMapping.voxelId;
			if (_voxelIdFaceMappingCache.ContainsKey (voxelId)) {
				throw new Exception ("duplicate face mapping for voxel id: " + voxelId);
			}
			_voxelIdFaceMappingCache.Add (voxelId, faceMappings);
		}
	}

	public Rect GetTileUv (int tileId)
	{
		return _tileUvs [tileId - 1];
	}

	public void GetVoxelIdFaceMapping (byte voxelId, out int frontFaceTileId, out int topFaceTileId, out int rightFaceTileId, out int backFaceTileId, out int bottomFaceTileId, out int leftFaceTileId)
	{
		int[] faceMapping;
		if (!_voxelIdFaceMappingCache.TryGetValue (voxelId, out faceMapping)) {
			throw new Exception ("unmapped voxel id: " + voxelId);
		}
		frontFaceTileId = faceMapping [0];
		topFaceTileId = faceMapping [1];
		rightFaceTileId = faceMapping [2];
		backFaceTileId = faceMapping [3];
		bottomFaceTileId = faceMapping [4];
		leftFaceTileId = faceMapping [5];
	}

}
