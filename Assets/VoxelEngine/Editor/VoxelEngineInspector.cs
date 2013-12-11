using UnityEngine;
using UnityEditor;
using System;

[CustomEditor (typeof(VoxelEngine))]
public class VoxelEngineInspector : Editor
{
	private VoxelEngine _target;
	private float newVoxelSize;
	private int newTileSize;
	
	void OnEnable ()
	{
		_target = (VoxelEngine)target;
		newVoxelSize = _target.voxelSize;
		newTileSize = _target.tileSize;
	}
	
	public override void OnInspectorGUI ()
	{
		newVoxelSize = EditorGUILayout.FloatField ("Voxel Size", newVoxelSize);
		_target.atlas = (Material)EditorGUILayout.ObjectField ("Atlas", _target.atlas, typeof(Material), true);
		newTileSize = EditorGUILayout.IntField ("Tile Size", newTileSize);
		if (GUILayout.Button ("Update")) {
			_target.SetTileSize (newTileSize);
			_target.SetVoxelSize (newVoxelSize);
		}
		
		if (GUI.changed) {
			EditorUtility.SetDirty (_target);
		}
	}
	
}
