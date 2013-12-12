using UnityEngine;
using System.Collections;

// TODO: Localize

public class CloseButton : MonoBehaviour
{
	private MessageDialog _dialog;

	private void OnClick()
	{
		_dialog = DialogManager.instance.ShowMessage( "Exit Vox Regis?", "Are you sure you want to exit Vox Regis?", DialogManager.MessageDialogOptions.YesNoButtons );
	}

	private void OnDialogConfirmation()
	{
		DialogManager.instance.OnDialogHidden -= OnDialogConfirmation;

		if( _dialog.Result == 1 )
			Application.Quit();
		else
			_dialog.Destroy();
	}

}
