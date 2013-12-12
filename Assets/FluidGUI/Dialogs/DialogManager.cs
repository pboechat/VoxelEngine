using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DialogManager : Singleton<DialogManager>
{
	// Hide constructor for Singleton
	protected DialogManager()
	{
	}
	
	public delegate void DialogShowOrHide();
	public event DialogShowOrHide OnDialogShown = delegate{};
	public event DialogShowOrHide OnDialogHidden = delegate{};

	private AbstractDialog _currentDialog = null;
	
	public void ShowDialog( AbstractDialog ad )
	{
		if( _currentDialog != null )
		{
			_currentDialog.Destroy();
			_currentDialog = null;
		}

		_currentDialog = ad;
		_currentDialog.Show();
	
		OnDialogShown();
	}
	
	public void HideDialog( AbstractDialog ad, bool destroy = true )
	{
		if( ad == null )
			return;
		else if( ad != _currentDialog )
		{
			ad.Destroy();
			ad = null;
			return;
		}

		if( _currentDialog == null )
			return;

		if( destroy )
			_currentDialog.Destroy();

		_currentDialog = null;

		OnDialogHidden();
	}

	public bool AnyDialogOpen
	{
		get{ return _currentDialog != null; }
	}

	public void DialogConfirm()
	{
		_currentDialog.Confirm();
	}

	public void DialogCancel()
	{
		_currentDialog.Cancel();
	}

//	#region New Scene Dialog
//	public void ShowNewSceneDialog()
//	{
//		if( AnyDialogOpen )
//			return;
//		
//		MessageDialog newSceneDialog = new MessageDialog( "Nova cena", "Deseja descartar a cena atual e começar uma nova?", new string[]{ "No", "Yes" } );
//		newSceneDialog.OnDialogClosed += DoNewScene;
//
//		ShowDialog( newSceneDialog );
//	}
//	
//	private void DoNewScene()
//	{
//		MessageDialog newSceneDialog = (MessageDialog)_currentDialog;
//
//		if( newSceneDialog == 1 )
//			SceneManager.Instance.NewScene();
//		
//		HideDialog( newSceneDialog );
//	}
//	#endregion
//
//	#region Save Dialog
//	public void ShowSaveDialog()
//	{
//		if( AnyDialogOpen )
//			return;
//
//		InputDialog saveDialog = new InputDialog( "Salvar", "Onde deseja salvar o arquivo de cena?", "Cancelar", "Salvar" );
//		saveDialog.OnDialogClosed += DoSave;
//
//		ShowDialog( saveDialog );
//	}
//
//	private void DoSave()
//	{
//		InputDialog saveDialog = (InputDialog)_currentDialog;
//
//		if( saveDialog )
//		{
//			if( saveDialog.Input == "" )
//				DialogManager.Instance.ShowMessage( "Erro", "Arquivo invalido!", DialogManager.MessageDialogOptions.OkayButtonOnly );
//			else
//				SceneManager.Instance.Save( saveDialog.Input );
//		}
//
//		HideDialog( saveDialog );
//	}
//	#endregion
//	
//	#region Load Dialog
//	public void ShowLoadDialog()
//	{
//		if( AnyDialogOpen )
//			return;
//		
//		InputDialog loadDialog = new InputDialog( "Carregar", "Qual arquivo de cena deseja carregar?", "Cancelar", "Carregar" );
//		loadDialog.OnDialogClosed += DoLoad;
//
//		ShowDialog( loadDialog );
//	}
//	
//	private void DoLoad()
//	{
//		InputDialog loadDialog = (InputDialog)_currentDialog;
//
//		if( loadDialog )
//		{
//			if( loadDialog.Input == "" )
//				DialogManager.Instance.ShowMessage( "Erro", "Arquivo invalido!", DialogManager.MessageDialogOptions.OkayButtonOnly );
//			else
//				SceneManager.Instance.Load( loadDialog.Input );
//		}
//		
//		HideDialog( loadDialog );
//	}
//	#endregion

	#region Generic Message Dialog
	public enum MessageDialogOptions
	{
		OkayButtonOnly,
		OkayCancelButtons,
		YesNoButtons
	}

	private List<MessageDialog> _messageDialogs = new List<MessageDialog>();
	public MessageDialog ShowMessage( string title, string message, MessageDialogOptions options )
	{
		MessageDialog md = null;
		switch( options )
		{
			case MessageDialogOptions.OkayButtonOnly:
				md = new MessageDialog( title, message, "Okay" );
				break;

			case MessageDialogOptions.OkayCancelButtons:
				md = new MessageDialog( title, message, new string[]{ "Cancel", "Okay" } );
				break;

			case MessageDialogOptions.YesNoButtons:
				md = new MessageDialog( title, message, new string[]{ "No", "Yes" } );
				break;
		}

		if( md == null )
			return null;

		md.OnDialogClosed += MessageDialogClosed;
		_messageDialogs.Add( md );
		ShowDialog( md );
		return md;
	}

	private void MessageDialogClosed()
	{
		MessageDialog md = (MessageDialog)_currentDialog;
		_messageDialogs.Remove( md );

		HideDialog( md, false );
	}
	#endregion
}
