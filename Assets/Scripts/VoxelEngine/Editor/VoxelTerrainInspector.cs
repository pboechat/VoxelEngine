using UnityEngine;
using UnityEditor;
using System;

[CustomEditor (typeof(VoxelTerrain))]
public class VoxelTerrainInspector : Editor
{
	VoxelTerrain _target;
	
	void OnEnable ()
	{
		_target = (VoxelTerrain)target;
	}
	
	public override void OnInspectorGUI ()
	{
		_target.width = EditorGUILayout.IntField ("Width", _target.width);
		_target.depth = EditorGUILayout.IntField ("Depth", _target.depth);
		_target.height = EditorGUILayout.IntField ("Height", _target.height);
		_target.minimumHeight = EditorGUILayout.IntField ("Minimum Height", _target.minimumHeight);
		_target.maximumHeight = EditorGUILayout.IntField ("Maximum Height", _target.maximumHeight);
		_target.scale = EditorGUILayout.Slider ("Scale", _target.scale, 1.0f, 10.0f);
		_target.smoothness = EditorGUILayout.Slider ("Smoothness", _target.smoothness, 1.0f, 100.0f);
		_target.useSeed = EditorGUILayout.BeginToggleGroup ("Use Seed", _target.useSeed);
		_target.seed = EditorGUILayout.IntField ("Seed", _target.seed);
		EditorGUILayout.EndToggleGroup ();
		_target.resizeDirection = (VoxelTerrain.ResizeDirection)EditorGUILayout.EnumPopup ("Resize Direction", _target.resizeDirection);
		_target.buildOnStart = EditorGUILayout.Toggle ("Build On Start", _target.buildOnStart);
		
		if (GUILayout.Button ("Build")) {
			_target.Build ();
		}
				
		if (GUILayout.Button ("Clear")) {
			_target.Clear ();
		}
		
		if (GUI.changed) {
			EditorUtility.SetDirty (_target);
		}
	}
	
}

