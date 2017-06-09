using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(MeshRenderer))]
public class MeshRendererEditor : Editor 
{
	override public void OnInspectorGUI()
	{
		// base stuff
		base.OnInspectorGUI ();

		// the sprite layout stuff
		EditorGUILayout.Space ();
		SpriteSortingLayer.DrawGUI((target as MeshRenderer));
		EditorGUILayout.Space ();
	}
	
}
