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

	void OnGUI ()
	{
		addMode = GUILayout.Toggle (addMode, "Add Mode");
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
					chunk.AddVoxel (1, hitInfo.point);
				} else {
					chunk.RemoveVoxel (hitInfo.point);
				}
			}
		}
	}

}
