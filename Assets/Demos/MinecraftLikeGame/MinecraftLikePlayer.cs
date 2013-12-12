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

	private void OnClick()
	{
		Ray pickingRay = camera.ScreenPointToRay( Input.mousePosition );
		RaycastHit hitInfo;
		if( Physics.Raycast( pickingRay, out hitInfo, pickingDistance ) )
		{
			VoxelChunk chunk = hitInfo.collider.GetComponent<VoxelChunk>();
			if( chunk == null )
				return;

			if( addMode )
				chunk.AddVoxel( (byte)voxelId, hitInfo.point );
			else
				chunk.RemoveVoxel( hitInfo.point );
		}
	}
}
