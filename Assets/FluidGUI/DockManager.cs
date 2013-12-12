using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class DockManager : MonoBehaviour
{
	[SerializeField]
	private DockAreaConfiguration[] _dockAreas = new DockAreaConfiguration[]
	{
		new DockAreaConfiguration( Dockable.Left ),
		new DockAreaConfiguration( Dockable.Right ),
		new DockAreaConfiguration( Dockable.Top ),
		new DockAreaConfiguration( Dockable.Bottom )
	};

	public enum Dockable
	{
		Left,
		Right,
		Top,
		Bottom,
		Center
	}

	public enum DockCorners
	{
		TopLeft,
		TopRight,
		BottomLeft,
		BottomRight
	}

	[SerializeField]
	private bool _updateNow = false;

	private SortedList<int,DockArea>[] _dockAreaList = new SortedList<int,DockArea>[]
	{
		new SortedList<int,DockArea>(),
		new SortedList<int,DockArea>(),
		new SortedList<int,DockArea>(),
		new SortedList<int,DockArea>(),
		new SortedList<int,DockArea>()
	};

	private void Update()
	{
		if( _updateNow )
		{
			_updateNow = false;
			DockArea[] das = transform.GetComponentsInChildren<DockArea>();
			foreach( DockArea da in das )
				da.Redraw();
		}
	}


	public void RegisterArea( DockArea area )
	{
		int d = (int)area.MainArea;

		if( !_dockAreaList[d].ContainsValue( area ) )
			_dockAreaList[d].Add( area.Weight, area );
	}

	public void UnregisterArea( DockArea area )
	{
		int d = (int)area.MainArea;

		if( _dockAreaList[d].ContainsValue( area ) )
			_dockAreaList[d].RemoveAt( _dockAreaList[d].IndexOfValue( area ) );
	}

	public Vector4 GetArea( DockArea area )
	{
		Vector4 dims = new Vector4();

		int d = (int)area.MainArea;
		if( !_dockAreaList[d].ContainsValue( area ) )
		{
			Debug.LogError( "Area '" + area.name + "' not registered." );
			return dims;
		}

		int count = _dockAreaList[d].Count;

		if( area.MainArea == Dockable.Center )
		{
			dims.x = Screen.width - ( _dockAreas[(int)Dockable.Left].TotalValue + _dockAreas[(int)Dockable.Right].TotalValue );
			dims.y = Screen.height - ( _dockAreas[(int)Dockable.Top].TotalValue + _dockAreas[(int)Dockable.Bottom].TotalValue );

			dims.z = ( _dockAreas[(int)Dockable.Left].TotalValue - _dockAreas[(int)Dockable.Right].TotalValue ) / 2.0f;
			dims.w = ( _dockAreas[(int)Dockable.Bottom].TotalValue - _dockAreas[(int)Dockable.Top].TotalValue ) / 2.0f;

			// TODO: Configure multiple docks in center
		}
		else if( area.MainArea == Dockable.Left || area.MainArea == Dockable.Right )
		{
			dims.x = _dockAreas[(int)area.MainArea].TotalValue;
			dims.y = Screen.height - ( _dockAreas[(int)Dockable.Top].TotalValue + _dockAreas[(int)Dockable.Bottom].TotalValue );

			if( area.MainArea == Dockable.Left )
				dims.z = dims.x / 2;
			else if( area.MainArea == Dockable.Right )
				dims.z = ( -dims.x ) / 2;

			dims.w = ( _dockAreas[(int)Dockable.Bottom].TotalValue - _dockAreas[(int)Dockable.Top].TotalValue ) / 2.0f;

			foreach( DockCorners dc in area.Corners )
			{
				if( dc == DockCorners.TopLeft || dc == DockCorners.TopRight )
				{
					dims.y += _dockAreas[(int)Dockable.Top].TotalValue;
					dims.w += _dockAreas[(int)Dockable.Top].TotalValue / 2;
				}
				else if( dc == DockCorners.BottomLeft || dc == DockCorners.BottomRight )
				{
					dims.y += _dockAreas[(int)Dockable.Bottom].TotalValue;
					dims.w -= _dockAreas[(int)Dockable.Bottom].TotalValue / 2;
				}
			}

			dims.y /= count;
			dims.w += ( ( ( count / 2.0f ) - 0.5f ) - _dockAreaList[d].IndexOfValue( area ) ) * dims.y;
		}
		else if( area.MainArea == Dockable.Top || area.MainArea == Dockable.Bottom )
		{
			dims.x = Screen.width - ( _dockAreas[(int)Dockable.Left].TotalValue + _dockAreas[(int)Dockable.Right].TotalValue );
			dims.y = _dockAreas[(int)area.MainArea].TotalValue;
			
			if( area.MainArea == Dockable.Top )
				dims.w = ( -dims.y ) / 2;
			else if( area.MainArea == Dockable.Bottom )
				dims.w = dims.y / 2;

			dims.z = ( _dockAreas[(int)Dockable.Left].TotalValue - _dockAreas[(int)Dockable.Right].TotalValue ) / 2.0f;

			foreach( DockCorners dc in area.Corners )
			{
				if( dc == DockCorners.TopLeft || dc == DockCorners.BottomLeft )
				{
					dims.x += _dockAreas[(int)Dockable.Left].TotalValue;
					dims.z -= _dockAreas[(int)Dockable.Left].TotalValue / 2;
				}
				else if( dc == DockCorners.TopRight || dc == DockCorners.BottomRight )
				{
					dims.x += _dockAreas[(int)Dockable.Right].TotalValue;
					dims.z += _dockAreas[(int)Dockable.Right].TotalValue / 2;
				}
			}

			dims.x /= count;
			dims.z += ( ( ( count / 2.0f ) - 0.5f ) - _dockAreaList[d].IndexOfValue( area ) ) * dims.x;
		}

		return dims;
	}
}
