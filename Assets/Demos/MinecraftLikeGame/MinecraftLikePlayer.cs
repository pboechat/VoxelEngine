using UnityEngine;

public class MinecraftLikePlayer : MonoBehaviour
{
	private static readonly string[] VOXEL_TYPES = new string[] {
		"Grass",
		"Stone",
		"Gravel",
		"Sand",
		"Snow",
		"Wood",
		"Wooden Plank",
		"Coal",
		"Iron",
		"Gold",
		"Diamond",
		"Cobblestone",
		"Stone Brick",
		"Brick",
		"Glass",
		"Furnace",
		"Toolbox"
	};
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
	private string voxelTypesLabel;

	void Awake ()
	{
		voxelIdStr = voxelId + "";
		
		voxelTypesLabel = "Voxel Types\n\n";
		int c = 1;
		foreach (string voxelType in VOXEL_TYPES) {
			voxelTypesLabel += "-" + voxelType + "=" + (c++) + "\n";
		}
	}

	void OnGUI ()
	{
		GUILayout.Window (0, new Rect (10, 10, 100, 100), OnDrawControlsWindow, "Controls");
		GUILayout.Window (1, new Rect (10, 120, 120, 200), OnDrawHintsWindow, "Hints");
	}

	void OnDrawControlsWindow (int windowId)
	{
		addMode = GUILayout.Toggle (addMode, "Add Mode");
		GUILayout.BeginHorizontal ();
		GUILayout.Label ("Voxel Id");
		voxelIdStr = GUILayout.TextField (voxelIdStr);
		GUILayout.EndHorizontal ();
		if (GUILayout.Button ("Select")) {
			if (!int.TryParse (voxelIdStr, out voxelId) || voxelId < 1 || voxelId > VOXEL_TYPES.Length) {
				voxelId = 1;
			}
		}
	}

	void OnDrawHintsWindow (int windowId)
	{
		GUILayout.Label (voxelTypesLabel);
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
