using UnityEngine;

//using UnityEditor;
using System;

[RequireComponent(typeof(CharacterController))]
public class CatapultController : MonoBehaviour
{
	[SerializeField]
	private float
		_panSpeed;
	[SerializeField]
	private float
		_zoomSpeed;
	[SerializeField]
	private float
		_minHeight;
	[SerializeField]
	private float
		_maxHeight;
	[SerializeField]
	private int
		_commandButton;
	[SerializeField]
	private int
		_panButton;
	[SerializeField]
	private VoxelTerrain
		_terrain;
	[SerializeField]
	private ParticleEmitter
		_smokePrefab;
	[SerializeField]
	private VoxelRemover
		_explosionQueryPrefab;
	[SerializeField]
	private Boulder
		_boulderPrefab;
	[SerializeField]
	private NavMeshAgent
		_catapult;
	[SerializeField]
	private float
		_catapultStrength;
	[SerializeField]
	private Vector3
		_catapultLaunchOffset;
	//private bool _initialized = false;
	private Camera _camera;
	private Vector3 _lastMousePosition;
	private CharacterController _controller;
  
	void Awake ()
	{
		_controller = GetComponent<CharacterController> ();
		_camera = GetComponentInChildren<Camera> ();
		_explosionQueryPrefab.terrain = _terrain;
	}

	void TryToMove (Vector3 move)
	{
		Vector3 position = transform.position;
		if (_controller.Move (move) != CollisionFlags.None) {
			transform.position = position;
		}
	}
  
	void Shoot ()
	{
		Boulder boulder = (Boulder)Instantiate (_boulderPrefab, _catapult.transform.position + _catapultLaunchOffset, _catapult.transform.rotation);
		boulder.Launch (_catapultStrength, _explosionQueryPrefab, _smokePrefab);
	}
  
	void Update ()
	{
		/*if (!_initialized) {
			int terrainLayer = LayerMask.NameToLayer ("Terrain");
			_terrain.gameObject.layer = terrainLayer;
			_terrain.gameObject.isStatic = true;
			MeshFilter[] meshFilters = _terrain.GetComponentsInChildren<MeshFilter> ();
			foreach (MeshFilter meshFilter in meshFilters) {
				GameObject childGameObject = meshFilter.gameObject;
				childGameObject.isStatic = true;
				childGameObject.layer = terrainLayer;
			}
			NavMeshBuilder.ClearAllNavMeshes ();
			NavMeshBuilder.BuildNavMesh ();
			_catapult.enabled = true;
			_initialized = true;
		}*/
  
		Vector3 mousePosition = Input.mousePosition;
		if (Input.GetKeyDown (KeyCode.Space)) {
			Shoot ();
		} else if (Input.GetMouseButtonDown (_commandButton)) {
			Ray ray = _camera.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit, float.MaxValue)) {
				_catapult.destination = hit.point;
			}
		} else if (Input.GetMouseButtonDown (_panButton)) {
			_lastMousePosition = mousePosition;
		} else if (Input.GetMouseButton (_panButton)) {
			if (_lastMousePosition == mousePosition) {
				return;
			}
			Vector3 pan = (mousePosition - _lastMousePosition) * _panSpeed * Time.deltaTime * -1;
			Vector3 move = transform.TransformDirection (new Vector3 (pan.x, 0, 0)) + new Vector3 (0, 0, pan.y);
			TryToMove (move);
			_lastMousePosition = mousePosition;
		} 
    
		float scrollWheel = Input.GetAxis ("Mouse ScrollWheel");
		if (scrollWheel != 0) {
			float verticalMove = scrollWheel * 10 * _zoomSpeed * Time.deltaTime * -1;
			if (verticalMove > 0 && transform.position.y + verticalMove >= _maxHeight) {
				verticalMove = _maxHeight - transform.position.y;
			} else if (verticalMove < 0 && transform.position.y + verticalMove <= _minHeight) {
				verticalMove = _minHeight - transform.position.y;
			}
			Vector3 move = transform.TransformDirection (new Vector3 (0, verticalMove, 0));
			TryToMove (move);
		}
	}
  
}