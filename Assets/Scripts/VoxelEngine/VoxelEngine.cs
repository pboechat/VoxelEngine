using UnityEngine;

public class VoxelEngine : Singleton<VoxelEngine>
{
		[SerializeField]
		private float
				_voxelSize;
		private float _halfVoxelSize;
		private Vector3 _up;
		private Vector3 _down;
		private Vector3 _left;
		private Vector3 _right;
		private Vector3 _back;
		private Vector3 _forward;
		
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
				get { return _voxelSize; }
		}
		
		public float halfVoxelSize {
				get { return _halfVoxelSize; }
		}
		
		public void _SetVoxelSize (float voxelSize)
		{
				_voxelSize = voxelSize;
				_halfVoxelSize = _voxelSize * 0.5f;
				_up = Vector3.up * _voxelSize;
				_down = Vector3.down * _voxelSize;
				_right = Vector3.right * _voxelSize;
				_left = Vector3.left * _voxelSize;
				_back = Vector3.back * _voxelSize;
				_forward = Vector3.forward * _voxelSize;
		}
}
