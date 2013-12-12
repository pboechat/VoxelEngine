using UnityEngine;
using UnityEditor;
using System;

[CustomEditor (typeof(VoxelTerrain))]
public class VoxelTerrainInspector : Editor
{
	private VoxelTerrain _target;
	private static bool saveToFile;
	private static string saveFilePath;
	
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
		_target.resizeDirection = (VoxelTerrain.ResizeDirection)EditorGUILayout.EnumPopup ("Resize Direction", _target.resizeDirection);
		_target.hideChunks = EditorGUILayout.Toggle ("Hide Chunks", _target.hideChunks);
		_target.useSeed = EditorGUILayout.BeginToggleGroup ("Use Seed", _target.useSeed);
		_target.seed = EditorGUILayout.IntField ("Seed", _target.seed);
		EditorGUILayout.EndToggleGroup ();
		_target.executeOnStart = EditorGUILayout.Toggle ("Execute On Start", _target.executeOnStart);
		_target.loadFromFile = EditorGUILayout.BeginToggleGroup ("Load From File", _target.loadFromFile);
		_target.filePath = EditorGUILayout.TextField ("File Path", _target.filePath);
		if (GUILayout.Button ("Load")) {
			_target.LoadFromFile ();
		}
		EditorGUILayout.EndToggleGroup ();

		if (GUILayout.Button ("Generate")) {
			_target.Generate ();
		}
				
		saveToFile = EditorGUILayout.BeginToggleGroup("Save To File", saveToFile);
		saveFilePath = EditorGUILayout.TextField ("File Path", saveFilePath);
		if (GUILayout.Button ("Save")) {
			_target.SaveToFile (saveFilePath);
		}
		EditorGUILayout.EndToggleGroup();
				
		if (GUILayout.Button ("Clear")) {
			_target.Clear ();
		}
		
		if (GUI.changed) {
			EditorUtility.SetDirty (_target);
		}
	}
	
}

