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

	private GameObject[] _buttons = new GameObject[VOXEL_TYPES.Length];
	
	private void Awake()
	{
		BuildButtons();

		_buttons[0] = transform.Find( "Button_00_Delete" ).gameObject;
		UIEventListener.Get( _buttons[0] ).onClick = OnClick_Delete;

		_buttons[1].GetComponent<UIToggle>().startsActive = true;

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
			_buttons[i] = bGO;
		}
	}

	private void Update()
	{
		float mouseScroll = Input.GetAxis( "Mouse ScrollWheel" );
		if( mouseScroll > 0 )
		{
			_picker.voxelId++;
			if( _picker.voxelId >= VOXEL_TYPES.Length )
				_picker.voxelId = 1;
			_buttons[_picker.voxelId].GetComponent<UIToggle>().value = true;
		}
		else if( mouseScroll < 0 )
		{
			_picker.voxelId--;
			if( _picker.voxelId < 1 )
				_picker.voxelId = VOXEL_TYPES.Length - 1;
			_buttons[_picker.voxelId].GetComponent<UIToggle>().value = true;
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
