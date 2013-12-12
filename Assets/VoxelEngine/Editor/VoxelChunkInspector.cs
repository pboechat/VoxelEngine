using UnityEngine;
using UnityEditor;
using System;

[CustomEditor (typeof(VoxelChunk))]
public class VoxelChunkInspector : Editor
{
		VoxelChunk _target;
		string newDataString;
    
		static Voxel[] GetDataFromString (int width, int height, int depth, string dataString)
		{
				string[] layers = dataString.Split ('-');
				int size = height * depth * width;
				Voxel[] data = new Voxel[size];
				int c = 0;
				foreach (string layer in layers) {
						string[] lines = layer.Split ('\n');
						foreach (string line in lines) {
								string row = line.Trim ();
								for (int i = 0; i < row.Length; i++) {
										Voxel v = new Voxel();
										int j;
										if (int.TryParse (row [i] + "", out j)) {
												v.id = (byte)j;
										} else {
												v.id = 0;
										}
										// cut in the middle of processing
										if (c == size) {
												return data;
										}
										data [c++] = v;
								}
						}
				}
				
				if (c != size) {
						throw new Exception ("insuficient voxel data");
				}
				
				return data;
		}
		
		static string GetStringFromData (int width, int height, int depth, Voxel[] data)
		{
				string dataString = "";
				for (int y = 0; y < height; y++) {
						for (int z = 0; z < depth; z++) {
								for (int x = 0; x < width; x++) {
										dataString += data [y * (depth * width) + z * width + x].id;
								}
								dataString += "\n";
						}
						dataString += "-\n";
				}
				return dataString;
		}

		void OnEnable ()
		{
				_target = (VoxelChunk)target;
				if (_target.data != null) {
						newDataString = GetStringFromData (_target.width, _target.height, _target.depth, _target.data);
				} else {
						newDataString = "";
				}
		}
  
		public override void OnInspectorGUI ()
		{
				_target.width = EditorGUILayout.IntField ("Width", _target.width);
				_target.height = EditorGUILayout.IntField ("Height", _target.height);
				_target.depth = EditorGUILayout.IntField ("Depth", _target.depth);
				
				newDataString = EditorGUILayout.TextArea (newDataString);
				
				if (GUILayout.Button ("Build")) {
						_target.data = GetDataFromString (_target.width, _target.height, _target.depth, newDataString);
						_target.Build ();
				}

				if (GUI.changed) {
						EditorUtility.SetDirty (_target);
				}
		}

}
