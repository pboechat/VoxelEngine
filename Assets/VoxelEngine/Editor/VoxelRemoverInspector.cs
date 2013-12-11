using UnityEngine;
using UnityEditor;
using System;

[CustomEditor (typeof(VoxelRemover))]
public class VoxelRemoverInspector : Editor
{
	VoxelRemover _target;
	string newMaskString;
	bool updated;
	
	static bool[] GetMaskFromString (int width, int height, int depth, string maskString)
	{
		string[] layers = maskString.Split ('-');
		int size = height * depth * width;
		bool[] mask = new bool[size];
		int c = 0;
		foreach (string layer in layers) {
			string[] lines = layer.Split ('\n');
			foreach (string line in lines) {
				string row = line.Trim ();
				for (int i = 0; i < row.Length; i++) {
					bool m = (row [i] == '1');
					// cut in the middle of processing
					if (c == size) {
						return mask;
					}
					mask [c++] = m;
				}
			}
		}
		
		if (c != size) {
			throw new Exception ("insuficient mask data");
		}
		
		return mask;
	}
	
	static string GetStringFromMask (int width, int height, int depth, bool[] mask)
	{
		string dataString = "";
		for (int y = 0; y < height; y++) {
			for (int z = 0; z < depth; z++) {
				for (int x = 0; x < width; x++) {
					dataString += ((mask [y * (depth * width) + z * width + x]) ? "1" : "0");
				}
				dataString += "\n";
			}
			dataString += "-\n";
		}
		return dataString;
	}
	
	void OnEnable ()
	{
		_target = (VoxelRemover)target;
		if (_target.queryMask != null) {
			newMaskString = GetStringFromMask (_target.queryWidth, _target.queryHeight, _target.queryDepth, _target.queryMask);
		} else {
			newMaskString = "";
		}
	}
	
	public override void OnInspectorGUI ()
	{
		_target.queryWidth = EditorGUILayout.IntField ("Query Width", _target.queryWidth);
		_target.queryHeight = EditorGUILayout.IntField ("Query Height", _target.queryHeight);
		_target.queryDepth = EditorGUILayout.IntField ("Query Depth", _target.queryDepth);
		_target.terrain = (VoxelTerrain)EditorGUILayout.ObjectField ("Terrain", _target.terrain, typeof(VoxelTerrain), true);
		
		newMaskString = EditorGUILayout.TextArea (newMaskString);
		
		if (GUILayout.Button ("Update")) {
			_target.queryMask = GetMaskFromString (_target.queryWidth, _target.queryHeight, _target.queryDepth, newMaskString);
			_target.DisplayArea ();
		}
		
		if (_target.terrain != null && _target.queryMask != null) {
			if (GUILayout.Button ("Execute")) {
				_target.Execute ();
			}
		}
		
		if (GUI.changed) {
			EditorUtility.SetDirty (_target);
		}
	}
	
}

