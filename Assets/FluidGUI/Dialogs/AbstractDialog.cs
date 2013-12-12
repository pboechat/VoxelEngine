using UnityEngine;
using System.Collections.Generic;

public class AbstractDialog
{
	protected AbstractDialog(){}

	protected GameObject _body;

	protected GameObject _pressedButton = null;
	protected Dictionary<string,GameObject> _buttons = new Dictionary<string,GameObject>();

	private int _result;
	public int Result
	{
		get{ return _result; }
	}

	public delegate void DialogClosed();
	public event DialogClosed OnDialogClosed = delegate{};
	public event DialogClosed OnDialogAccepted = delegate{};

	public void Show()
	{
		_body.SetActive( true );
	}

	public void Hide()
	{
		_body.SetActive( false );
		OnDialogClosed();

		if( Result == 1 )
			OnDialogAccepted();
	}

	public void Destroy()
	{
		GameObject.Destroy( _body );
	}

	public void Confirm()
	{
		_result = Mathf.Min( 1, _buttons.Count );
		Hide();
	}

	public void Cancel()
	{
		_result = 0;
		Hide();
	}

	protected GameObject LoadDialog( GameObject dialog )
	{
		GameObject body = (GameObject)GameObject.Instantiate( dialog );
		MonoBehaviour m = (MonoBehaviour)GameObject.FindObjectOfType( typeof( UIRoot ) );
		body.transform.parent = m.transform;
		body.transform.localScale = Vector3.one;
		
		return body;
	}
	
	protected void SetTitle( string title )
	{
		UILabel uititle = _body.transform.Find( "Title" ).Find( "_Label" ).GetComponent<UILabel>();
		uititle.text = title;
	}
	
	protected void SetMessage( string message )
	{
		UILabel uimessage = _body.transform.Find( "Message" ).Find( "_Label" ).GetComponent<UILabel>();
		uimessage.text = message;
	}
	
	protected void AddButtons( string[] buttons )
	{
		UITable uibuttons = _body.transform.Find( "Buttons" ).GetComponent<UITable>();
		foreach( string btn in buttons )
		{
			GameObject b = (GameObject)GameObject.Instantiate( (GameObject)Resources.Load( "Dialog/Button" ) );
			b.transform.Find( "_Label" ).GetComponent<UILabel>().text = btn;
			b.transform.parent = uibuttons.transform;
			b.transform.localScale = Vector3.one;
			b.GetComponent<UIButtonEvent>().Clicked += ButtonPressed;

			_buttons[btn] = b;
		}
		
		uibuttons.repositionNow = true;
	}

	private void ButtonPressed( GameObject button )
	{
		_pressedButton = button;
		
		GameObject[] btns = new GameObject[_buttons.Count];
		_buttons.Values.CopyTo( btns, 0 );
		for( int i = 0; i < btns.Length; i++ )
			if( btns[i] == button )
				_result = i;

		Hide();
	}

	public static implicit operator bool( AbstractDialog ad )
	{
		return false;
	}
	
	public static bool operator ==( AbstractDialog ad, int btn )
	{
		return ad.Result == btn;
	}
	
	public static bool operator !=( AbstractDialog ad, int btn )
	{
		return ad.Result != btn;
	}
}
