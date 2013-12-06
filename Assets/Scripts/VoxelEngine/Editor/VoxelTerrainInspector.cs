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
				_target.minimumHeight = EditorGUILayout.IntField ("Minimum Height", _target.minimumHeight);
				_target.maximumHeight = EditorGUILayout.IntField ("Maximum Height", _target.maximumHeight);
				_target.useSeed = EditorGUILayout.BeginToggleGroup ("Use Seed", _target.useSeed);
				_target.seed = EditorGUILayout.IntField ("Seed", _target.seed);
				EditorGUILayout.EndToggleGroup ();
				
				_target.material = (Material)EditorGUILayout.ObjectField ("Material", _target.material, typeof(Material), true);
		
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

