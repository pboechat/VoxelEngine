using UnityEngine;
using System.Collections;

public class UIButtonEvent : MonoBehaviour
{
	public delegate void ClickAction( GameObject button );
	public event ClickAction Clicked = delegate{};
	
	void OnClick()
	{
		Clicked( gameObject );
	}

}
