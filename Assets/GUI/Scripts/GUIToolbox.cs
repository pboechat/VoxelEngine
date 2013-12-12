using UnityEngine;

public class GUIToolbox : MonoBehaviour
{
	private static readonly string[] VOXEL_TYPES = new string[]
	{
		"Delete",
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
	private MinecraftLikePlayer _picker;

	[SerializeField]
	private GameObject _prefab_ButtonAddVoxel;
	
	private void Awake()
	{
		BuildButtons();

		UIEventListener.Get( transform.Find( "Button_00_Delete" ).gameObject ).onClick = OnClick_Delete;

		GetComponent<UIGrid>().repositionNow = true;
	}
	
	private void BuildButtons()
	{
		for( int i = 1; i < VOXEL_TYPES.Length; i++ )
		{
			GameObject bGO = (GameObject)Instantiate( _prefab_ButtonAddVoxel, transform.position, transform.rotation );
			bGO.transform.parent = transform;
			bGO.transform.localScale = Vector3.one;

			bGO.name = "Button_" + i.ToString( "00" ) + "_" + VOXEL_TYPES[i];

			UIEventListener.Get( bGO ).onClick = OnClick_Add;
			bGO.transform.Find( "Image" ).GetComponent<UISprite>().spriteName = i.ToString();
			
			if( i == 1 )
				bGO.GetComponent<UIToggle>().startsActive = true;
		}
	}
	
	
	#region Button Event Handlers
	
	private void OnClick_Delete( GameObject go )
	{
		_picker.addMode = false;
	}
	
	private void OnClick_Add( GameObject go )
	{
		_picker.addMode = true;

		string[] opid = go.name.Split( '_' );
		_picker.voxelId = int.Parse( opid[1] );
	}
	
	#endregion
}
