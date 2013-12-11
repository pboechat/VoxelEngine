using UnityEngine;
using UnityEditor;
using System;

[CustomEditor (typeof(VoxelEngine))]
public class VoxelEngineInspector : Editor
{
	VoxelEngine _target;
	float newVoxelSize;
	int newTileSize;
	
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
		/*SerializedProperty tileMapsProperty = serializedObject.FindProperty ("_tileMaps");
				EditorGUI.BeginChangeCheck ();
				EditorGUILayout.PropertyField (tileMapsProperty, true);
				if (EditorGUI.EndChangeCheck ()) {
						serializedObject.ApplyModifiedProperties ();
				}*/
		
		if (GUI.changed) {
			EditorUtility.SetDirty (_target);
		}
	}
	
}
