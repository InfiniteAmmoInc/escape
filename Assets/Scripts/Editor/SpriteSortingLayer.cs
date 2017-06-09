using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Collections;
using System.Reflection;

public class SpriteSortingLayer : Editor 
{

	public static void DrawGUI(Renderer renderer)
	{
		string name = renderer.sortingLayerName;
		int order = renderer.sortingOrder;
		DrawGUI (renderer, ref name, ref order);
		if (name != renderer.sortingLayerName)
			renderer.sortingLayerName = name;
		if (order != renderer.sortingOrder)
			renderer.sortingOrder = order;
	}

	public static void DrawGUI(UnityEngine.Object obj, ref string layerName, ref int layerOrder)
	{
		// get the list of layers
		string[] layerNames = GetSortingLayerNames();
		
		// get the index of the current sorting layer of the renderer
		int index = 0;
		for (int i = 0; i < layerNames.Length; i ++)
		{
			if (layerName == layerNames[i])
			{
				index = i;
				break;
			}
		}
		
		// show the sorting layer names
		int nextIndex;
		nextIndex = EditorGUILayout.Popup ("Sorting Layer", index, layerNames);
		
		// change sorting layer
		if (nextIndex != index)
		{
			Undo.RecordObject(obj, "Changed Sorting Layer Name");
			string nextLayerName = layerNames[nextIndex];
			layerName = nextLayerName;
			EditorUtility.SetDirty(obj);
		}
		
		// sorting order
		int newSortingLayerOrder = EditorGUILayout.IntField("Order in Layer", layerOrder);
		if (newSortingLayerOrder != layerOrder) 
		{
			Undo.RecordObject(obj, "Edit Sorting Order");
			layerOrder = newSortingLayerOrder;
			EditorUtility.SetDirty(obj);
		}
	}

	// Get the sorting layer names
	public static string[] GetSortingLayerNames() 
	{
		Type internalEditorUtilityType = typeof(InternalEditorUtility);
		PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
		return (string[])sortingLayersProperty.GetValue(null, new object[0]);
	}
	
	/*
	// Get the unique sorting layer IDs
	public static int[] GetSortingLayerUniqueIDs() 
	{
		Type internalEditorUtilityType = typeof(InternalEditorUtility);
		PropertyInfo sortingLayerUniqueIDsProperty = internalEditorUtilityType.GetProperty("sortingLayerUniqueIDs", BindingFlags.Static | BindingFlags.NonPublic);
		return (int[])sortingLayerUniqueIDsProperty.GetValue(null, new object[0]);
	}
	*/

}
