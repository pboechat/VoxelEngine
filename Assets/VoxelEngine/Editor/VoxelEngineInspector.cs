using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

[CustomEditor (typeof(VoxelEngine))]
public class VoxelEngineInspector : Editor
{
	private VoxelEngine _target;
	private string filePath;
	
	void OnEnable ()
	{
		_target = (VoxelEngine)target;
		string[] path = EditorApplication.currentScene.Split ('/');
		path [path.Length - 1] = "VoxelEngine.config";
		filePath = string.Join ("/", path);
	}
	
	void SaveToFile ()
	{
		FileStream fileStream;
		StreamWriter writer;
		if (File.Exists (filePath)) {
			fileStream = File.OpenWrite (filePath);
			writer = new StreamWriter (fileStream);
		} else {
			fileStream = null;
			writer = File.CreateText (filePath);
		}
		writer.WriteLine ("voxel_size=" + _target.voxelSize);
		writer.WriteLine ("atlas=" + AssetDatabase.GetAssetPath (_target.atlas));
		writer.WriteLine ("tile_size=" + _target.tileSize);
		string voxelFaceMappingsStr = "voxel_face_mappings=";
		if (_target.voxelFaceMappings != null) {
			foreach (VoxelEngine.VoxelFaceMapping voxelFaceMapping in _target.voxelFaceMappings) {
				voxelFaceMappingsStr += string.Format ("({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}), ", 
				voxelFaceMapping.voxelId, 
				voxelFaceMapping.frontFaceTileId, 
				voxelFaceMapping.topFaceTileId,
				voxelFaceMapping.rightFaceTileId,
				voxelFaceMapping.backFaceTileId,
				voxelFaceMapping.bottomFaceTileId,
				voxelFaceMapping.leftFaceTileId,
				voxelFaceMapping.frontFaceTileIdWithoutTopNeighbor,
				voxelFaceMapping.rightFaceTileIdWithoutTopNeighbor,
				voxelFaceMapping.backFaceTileIdWithoutTopNeighbor,
				voxelFaceMapping.leftFaceTileIdWithoutTopNeighbor);
			}
			
			if (voxelFaceMappingsStr.Length > 2) {
				voxelFaceMappingsStr = voxelFaceMappingsStr.Substring (0, voxelFaceMappingsStr.Length - 2);
			}
		}
		writer.WriteLine (voxelFaceMappingsStr);
		writer.Close ();
		
		if (fileStream != null) {
			fileStream.Close ();
		}
	}
	
	static void ParseKeyValueString (string arg0, out string key, out string value)
	{
		string[] parts = arg0.Split ('=');
		if (parts.Length < 2) {
			throw new Exception ("invalid key/value pair: " + arg0);
		}
		key = parts [0];
		value = parts [1];
	}
	
	void LoadFromFile ()
	{
		if (!File.Exists (filePath)) {
			Debug.LogError ("voxel engine configuration file doesn't exist: " + filePath);
			return;
		}
		FileStream fileStream = File.OpenRead (filePath);
		StreamReader reader = new StreamReader (fileStream);
		string key, value;
		ParseKeyValueString (reader.ReadLine (), out key, out value);
		if (key != "voxel_size") {
			reader.Close ();
			Debug.LogError ("invalid configuration file property (expected=voxel_size, got=" + key + ")");
			return;
		}
		int voxelSize;
		if (!int.TryParse (value, out voxelSize)) {
			reader.Close ();
			Debug.LogError ("invalid configuration file value (property=voxel_size, expected_type=int, got=" + value + ")");
			return;
		}
		ParseKeyValueString (reader.ReadLine (), out key, out value);
		if (key != "atlas") {
			Debug.LogError ("invalid configuration file property (expected=atlas, got=" + key + ")");
			return;
		}
		string atlasPath = value;
		ParseKeyValueString (reader.ReadLine (), out key, out value);
		if (key != "tile_size") {
			reader.Close ();
			Debug.LogError ("invalid configuration file property (expected=tile_size, got=" + key + ")");
			return;
		}
		int tileSize;
		if (!int.TryParse (value, out tileSize)) {
			reader.Close ();
			Debug.LogError ("invalid configuration file value (property=tile_size, expected_type=int, got=" + value + ")");
			return;
		}
		string voxelFaceMappingsStr = reader.ReadLine ();
		List<VoxelEngine.VoxelFaceMapping> voxelFaceMappings = new List<VoxelEngine.VoxelFaceMapping> ();
		MatchCollection matches = Regex.Matches (voxelFaceMappingsStr, @"\([^\)]+\)");
		foreach (Match match in matches) {
			string voxelFaceMappingStr = match.Value;
			voxelFaceMappingStr = voxelFaceMappingStr.Replace ("(", "").Replace (")", "");
			string[] parts = voxelFaceMappingStr.Split (',');
			if (parts.Length != 11) {
				reader.Close ();
				Debug.LogError ("invalid voxel face mapping: " + voxelFaceMappingStr);
				return;
			}
			VoxelEngine.VoxelFaceMapping voxelFaceMapping = new VoxelEngine.VoxelFaceMapping ();
			if (!int.TryParse (parts [0], out voxelFaceMapping.voxelId)) {
				reader.Close ();
				Debug.LogError ("invalid voxel id in voxel face mapping");
				return;
			}
			
			if (!int.TryParse (parts [1], out voxelFaceMapping.frontFaceTileId)) {
				reader.Close ();
				Debug.LogError ("invalid front face tile id in voxel face mapping");
				return;
			}
			
			if (!int.TryParse (parts [2], out voxelFaceMapping.topFaceTileId)) {
				reader.Close ();
				Debug.LogError ("invalid top face tile id in voxel face mapping");
				return;
			}
			
			if (!int.TryParse (parts [3], out voxelFaceMapping.rightFaceTileId)) {
				reader.Close ();
				Debug.LogError ("invalid right face tile id in voxel face mapping");
				return;
			}
			
			if (!int.TryParse (parts [4], out voxelFaceMapping.backFaceTileId)) {
				reader.Close ();
				Debug.LogError ("invalid back face tile id in voxel face mapping");
				return;
			}
			
			if (!int.TryParse (parts [5], out voxelFaceMapping.bottomFaceTileId)) {
				reader.Close ();
				Debug.LogError ("invalid bottom face tile id in voxel face mapping");
				return;
			}
			
			if (!int.TryParse (parts [6], out voxelFaceMapping.leftFaceTileId)) {
				reader.Close ();
				Debug.LogError ("invalid left face tile id in voxel face mapping");
				return;
			}
			
			if (!int.TryParse (parts [7], out voxelFaceMapping.frontFaceTileIdWithoutTopNeighbor)) {
				reader.Close ();
				Debug.LogError ("invalid front face tile id in voxel face mapping");
				return;
			}
			
			if (!int.TryParse (parts [8], out voxelFaceMapping.rightFaceTileIdWithoutTopNeighbor)) {
				reader.Close ();
				Debug.LogError ("invalid right face tile id w/o top neighbor in voxel face mapping");
				return;
			}
			
			if (!int.TryParse (parts [9], out voxelFaceMapping.backFaceTileIdWithoutTopNeighbor)) {
				reader.Close ();
				Debug.LogError ("invalid left face tile id w/o top neighbor in voxel face mapping");
				return;
			}
			
			if (!int.TryParse (parts [10], out voxelFaceMapping.leftFaceTileIdWithoutTopNeighbor)) {
				reader.Close ();
				Debug.LogError ("invalid left face tile id w/o top neighbor in voxel face mapping");
				return;
			}
			
			voxelFaceMappings.Add (voxelFaceMapping);
		}
		reader.Close ();
		fileStream.Close ();
		_target.voxelSize = voxelSize;
		_target.atlas = (Material)AssetDatabase.LoadAssetAtPath (atlasPath, typeof(Material));
		_target.tileSize = tileSize;
		_target.voxelFaceMappings = voxelFaceMappings.ToArray ();
	}
	
	public override void OnInspectorGUI ()
	{
		_target.voxelSize = EditorGUILayout.FloatField ("Voxel Size", _target.voxelSize);
		_target.atlas = (Material)EditorGUILayout.ObjectField ("Atlas", _target.atlas, typeof(Material), true);
		_target.tileSize = EditorGUILayout.IntField ("Tile Size", _target.tileSize);
		SerializedProperty voxelFaceMappingsProperty = serializedObject.FindProperty ("voxelFaceMappings");
		EditorGUI.BeginChangeCheck ();
		EditorGUILayout.PropertyField (voxelFaceMappingsProperty, true);
		if (EditorGUI.EndChangeCheck ()) {
			serializedObject.ApplyModifiedProperties ();
		}
		if (GUILayout.Button ("Update")) {
			_target.Update ();
		}
		filePath = EditorGUILayout.TextField ("File Path", filePath);
		if (!string.IsNullOrEmpty (filePath)) {
			if (GUILayout.Button ("Load")) {
				LoadFromFile ();
			}
			if (GUILayout.Button ("Save")) {
				SaveToFile ();
			}
		}
		
		if (GUI.changed) {
			EditorUtility.SetDirty (_target);
		}
	}
	
}
