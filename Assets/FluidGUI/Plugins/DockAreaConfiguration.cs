using UnityEngine;
using System.Collections;

[System.Serializable]
public class DockAreaConfiguration
{
	//[SerializeField]
	//[HideInInspector]
	//private string _name;
	
	[SerializeField]
	[HideInInspector]
	private DockManager.Dockable _area;


	[SerializeField]
	private int _absoluteValue;
	[SerializeField]
	private float _relativeValue;

	public DockAreaConfiguration( DockManager.Dockable area )
	{
		_area = area;
		//_name = _area.ToString();
	}

	public int TotalValue
	{
		get
		{
			if( _area == DockManager.Dockable.Left || _area == DockManager.Dockable.Right )
				return (int)( ( Screen.width * _relativeValue ) + _absoluteValue );
			else if( _area == DockManager.Dockable.Top || _area == DockManager.Dockable.Bottom )
				return (int)( ( Screen.height * _relativeValue ) + _absoluteValue );
			return 0;
		}
	}
}
