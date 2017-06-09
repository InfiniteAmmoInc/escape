using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Scape;

[CustomEditor(typeof(Path))]
[CanEditMultipleObjects]
public class EditPath : Editor
{
	enum State { Hover, Drag }
	enum Mode { Normal, Add, Remove, AvgX, AvgY, Select }
	Mode mode;
	const float clickRadius = 0.12f;
	Vector3 mousePosition;
	Path currentPath;
	State state;
	int dragIndex;
	Vector3 dragOffset;
	bool editing;
	Event e;
	enum Constrain { None, X, Y}
	Constrain constrain;
	bool leftShift;
	Vector3 originalPoint;

	bool KeyPressed(KeyCode key)
	{
		e = Event.current;
		return e.type == EventType.KeyDown && e.keyCode == key;
	}

	public override void OnInspectorGUI()
	{
		if (editing)
		{
			if (GUILayout.Button("Stop Editing Path"))
				editing = false;
		}
		else
		{
			if (GUILayout.Button("Edit Path"))
				editing = true;
		}
		base.OnInspectorGUI();

		if (GUI.changed)
			EditorUtility.SetDirty(target);
	}

	void OnSceneGUI()
	{
		float s = 40f;
		float s2 = s/2f;
		//float sb = 30f;
		//float sb2 = sb/2f;
		float d = 160f;

		currentPath = (Path)target;
		e = Event.current;

		
		if (!editing)
		{
		}
		else
		{

			if (e.type == EventType.Layout)
			{
				HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
			}

            const int buttonWidth = 60;
            const int buttonHeight = 40;

            Handles.BeginGUI();
			if (mode == Mode.Normal)
			{


				if (GUI.Button(new Rect(0, 0, buttonWidth, buttonHeight), "Add"))
					mode = Mode.Add;
				if (GUI.Button(new Rect(buttonWidth * 1, 0, buttonWidth, buttonHeight), "Rem"))
					mode = Mode.Remove;
				if (GUI.Button(new Rect(buttonWidth * 2, 0, buttonWidth, buttonHeight), "AvgX"))
					mode = Mode.AvgX;
				if (GUI.Button(new Rect(buttonWidth * 3, 0, buttonWidth, buttonHeight), "AvgY"))
					mode = Mode.AvgY;
                if (GUI.Button(new Rect(buttonWidth * 4, 0, buttonWidth, buttonHeight), "Recntr"))
                    Recenter();
				if (GUI.Button(new Rect(buttonWidth * 5, 0, buttonWidth, buttonHeight), "X"))
					editing = false;
			}
			else if (mode == Mode.Add)
			{
				if (GUI.Button(new Rect(0, 0, buttonWidth, buttonHeight), "X"))
				{
					mode = Mode.Normal;
				}

				Vector3 pos = Vector3.zero;
				for (int i = 0; i < currentPath.nodes.Length-1; i++)
				{
					Vector3 p1 = currentPath.nodes[i].point;
					Vector3 p2 = currentPath.nodes[i+1].point;
					Vector3 p = (p2 - p1) * .5f + p1;
					pos = SceneView.currentDrawingSceneView.camera.WorldToScreenPoint(currentPath.transform.TransformPoint(p));
					if (GUI.Button(new Rect(pos.x - s2, Screen.height - pos.y - s2 - d*.25f, s, s), "Add"))
					{
						Undo.RegisterCompleteObjectUndo(currentPath, "Add Point");
						ArrayUtility.Insert(ref currentPath.nodes, i+1, new Path.Node(p));
						break;
					}
				}

				// add buttons to beginning + end
				if (currentPath.nodes.Length > 1)
				{
					//const float maxDistance = 2f;
					//const float minDistance = .25f;

					{
						Vector3 p1 = currentPath.nodes[0].point;
						Vector3 p2 = currentPath.nodes[1].point;
						Vector3 p = p1 - (p2-p1).normalized * .5f;
						pos = SceneView.currentDrawingSceneView.camera.WorldToScreenPoint(currentPath.transform.TransformPoint(p));

						if (GUI.Button(new Rect(pos.x - s2, Screen.height - pos.y - s2 - d*.25f, s, s), "Add"))
						{
							Undo.RegisterCompleteObjectUndo(currentPath, "Add Point");
							ArrayUtility.Insert(ref currentPath.nodes, 0, new Path.Node(p));
						}
					}

					{
						Vector3 p1 = currentPath.nodes[currentPath.nodes.Length-2].point;
						Vector3 p2 = currentPath.nodes[currentPath.nodes.Length-1].point;
						Vector3 p = p2 + (p2-p1).normalized * .5f;
						pos = SceneView.currentDrawingSceneView.camera.WorldToScreenPoint(currentPath.transform.TransformPoint(p));

						/*
						Vector3 pdiff = pos - entity.transform.TransformPoint(p1);
						if (pdiff.sqrMagnitude > maxDistance * maxDistance)
						{
							pos = pdiff.normalized * maxDistance + entity.transform.TransformPoint(p1);
						}
						else if (pdiff.sqrMagnitude < minDistance * minDistance)
						{
							pos = pdiff.normalized * minDistance + entity.transform.TransformPoint(p1);
						}
						*/

						if (GUI.Button(new Rect(pos.x - s2, Screen.height - pos.y - s2 - d*.25f, s, s), "Add"))
						{
							Undo.RegisterCompleteObjectUndo(currentPath, "Add Point");
							ArrayUtility.Insert(ref currentPath.nodes, currentPath.nodes.Length, new Path.Node(p));
						}
					}
				}
				else
				{

				}
			}
			else if (mode == Mode.Remove)
			{
				if (GUI.Button(new Rect(0, 0, buttonWidth, buttonHeight), "X"))
				{
					mode = Mode.Normal;
				}

				Vector3 pos = Vector3.zero;
				for (int i = 0; i < currentPath.nodes.Length; i++)
				{
					pos = SceneView.currentDrawingSceneView.camera.WorldToScreenPoint(currentPath.transform.TransformPoint(currentPath.nodes[i].point));
					if (GUI.Button(new Rect(pos.x - s2, Screen.height - pos.y - s2, s, s), "Rem"))
					{
						Undo.RegisterCompleteObjectUndo(currentPath, "Remove Point");
						ArrayUtility.RemoveAt(ref currentPath.nodes, i);
						break;
					}
				}
			}
			else if (mode == Mode.AvgX)
			{
				if (GUI.Button(new Rect(0, 0, buttonWidth, buttonHeight), "X"))
				{
					mode = Mode.Normal;
				}

				Vector3 pos = Vector3.zero;
				for (int i = 0; i < currentPath.nodes.Length-1; i++)
				{
					Vector3 p1 = currentPath.nodes[i].point;
					Vector3 p2 = currentPath.nodes[i+1].point;
					Vector3 p = (p2 - p1) * .5f + p1;
					pos = SceneView.currentDrawingSceneView.camera.WorldToScreenPoint(currentPath.transform.TransformPoint(p));
					if (GUI.Button(new Rect(pos.x - s2, Screen.height - pos.y - s2 - d*.25f, s, s), "AvgX"))
					{
						Undo.RegisterCompleteObjectUndo(currentPath, "AvgX");
						float avgX = (currentPath.nodes[i].point.x + currentPath.nodes[i+1].point.x) * .5f;
						currentPath.nodes[i].point.x = currentPath.nodes[i+1].point.x = avgX;
						break;
					}
				}
			}
			else if (mode == Mode.AvgY)
			{
				if (GUI.Button(new Rect(0, 0, buttonWidth, buttonHeight), "X"))
				{
					mode = Mode.Normal;
				}

				Vector3 pos = Vector3.zero;
				for (int i = 0; i < currentPath.nodes.Length-1; i++)
				{
					Vector3 p1 = currentPath.nodes[i].point;
					Vector3 p2 = currentPath.nodes[i+1].point;
					Vector3 p = (p2 - p1) * .5f + p1;
					pos = SceneView.currentDrawingSceneView.camera.WorldToScreenPoint(currentPath.transform.TransformPoint(p));
					if (GUI.Button(new Rect(pos.x - s2, Screen.height - pos.y - s2 - d*.25f, s, s), "AvgY"))
					{
						Undo.RegisterCompleteObjectUndo(currentPath, "AvgY");
						float avgY = (currentPath.nodes[i].point.y + currentPath.nodes[i+1].point.y) * .5f;
						currentPath.nodes[i].point.y = currentPath.nodes[i+1].point.y = avgY;
						break;
					}
				}
			}
			Handles.EndGUI();
		}
		

		leftShift = e.shift;

		if (editing && currentPath.nodes != null && currentPath.nodes.Length > 0)
		{
			
			//Quit if panning or no camera exists
			if (Tools.current == Tool.View || (e.isMouse && e.button > 0) || Camera.current == null || e.type == EventType.ScrollWheel)
				return;
			
			//Quit if laying out
			if (e.type == EventType.Layout)
			{
				HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
				return;
			}

			Vector3 screenMousePosition = new Vector3(e.mousePosition.x, Camera.current.pixelHeight - e.mousePosition.y);
			var plane = new Plane(-currentPath.transform.forward, currentPath.transform.position);
			var ray = Camera.current.ScreenPointToRay(screenMousePosition);
			float hit;
			if (plane.Raycast(ray, out hit))
				mousePosition = currentPath.transform.worldToLocalMatrix.MultiplyPoint(ray.GetPoint(hit));
			else
				return;

			if (state == State.Hover)
			{
				if (e.type == EventType.MouseDown)
				{
					for (int i = 0; i < currentPath.nodes.Length; i++)
					{
						if (IsHovering(currentPath.nodes[i].point))
						{
							Undo.RegisterCompleteObjectUndo(currentPath, "Move Point");
							//Undo.RecordObject(entity, "Move Point");
							e.Use();
							originalPoint = currentPath.nodes[i].point;
							dragOffset = currentPath.nodes[i].point - mousePosition;
							dragIndex = i;

							state = State.Drag;
							break;
						}
					}
				}
			}
			else if (state == State.Drag)
			{
				if (leftShift)
				{
					if (constrain == Constrain.None)
					{
						Vector3 moveDiff = mousePosition - originalPoint;
						if (moveDiff.sqrMagnitude > 0f)
						{
							if (Mathf.Abs(moveDiff.x) > Mathf.Abs(moveDiff.y))
								constrain = Constrain.Y;
							else
								constrain = Constrain.X;
						}
					}
				}
				else
				{
					constrain = Constrain.None;
				}


				if (e.type == EventType.MouseUp)
				{
					state = State.Hover;
					e.Use();
				}
				else if (e.type == EventType.MouseDown)
				{

				}
				else
				{
					Vector3 wantPosition = mousePosition + dragOffset;
					if (constrain == Constrain.None)
						currentPath.nodes[dragIndex].point = wantPosition;
					else if (constrain == Constrain.X)
						currentPath.nodes[dragIndex].point = new Vector3(originalPoint.x, wantPosition.y, 0f);
					else if (constrain == Constrain.Y)
						currentPath.nodes[dragIndex].point = new Vector3(wantPosition.x, originalPoint.y, 0f);
				}
			}

			Handles.matrix = currentPath.transform.localToWorldMatrix;
			for (int i = 0; i < currentPath.nodes.Length; i++)
			{
				if (i < currentPath.nodes.Length-1)
				{
					Handles.DrawLine(currentPath.nodes[i].point, currentPath.nodes[i+1].point);

                    /*
					if (currentPath.boxExtrude != 0f)
					{
						Vector3 diff = currentPath.points[i+1] - currentPath.points[i];
						diff.Normalize();
						diff = new Vector3(diff.y, -diff.x, 0f);
						Handles.DrawLine(currentPath.points[i] + diff * currentPath.boxExtrude,
						                currentPath.points[i+1] + diff * currentPath.boxExtrude);
					}
                    */
				}
				Handles.CircleCap(0, currentPath.nodes[i].point, Quaternion.identity, HandleUtility.GetHandleSize(currentPath.nodes[i].point) * 0.1f);
			}

			HandleUtility.Repaint();
			if (GUI.changed)
				EditorUtility.SetDirty(target);
		}
	}

    void Recenter()
    {
        if (currentPath.nodes.Length > 0)
        {
            Undo.RegisterFullObjectHierarchyUndo(currentPath, "Recenter Points");
            var averagePoint = Vector3.zero;
            for (int i = 0; i < currentPath.nodes.Length; i++)
            {
                averagePoint += currentPath.transform.TransformPoint(currentPath.nodes[i].point);
            }
            averagePoint /= (float)currentPath.nodes.Length;
            var wantPoint = averagePoint + Vector3.down * .5f;
            var diff = currentPath.transform.position - wantPoint;
            for (int i = 0; i < currentPath.nodes.Length; i++)
            {
                currentPath.nodes[i].point += Vector3.Scale(diff, new Vector3(1f/currentPath.transform.localScale.x, 1f/currentPath.transform.localScale.y, 1f/currentPath.transform.localScale.z));
            }
            currentPath.transform.position = wantPoint;
        }
    }

	bool IsHovering(Vector3 point)
	{
		return Vector3.Distance(mousePosition, point) < HandleUtility.GetHandleSize(point) * clickRadius;
	}
}
