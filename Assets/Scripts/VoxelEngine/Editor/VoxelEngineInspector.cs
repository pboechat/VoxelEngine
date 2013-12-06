using UnityEngine;
using UnityEditor;
using System;

[CustomEditor (typeof(VoxelEngine))]
public class VoxelEngineInspector : Editor
{
		VoxelEngine _target;
	
		void OnEnable ()
		{
				_target = (VoxelEngine)target;
		}
	
		public override void OnInspectorGUI ()
		{
				float newVoxelSize = EditorGUILayout.FloatField ("Voxel Size", _target.voxelSize);
				_target._SetVoxelSize (newVoxelSize);
		
				if (GUI.changed) {
						EditorUtility.SetDirty (_target);
				}
		}
	
}
