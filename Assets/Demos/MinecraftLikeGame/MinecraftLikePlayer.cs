using UnityEngine;

public class MinecraftLikePlayer : MonoBehaviour
{
	[SerializeField]
	public int
		pickingButton;
	[SerializeField]
	public float
		pickingDistance;
	[SerializeField]
	new public Camera
		camera;
	[SerializeField]
	public bool
		addMode;
	[SerializeField]
	public int
		voxelId;
	private string voxelIdStr;

	void Awake ()
	{
		voxelIdStr = voxelId + "";
	}

	void OnGUI ()
	{
		GUILayout.Window(0, new Rect(10, 10, 100, 100), OnDrawControlsWindow, "Controls");
		GUILayout.Window(1, new Rect(10, 120, 120, 200), OnDrawHintsWindow, "Hints");
	}

	void OnDrawControlsWindow(int windowId)
	{
		addMode = GUILayout.Toggle (addMode, "Add Mode");
		GUILayout.BeginHorizontal ();
		GUILayout.Label ("Voxel Id");
		voxelIdStr = GUILayout.TextField (voxelIdStr);
		GUILayout.EndHorizontal ();
		if (GUILayout.Button ("Select")) {
			if (!int.TryParse (voxelIdStr, out voxelId) || voxelId < 1 || voxelId > 13) {
				voxelId = 1;
			}
		}
	}

	void OnDrawHintsWindow(int windowId)
	{
		GUILayout.Label("Voxel Id List:\n\n-Grass=1\n-Stone=2\n-Wood=3\n-Sand=4\n-Snow=5\n-Coal=6\n-Iron=7\n-Gold=8\n-Diamond=9\n-Cobblestone=10\n-Brick=11\n-Wooden Plank=12\n-Lava=13");
	}

	void Update ()
	{
		if (camera == null) {
			return;
		}
		
		if (Input.GetMouseButtonDown (pickingButton)) {
			Ray pickingRay = camera.ScreenPointToRay (Input.mousePosition);
			RaycastHit hitInfo;
			if (Physics.Raycast (pickingRay, out hitInfo, pickingDistance)) {
				VoxelChunk chunk = hitInfo.collider.GetComponent<VoxelChunk> ();
				if (chunk == null) {
					return;
				}
				if (addMode) {
					chunk.AddVoxel ((byte)voxelId, hitInfo.point);
				} else {
					chunk.RemoveVoxel (hitInfo.point);
				}
			}
		}
	}

}
