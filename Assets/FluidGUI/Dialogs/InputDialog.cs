using UnityEngine;
using System.Collections;

public class InputDialog : AbstractDialog
{
	//private string _submitString;

	public string Input
	{
		get{ return _body.transform.Find( "Input" ).GetComponent<UIInput>().value; }
	}

	public InputDialog( string title, string message, string cancelString, string submitString )
	{
		_body = LoadDialog( (GameObject)Resources.Load( "Dialog/InputDialog" ) );
		SetTitle( title );
		SetMessage( message );
		AddButtons( new string[]{ cancelString, submitString } );

		//_submitString = submitString;
	}
	
	public static implicit operator bool( InputDialog d )
	{
		if( d.Result == 1 )
			return true;

		return false;
	}
}
