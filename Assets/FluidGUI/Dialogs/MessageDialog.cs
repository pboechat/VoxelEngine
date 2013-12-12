using UnityEngine;
using System.Collections;

public class MessageDialog : AbstractDialog
{
	public MessageDialog( string title, string message, string[] buttons )
	{
		_body = LoadDialog( (GameObject)Resources.Load( "Dialog/MessageDialog" ) );
		SetTitle( title );
		SetMessage( message );
		AddButtons( buttons );
	}

	public MessageDialog( string title, string message, string button )
	{
		_body = LoadDialog( (GameObject)Resources.Load( "Dialog/MessageDialog" ) );
		SetTitle( title );
		SetMessage( message );
		AddButtons( new string[]{ button } );
	}
}
