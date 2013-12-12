using UnityEngine;
using System.Collections;

// TODO: Localize

public class CloseButton : MonoBehaviour
{
	private MessageDialog _dialog;

	private void OnClick()
	{
		_dialog = DialogManager.instance.ShowMessage( "Exit Voxel Engine Demo?", "Are you sure you want to exit Voxel Engine Demo?", DialogManager.MessageDialogOptions.YesNoButtons );
		_dialog.OnDialogAccepted += OnDialogConfirmation;
	}

	private void OnDialogConfirmation()
	{
		Application.Quit();
	}

}
