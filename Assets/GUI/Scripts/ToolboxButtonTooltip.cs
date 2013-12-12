using UnityEngine;
using System.Collections;

public class ToolboxButtonTooltip : MonoBehaviour
{
	private void OnTooltip( bool show )
	{
		if( show )
			Tooltip.Show( gameObject.name.Split( '_' )[2] );
		else
			Tooltip.Hide();
	}
}
