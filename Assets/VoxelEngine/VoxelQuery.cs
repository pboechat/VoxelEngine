using UnityEngine;

public class VoxelQuery
{
	public delegate bool Callback(VoxelChunk chunk, int voxelX, int voxelY, int voxelZ);

	private Vector3 _position;
	private int _width;
	private int _height;
	private int _depth;
	private bool[] _mask;
	private Callback _callback;

	public Vector3 position {
		get {
			return _position;
		}
	}
	
	public int width {
		get {
			return _width;
		}
	}
	
	public int height {
		get {
			return _height;
		}
	}
	
	public int depth {
		get {
			return _depth;
		}
	}
	
	public bool[] mask {
		get {
			return _mask;
		}
	}

	public VoxelQuery (Vector3 position, int width, int height, int depth, bool[] mask, Callback callback)
	{
		_position = position;
		_width = width;
		_height = height;
		_depth = depth;
		_mask = mask;
		_callback = callback;
	}
	
	public bool Execute(VoxelChunk chunk, int voxelX, int voxelY, int voxelZ)
	{
		return _callback(chunk, voxelX, voxelY, voxelZ);
	}

}
