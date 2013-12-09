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
		_target.pickingButton = EditorGUILayout.IntField ("Picking Button", _target.pickingButton);
		_target.pickingDistance = EditorGUILayout.FloatField ("Picking Distance", _target.pickingDistance);
		_target.camera = (Camera)EditorGUILayout.ObjectField ("Camera", _target.camera, typeof(Camera), true);
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
