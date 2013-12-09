using UnityEngine;
using UnityEditor;
using System;

[CustomEditor (typeof(VoxelEngine))]
public class VoxelEngineInspector : Editor
{
	VoxelEngine _target;
	float newVoxelSize;
	
	void OnEnable ()
	{
		_target = (VoxelEngine)target;
		newVoxelSize = _target.voxelSize;
	}
	
	public override void OnInspectorGUI ()
	{
		newVoxelSize = EditorGUILayout.FloatField ("Voxel Size", newVoxelSize);
		_target.atlas = (Material)EditorGUILayout.ObjectField ("Atlas", _target.atlas, typeof(Material), true);
		int tileSize = EditorGUILayout.IntField ("Tile Size", _target.tileSize);
		if (GUILayout.Button ("Update")) {
			_target.SetTileSize (tileSize);
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
