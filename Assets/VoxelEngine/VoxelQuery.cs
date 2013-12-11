using UnityEngine;
using System;

public class VoxelQuery
{
	public delegate void PrepareCallback ();

	public delegate bool ExecuteCallback (VoxelChunk chunk,int voxelX,int voxelY,int voxelZ);

	public delegate void DisposeCallback ();

	private Vector3 _position;
	private int _width;
	private int _height;
	private int _depth;
	private bool[] _mask;
	private PrepareCallback _prepareCallback;
	private ExecuteCallback _executeCallback;
	private DisposeCallback _disposeCallback;

	public Vector3 position {
		get {
			return _position;
		}
		set {
			_position = value;
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

	public PrepareCallback prepareCallback {
		set {
			_prepareCallback = value;
		}
	}

	public DisposeCallback disposeCallback {
		set {
			_disposeCallback = value;
		}
	}

	public VoxelQuery (Vector3 position, int width, int height, int depth, bool[] mask, ExecuteCallback executeCallback)
	{
		if (width < 1 || height < 1 || depth < 1) {
			throw new Exception ("width < 1 || height < 1 || depth < 1");
		}

		int size = width * height * depth;
		if (mask.Length != size) {
			throw new Exception ("mask.Length != size");
		}

		if (executeCallback == null) {
			throw new Exception ("executeCallback == null");
		}

		_position = position;
		_width = width;
		_height = height;
		_depth = depth;
		_mask = mask;
		_executeCallback = executeCallback;
	}

	public VoxelQuery (int width, int height, int depth, bool[] mask, ExecuteCallback executeCallback) : this(Vector3.zero, width, height, depth, mask, executeCallback)
	{
	}

	public void Prepare ()
	{
		if (_prepareCallback != null) {
			_prepareCallback ();
		}
	}
	
	public bool Execute (VoxelChunk chunk, int x, int y, int z)
	{
		return _executeCallback (chunk, x, y, z);
	}

	public void Dispose ()
	{
		if (_disposeCallback != null) {
			_disposeCallback ();
		}
	}

}
