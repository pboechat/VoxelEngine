using UnityEngine;
using System.Collections;

public class Tooltip : MonoBehaviour
{
	[SerializeField]
	private Camera _uiCamera;
	private UILabel _label;

	private static Tooltip _instance;

	private void Awake()
	{
		_instance = this;
		_label = GetComponentInChildren<UILabel>();

		Hide();
	}

	private void Update()
	{
		Vector3 mPos = Input.mousePosition;

		if( _uiCamera != null )
			transform.position = _uiCamera.ScreenToWorldPoint( Input.mousePosition );
	}

	public static void Show( string text )
	{
		_instance._label.text = text;
		_instance.gameObject.SetActive( true );
	}

	public static void Hide()
	{
		_instance.gameObject.SetActive( false );
	}

}
