using UnityEngine;
using System;
using System.Collections;

[RequireComponent( typeof( UIPanel ) )]
[RequireComponent( typeof( UIAnchor ) )]
public class DockArea : MonoBehaviour
{
	[SerializeField]
	private DockManager.Dockable _dockArea;

	[SerializeField]
	private Vector2 _forceOverflow;

	[SerializeField]
	private int _weight;

	public DockManager.Dockable MainArea
	{
		get{ return _dockArea; }
	}

	public int Weight
	{
		get{ return _weight; }
	}

	[SerializeField]
	private DockManager.DockCorners[] _corners = new DockManager.DockCorners[0];

	public DockManager.DockCorners[] Corners
	{
		get{ return _corners; }
	}

	private DockManager _dockManager;

	private void Awake()
	{
		_dockManager = NGUITools.FindInParents<DockManager>( gameObject );
		
		if( _dockManager == null )
		{
			Debug.LogError( "DockArea " + gameObject.name + " doesn't have an ancestor with DockManager" );
			this.enabled = false;
			return;
		}
	}

	private void OnEnable()
	{
		_dockManager.RegisterArea( this );
	}

	private void OnDisable()
	{
		_dockManager.UnregisterArea( this );
	}

	private void OnGUI()
	{
		Redraw();
		//this.enabled = false;
	}

	public void Redraw()
	{
		UIAnchor anc = GetComponent<UIAnchor>();
		UIPanel pan = GetComponent<UIPanel>();

		anc.side = (UIAnchor.Side)Enum.Parse( typeof( UIAnchor.Side ), _dockArea.ToString() );

		if( _dockManager == null ) 
			_dockManager = NGUITools.FindInParents<DockManager>( gameObject );

		Vector4 dim = _dockManager.GetArea( this );

		pan.clipping = UIDrawCall.Clipping.AlphaClip;
		pan.clipRange = new Vector4( 0, 0, dim.x + ( _forceOverflow.x * 2 ), dim.y + ( _forceOverflow.y * 2 ) );
		
		anc.pixelOffset.x = dim.z;
		anc.pixelOffset.y = dim.w;

		anc.enabled = true;

		UIStretch[] strs = GetComponentsInChildren<UIStretch>();
		foreach( UIStretch st in strs )
			st.enabled = true;

		UIAnchor[] ancs = GetComponentsInChildren<UIAnchor>();
		foreach( UIAnchor an in ancs )
			an.enabled = true;
	}
}
