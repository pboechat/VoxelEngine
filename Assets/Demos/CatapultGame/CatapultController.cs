using UnityEngine;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class CatapultController : MonoBehaviour
{
	private static readonly bool[] EXPLOSION_QUERY_MASK = new bool[] { 
		false, false, false, false, false, false, false, 
		false, false, false, false, false, false, false, 
		false, false, false, false, false, false, false, 
		false, false, false, true, false, false, false, 
		false, false, false, false, false, false, false, 
		false, false, false, false, false, false, false, 
		false, false, false, false, false, false, false, 
		
		false, false, false, false, false, false, false, 
		false, false, false, false, false, false, false, 
		false, false, false, true, false, false, false, 
		false, false, true, true, true, false, false, 
		false, false, false, true, false, false, false, 
		false, false, false, false, false, false, false, 
		false, false, false, false, false, false, false, 
		
		false, false, false, false, false, false, false, 
		false, false, false, true, false, false, false, 
		false, false, true, true, true, false, false, 
		false, true, true, true, true, true, false, 
		false, false, true, true, true, false, false, 
		false, false, false, true, false, false, false, 
		false, false, false, false, false, false, false, 
		
		false, false, false, true, false, false, false, 
		false, false, true, true, true, false, false, 
		false, true, true, true, true, true, false, 
		true, true, true, true, true, true, true, 
		false, true, true, true, true, true, false, 
		false, false, true, true, true, false, false, 
		false, false, false, true, false, false, false, 
		
		false, false, false, false, false, false, false, 
		false, false, false, true, false, false, false, 
		false, false, true, true, true, false, false, 
		false, true, true, true, true, true, false, 
		false, false, true, true, true, false, false, 
		false, false, false, true, false, false, false, 
		false, false, false, false, false, false, false, 
		
		false, false, false, false, false, false, false, 
		false, false, false, false, false, false, false, 
		false, false, false, true, false, false, false, 
		false, false, true, true, true, false, false, 
		false, false, false, true, false, false, false, 
		false, false, false, false, false, false, false, 
		false, false, false, false, false, false, false, 
		
		false, false, false, false, false, false, false, 
		false, false, false, false, false, false, false, 
		false, false, false, false, false, false, false, 
		false, false, false, true, false, false, false, 
		false, false, false, false, false, false, false, 
		false, false, false, false, false, false, false, 
		false, false, false, false, false, false, false
		
	};

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
	private float
		panSpeed;
	[SerializeField]
	private float
		zoomSpeed;
	[SerializeField]
	private float
		minHeight;
	[SerializeField]
	private float
		maxHeight;
	[SerializeField]
	private int
		catapultButton;
	[SerializeField]
	private int
		cameraButton;
	[SerializeField]
	private VoxelTerrain
		terrain;
	[SerializeField]
	private ParticleEmitter
		smokePrefab;
	[SerializeField]
	private Boulder
		boulderPrefab;
	[SerializeField]
	private NavMeshAgent
		catapult;
	[SerializeField]
	private float
		catapultStrength;
	[SerializeField]
	private Vector3
		boulderLaunchOffset;
	/*[SerializeField]
	private int
		explosionSize;*/
	private VoxelQuery explosionQuery;
	private Dictionary<VoxelChunk, List<VoxelPosition>> queryResults;
	new private Camera camera;
	private Vector3 lastMousePosition;
	private CharacterController controller;
  
	void Start ()
	{
		controller = GetComponent<CharacterController> ();
		camera = GetComponentInChildren<Camera> ();

		if (controller == null) {
			Debug.LogError ("controller == null");
			enabled = false;
			return;
		}

		if (camera == null) {
			Debug.LogError ("camera == null");
			enabled = false;
			return;
		}

		if (catapult == null) {
			Debug.LogError ("catapult == null");
			enabled = false;
			return;
		}

		if (boulderPrefab == null) {
			Debug.LogError ("boulderPrefab == null");
			enabled = false;
			return;
		}

		if (smokePrefab == null) {
			Debug.LogError ("smokePrefab == null");
			enabled = false;
			return;
		}

		if (cameraButton == catapultButton) {
			Debug.LogError ("panButton == commandButton");
			enabled = false;
			return;
		}

		if (catapultStrength <= 0) {
			Debug.LogError ("catapultStrength <= 0");
			enabled = false;
			return;
		}

		/*if (explosionSize <= 0) {
			Debug.LogError ("explosionSize <= 0");
			enabled = false;
			return;
		}*/

		BuildExplosionQuery ();
	}

	void BuildExplosionQuery ()
	{
		/*int halfExplosionSize = (explosionSize + 1) / 2;
		bool[] explosionMask = new bool[explosionSize * explosionSize * explosionSize];
		for (int y = 0; y < halfExplosionSize; y++) {
			for (int z = 0; z < halfExplosionSize; z++) {
				for (int x = 0; x < explosionSize; x++) {

				}
			}
		}
		explosionQuery = new VoxelQuery (explosionSize, explosionSize, explosionSize, explosionMask, OnQueryExecute);*/
		explosionQuery = new VoxelQuery (7, 7, 7, EXPLOSION_QUERY_MASK, OnQueryExecute);
		explosionQuery.prepareCallback = OnQueryPrepare;
		explosionQuery.disposeCallback = OnQueryDispose;
	}

	void TryToMove (Vector3 move)
	{
		Vector3 position = transform.position;
		if (controller.Move (move) != CollisionFlags.None) {
			transform.position = position;
		}
	}
  
	void Shoot ()
	{
		Boulder boulder = (Boulder)Instantiate (boulderPrefab, catapult.transform.position + boulderLaunchOffset, catapult.transform.rotation);
		boulder.Launch (catapultStrength, OnBoulderCollide);
	}
  
	void Update ()
	{
		Vector3 mousePosition = Input.mousePosition;

		// catapult shot
		if (Input.GetKeyDown (KeyCode.Space)) {
			Shoot ();
		} 

		// catapult pathfinding
		if (Input.GetMouseButtonDown (catapultButton)) {
			Ray ray = camera.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit, float.MaxValue)) {
				catapult.destination = hit.point;
			}
		} 

		// camera pan
		if (Input.GetMouseButtonDown (cameraButton)) {
			lastMousePosition = mousePosition;
		} else if (Input.GetMouseButton (cameraButton)) {
			if (lastMousePosition == mousePosition) {
				return;
			}
			Vector3 pan = (mousePosition - lastMousePosition) * panSpeed * Time.deltaTime * -1;
			Vector3 move = transform.TransformDirection (new Vector3 (pan.x, 0, 0)) + new Vector3 (0, 0, pan.y);
			TryToMove (move);
			lastMousePosition = mousePosition;
		} 
    
		// camera zoom
		float scrollWheel = Input.GetAxis ("Mouse ScrollWheel");
		if (scrollWheel != 0) {
			float verticalMove = scrollWheel * 10 * zoomSpeed * Time.deltaTime * -1;
			if (verticalMove > 0 && transform.position.y + verticalMove >= maxHeight) {
				verticalMove = maxHeight - transform.position.y;
			} else if (verticalMove < 0 && transform.position.y + verticalMove <= minHeight) {
				verticalMove = minHeight - transform.position.y;
			}
			Vector3 move = transform.TransformDirection (new Vector3 (0, verticalMove, 0));
			TryToMove (move);
		}
	}

	void OnQueryPrepare ()
	{
		queryResults = new Dictionary<VoxelChunk, List<VoxelPosition>> ();
	}

	bool OnQueryExecute (VoxelChunk chunk, int voxelX, int voxelY, int voxelZ)
	{
		List<VoxelPosition> voxelPositions;
		if (!queryResults.TryGetValue (chunk, out voxelPositions)) {
			voxelPositions = new List<VoxelPosition> ();
			queryResults.Add (chunk, voxelPositions);
		}
		voxelPositions.Add (new VoxelPosition (voxelX, voxelY, voxelZ));
		return true;
	}

	void OnQueryDispose ()
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

	void OnBoulderCollide (Vector3 point)
	{
		explosionQuery.position = point;
		terrain.ExecuteQuery (explosionQuery);

		Instantiate (smokePrefab, point, Quaternion.identity);
	}
  
}