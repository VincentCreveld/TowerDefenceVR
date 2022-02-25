using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(WallPainter))]
public class WallBuilderEditorWindow : EditorWindow
{
	private enum SelectionSnap
	{
		Full,
		Half,
		Quarter
	}

	static WallPainter wallPainter = null;
	static Camera cam = null;

	private bool isHittingPlane = false;
	private Vector3 planeHitPos = Vector3.zero;

	bool isPlacing = false;
	private bool claimFocus = false;

	private bool IsMouseDownThisFrame => Event.current.type == EventType.MouseDown && Event.current.button == 0;
	private bool IsMouseUpThisFrame => Event.current.type == EventType.MouseUp && Event.current.button == 0;
	private float planeOffset = 0.0f;
	[HideInInspector] public bool wantsToPlace = false;

	[MenuItem("Asset builder tool/Editor window")]
	public static void ShowWindow()
	{
		EditorWindow.GetWindow(typeof(WallBuilderEditorWindow));
	}


	// Window has been selected
	void OnFocus()
	{
		// Remove delegate listener if it has previously
		// been assigned.
		SceneView.duringSceneGui -= this.OnSceneGUI;
		// Add (or re-add) the delegate.
		SceneView.duringSceneGui += this.OnSceneGUI;
	}

	void OnDestroy()
	{
		// When the window is destroyed, remove the delegate
		// so that it will no longer do any drawing.
		SceneView.duringSceneGui -= this.OnSceneGUI;
	}

	public void OnGUI()
	{
		wallPainter = GameObject.FindObjectOfType<WallPainter>();

		if (wallPainter == null || Selection.count > 1)
		{
			Debug.LogError("Select only one asset when editing walls.");
			return;
		}

		EditorGUILayout.BeginVertical();
		claimFocus = EditorGUILayout.Toggle("Prevent focus loss ", claimFocus);

		EditorGUILayout.LabelField("Plane display variables", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		if(GUILayout.Button("Toggle gizmo grid " + ((wallPainter.drawGrid) ? "off" : "on")))
		{
			wallPainter.SetDrawGrid(!wallPainter.drawGrid);
		}

		planeOffset = EditorGUILayout.Slider("Plane offset Y ", wallPainter.yGridPos, -10f, 10f);
		//planeOffset = EditorGUILayout.FloatField("Plane offset Y: ", wallPainter.yOffset);
		wallPainter.SetYOffset(planeOffset);


		wallPainter.gridSize = Mathf.Abs(EditorGUILayout.IntField("Grid dimensions ", wallPainter.gridSize));
		EditorGUI.indentLevel--;

		EditorGUILayout.EndVertical();

		string buttonText = (!wantsToPlace) ? "Place wall node" : "Stop placing wall node";
		if(GUILayout.Button(buttonText))
		{
			wantsToPlace = !wantsToPlace;
			isPlacing = false;
			dragStartPos = new Vector3();
		}
	}

	private Vector3 dragStartPos = new Vector3();
	private SnapAxis snapDir = SnapAxis.X;
	private Vector3 newSnappedPos = new Vector3();
	private Vector3 dragEndPos = new Vector3();
	private void OnSceneGUI(SceneView sceneView)
	{
		wallPainter = GameObject.FindObjectOfType<WallPainter>();
		cam = Camera.current;

		if(claimFocus)
		{
			int id = GUIUtility.GetControlID(FocusType.Passive);
			HandleUtility.AddDefaultControl(id);
			Tools.current = Tool.None;
		}

		if (wallPainter != null)
		{
			Vector3 pos = Vector3.up * planeOffset;
			wallPainter.UpdatePlane();

			SceneView.duringSceneGui += UpdateRaycast;
		}

		if (isHittingPlane)
		{
			if (wallPainter != null)
			{
				if(wantsToPlace)
					DrawCircleHandle();
			}

			if (Event.current.isMouse && Event.current.button == 0)
			{
				switch (Event.current.type)
				{
					case EventType.MouseDown:
						if (wantsToPlace)
						{
							dragStartPos = GetSnappedHandlePos();
							newSnappedPos = SnapVector3ToAxis(dragStartPos, GetSnappedHandlePos());
							isPlacing = true;
						}
						Event.current.Use();
						break;
					case EventType.MouseUp:
						if (wantsToPlace)
						{
							isPlacing = false;
							dragEndPos = newSnappedPos;
							Debug.DrawLine(dragStartPos, dragEndPos, Color.red, 3f);
							dragEndPos = new Vector3();
							dragStartPos = new Vector3();
						}
						Event.current.Use();
						break;
					case EventType.MouseDrag:
						newSnappedPos = SnapVector3ToAxis(dragStartPos, GetSnappedHandlePos());
						Event.current.Use();
						break;
					case EventType.DragUpdated:
					case EventType.DragPerform:
					case EventType.DragExited:
						Debug.Log(Event.current.type);
						Event.current.Use();
						break;
				}
			}
		}

		if(isPlacing)
		{
			Handles.color = Color.red;
			Handles.DrawSolidDisc(newSnappedPos, Vector3.up, 0.1f);
		}

		SceneView.RepaintAll();
	}

	private void DrawCircleHandle()
	{
		Vector3 handlePos = GetSnappedHandlePos();
		Handles.color = (isPlacing) ? Color.blue : Color.yellow;
		Handles.DrawWireDisc(handlePos, Vector3.up, 0.5f, 0.1f);
		Handles.DrawSolidDisc(handlePos, Vector3.up, 0.1f);

		string text = (isPlacing) ? "Click to place wall end": "Click to place wall start";
		Handles.Label(handlePos + Vector3.up * 0.5f, text + " " + GetSnappedHandlePos());

		Handles.color = new Color();
	}

	private Vector3 GetSnappedHandlePos()
	{
		Vector3 handlePos = planeHitPos;
		handlePos.y = wallPainter.yGridPos;
		return handlePos;
	}

	private void UpdateRaycast(SceneView sceneView)
	{
		if (wallPainter == null)
		{
			SceneView.duringSceneGui -= UpdateRaycast;
			return;
		}

		Vector3 mousePosition = Event.current.mousePosition;
		mousePosition.y = SceneView.currentDrawingSceneView.camera.pixelHeight - mousePosition.y;
		mousePosition = SceneView.currentDrawingSceneView.camera.ScreenToWorldPoint(mousePosition);
		mousePosition.y = -mousePosition.y;

		Ray mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
		List<Vector3> raycastHits = Physics.RaycastAll(mouseRay, float.PositiveInfinity, wallPainter.rayFloorMask)
			.Select(hit => hit.point)
			.ToList();
		if (raycastHits.Count > 0)
			DoPlaneHit(raycastHits[0]);
		else
			isHittingPlane = false;

		List<WallNode> wallNodes = Physics.RaycastAll(mouseRay, float.PositiveInfinity, wallPainter.rayFloorMask)
			.Select(hit => hit.transform.gameObject.GetComponentInParent<WallNode>())
			.ToList();
		wallNodes = wallNodes.Where(item => item != null).ToList();

		if (wallNodes.Count > 0)
			DoPlaneHit(wallNodes[0].transform.position);
		else if (raycastHits.Count > 0)
			DoPlaneHit(raycastHits[0]);
		else
			isHittingPlane = false;

		SceneView.duringSceneGui -= UpdateRaycast;
	}

	private void DoPlaneHit(Vector3 pos)
	{
		if (wallPainter == null)
			return;

		isHittingPlane = true;
		planeHitPos = SnapVector3(pos, 0.5f);
	}

	private Vector3 SnapVector3(Vector3 pos, float f)
	{
		Vector3 p = new Vector3(
		Mathf.Round(pos.x / f) * f,
		Mathf.Round(pos.y / f) * f,
		Mathf.Round(pos.z / f) * f
			);
		return p;
	}

	private Vector3 SnapVector3ToAxis(Vector3 startPos, Vector3 pos)
	{
		Vector3 returnVal = pos;
		SnapAxis axis = GetSnapAxis(startPos, pos);

		switch (axis)
		{
			case SnapAxis.X:
				returnVal.x = startPos.x;
				break;
			case SnapAxis.Y:
				returnVal.y = startPos.y;
				break;
			case SnapAxis.Z:
				returnVal.z = startPos.z;
				break;
			default:
				break;
		}

		return returnVal;
	}

	private SnapAxis GetSnapAxis(Vector3 startPos, Vector3 currentPos)
	{
		float ang = Mathf.Round((Mathf.Atan2(dragStartPos.z - GetSnappedHandlePos().z, dragStartPos.x - GetSnappedHandlePos().x) * Mathf.Rad2Deg));
		if (Mathf.Abs(ang) > 45 && Mathf.Abs(ang) <= 135)
			return SnapAxis.X;
		else
			return SnapAxis.Z;
	}
}