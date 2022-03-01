using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(WallPainter))]
public class WallBuilderEditorWindow : EditorWindow
{
	static WallPainter wallPainter = null;
	static Camera cam = null;

	private bool isHittingPlane = false;
	private Vector3 planeHitPos = Vector3.zero;

	bool isPlacing = false;
	private bool isBoxSelection = false;
	private bool isToolActive = false;

	private bool IsMouseDownThisFrame => Event.current.type == EventType.MouseDown && Event.current.button == 0;
	private bool IsMouseUpThisFrame => Event.current.type == EventType.MouseUp && Event.current.button == 0;
	private float planeOffset = 0.0f;
	[HideInInspector] public bool wantsToPlace = false;

	private Vector3 dragStartPos = new Vector3();
	private Vector3 newSnappedPos = new Vector3();
	private Vector3 dragEndPos = new Vector3();

	[MenuItem("Asset builder tool/Editor window")]
	public static void ShowWindow()
	{
		EditorWindow.GetWindow(typeof(WallBuilderEditorWindow));
	}

	void OnFocus()
	{
		SceneView.duringSceneGui -= this.OnSceneGUI;
		SceneView.duringSceneGui += this.OnSceneGUI;
	}

	void OnDestroy()
	{
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
		isToolActive = EditorGUILayout.Toggle("Prevent focus loss ", isToolActive);

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
		if(wantsToPlace)
		{
			string selectionTypeText = (isBoxSelection) ? "Current tool: box selection" : "Current tool: line selection";
			if (GUILayout.Button(selectionTypeText))
			{
				isBoxSelection = !isBoxSelection;
				dragStartPos = new Vector3();
				dragEndPos = new Vector3();
			}
		}

	}

	
	private void OnSceneGUI(SceneView sceneView)
	{
		if (!isToolActive)
			return;
		wallPainter = GameObject.FindObjectOfType<WallPainter>();
		cam = Camera.current;

		if(isToolActive)
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
							if (!isBoxSelection)
								newSnappedPos = SnapVector3ToAxis(dragStartPos, GetSnappedHandlePos());
							else
								newSnappedPos = GetSnappedHandlePos();
							isPlacing = true;
						}
						Event.current.Use();
						break;
					case EventType.MouseUp:
						if (wantsToPlace)
						{
							isPlacing = false;
							dragEndPos = newSnappedPos;

							if(isBoxSelection)
								OnEndDragBoxSelection();
							else
								OnEndDrag();

						}
						Event.current.Use();
						break;
					case EventType.MouseDrag:
						if (!isBoxSelection)
						{
							newSnappedPos = SnapVector3ToAxis(dragStartPos, GetSnappedHandlePos());
						}
						else
						{
							newSnappedPos = GetSnappedHandlePos();
						}

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

			if (!isBoxSelection)
				DrawLineSelection(dragStartPos, newSnappedPos);
			else
				DrawBoxSelection(dragStartPos, newSnappedPos);

		}
		SceneView.RepaintAll();
	}

	private void OnEndDrag()
	{
		//PlaceWallNodesLine(dragStartPos, dragEndPos);

		//ConnectWallNodes(PlaceWallNodesLine(dragStartPos, dragEndPos), GetSnapAxis(dragStartPos, dragEndPos));
		PlaceWallNodesLine(dragStartPos, dragEndPos);
		ProcWallNodesInLine(dragStartPos, dragEndPos);
		EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

		dragEndPos = new Vector3();
		dragStartPos = new Vector3();
	}

	private List<WallNode> ProcWallNodesInLine(Vector3 A, Vector3 B)
	{
		List<WallNode> foundNodes = new List<WallNode>();

		Vector3 directional = B - A;
		int halfSegmentCount = Mathf.RoundToInt(directional.magnitude / 0.5f);

		Vector3 curCheckPos = A;
		for (int i = 0; i < halfSegmentCount; i++)
		{
			WallNode node = GetWallNodeAtPos(curCheckPos);
			if(node != null)
				foundNodes.Add(node);
			curCheckPos += directional.normalized * 0.5f;
		}

		SnapAxis axis = GetSnapAxis(A, B);

		foreach (var item in foundNodes)
		{
			item.SetupWallFirstTime(axis);
		}

		return foundNodes;
	}

	private void ConnectWallNodes(List<WallNode> nodes, SnapAxis axis)
	{
		foreach (WallNode node in nodes)
		{
			node.SetupWallFirstTime(axis);
		}
	}	

	private void OnEndDragBoxSelection()
	{
		DrawBoxSelection(dragStartPos, dragEndPos);

		if(dragStartPos.x == dragEndPos.x || dragStartPos.z == dragEndPos.z)
			PlaceWallNodesLine(dragStartPos, dragEndPos);
		else
			PlaceWallNodes(dragStartPos, dragEndPos);
		EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

		dragEndPos = new Vector3();
		dragStartPos = new Vector3();
	}

	private bool CanPlaceWallNode(Vector3 pos)
	{
		Collider[] cols = Physics.OverlapSphere(pos, 0.25f, wallPainter.rayFloorMask);
		bool canPlace = true;
		foreach (var item in cols)
		{
			if (item.transform.gameObject.GetComponentInParent<WallNode>())
				return false;
		}
		return canPlace;
	}

	private WallNode GetWallNodeAtPos(Vector3 pos)
	{
		Collider[] cols = Physics.OverlapSphere(pos, 0.25f, wallPainter.rayFloorMask);
		foreach (var item in cols)
		{
			if (item.transform.gameObject.GetComponentInParent<WallNode>())
				return item.transform.gameObject.GetComponentInParent<WallNode>();
		}
		return null;
	}

	private void DrawBoxSelection(Vector3 A, Vector3 B, float thickness = 0.2f)
	{
		Color c = Handles.color;
		Handles.color = Color.yellow;
		Vector3 AB = dragStartPos;
		AB.z = B.z;

		Vector3 BA = dragStartPos;
		BA.x = B.x;
		Color cc = Color.green;
		Handles.DrawSolidRectangleWithOutline(new Vector3[] { A, BA, B, AB }, (new Color(cc.b, cc.g, cc.b, 0.25f) + Color.black) * 0.5f, cc);

		Handles.Label((A + AB) * 0.5f + Vector3.up * 0.25f, Vector3.Distance(A, AB).ToString(), EditorStyles.boldLabel);
		Handles.Label((A + BA) * 0.5f + Vector3.up * 0.25f, Vector3.Distance(A, BA).ToString(), EditorStyles.boldLabel);
		Handles.Label((AB + B) * 0.5f + Vector3.up * 0.25f, Vector3.Distance(AB, B).ToString(), EditorStyles.boldLabel);
		Handles.Label((BA + B) * 0.5f + Vector3.up * 0.25f, Vector3.Distance(BA, B).ToString(), EditorStyles.boldLabel);

		DrawDummyWall(A, AB, Handles.color);
		DrawDummyWall(A, BA, Handles.color);
		DrawDummyWall(B, AB, Handles.color);
		DrawDummyWall(B, BA, Handles.color);

		Handles.color = c;
	}

	private void DrawLineSelection(Vector3 A, Vector3 B, float thickness = 0.2f)
	{
		Color c = Handles.color;
		Handles.color = Color.yellow;
		Handles.Label((A + B) * 0.5f + Vector3.up * 0.25f, Vector3.Distance(A, B).ToString());
		DrawDummyWall(A, B, Handles.color);
		Handles.color = c;
	}

	private void DrawDummyWall(Vector3 A, Vector3 B, Color color, float height = 3f)
	{
		//SnapAxis axis = SnapAxis.None;

		
		Vector3 directional = B - A;
		float distance = directional.magnitude;
		bool isHalfWall = distance % 1 != 0;
		int segmentCount = Mathf.RoundToInt((isHalfWall) ? distance - 0.5f : distance);

		Vector3 placePos = A;
		for (int i = 0; i < segmentCount; i++)
		{
			Vector3 localTarget = placePos + directional.normalized;

			Handles.Label(placePos + Vector3.up * 0.25f, (CanPlaceWallNode(placePos) ? "not overlapping" : "overlapping"));

			DrawWallSegment(placePos, localTarget, color, height);
			placePos += directional.normalized;
		}
		if (isHalfWall)
		{
			Handles.Label(placePos + Vector3.up * 0.25f, (CanPlaceWallNode(placePos) ? "not overlapping" : "overlapping"));

			DrawWallSegment(placePos, B, color, height);
		}

		Handles.Label(B + Vector3.up * 0.25f, (CanPlaceWallNode(B) ? "not overlapping" : "overlapping"));

	}

	private List<Vector3> GetWallNodeList(Vector3 A, Vector3 B)
	{
		List<Vector3> wallSegmentNodes = new List<Vector3>();

		Vector3 directional = B - A;
		float distance = directional.magnitude;
		bool isHalfWall = distance % 1 != 0;
		int segmentCount = Mathf.RoundToInt((isHalfWall) ? distance - 0.5f : distance);

		// some ray check to see if walls overlap
		Vector3 placePos = A;
		for (int i = 0; i < segmentCount; i++)
		{
			if(Physics.Raycast(placePos, directional, 0.5f, wallPainter.rayFloorMask) && Physics.Raycast(placePos, -directional, 0.5f, wallPainter.rayFloorMask))
				placePos += directional.normalized;
			else
			{
				wallSegmentNodes.Add(placePos);
				placePos += directional.normalized;
			}
		}
		if (isHalfWall)
		{
			if (!(Physics.Raycast(placePos, directional, 0.5f, wallPainter.rayFloorMask) && Physics.Raycast(placePos, -directional, 0.5f, wallPainter.rayFloorMask)))
				wallSegmentNodes.Add(placePos);
		}

		if (!(Physics.Raycast(placePos, directional, 0.5f, wallPainter.rayFloorMask) && Physics.Raycast(placePos, -directional, 0.5f, wallPainter.rayFloorMask)))
			wallSegmentNodes.Add(B);

		return wallSegmentNodes;
	}

	private List<Transform> PlaceWallNodes(Vector3 A, Vector3 B)
	{
		List<Transform> placedNodes = new List<Transform>();

		Vector3 AB = A;
		AB.z = B.z;

		Vector3 BA = A;
		BA.x = B.x;

		List<Vector3> allPositions = new List<Vector3>();
		allPositions.AddRange(GetWallNodeList(A, AB));
		allPositions.AddRange(GetWallNodeList(A, BA));
		allPositions.AddRange(GetWallNodeList(B, AB));
		allPositions.AddRange(GetWallNodeList(B, BA));

		int no = 0;
		foreach (Vector3 pos in allPositions)
		{
			if (CanPlaceWallNode(pos))
			{
				no++;
				Object go = PrefabUtility.InstantiatePrefab(wallPainter.wallNodePrefab, wallPainter.transform);

				Transform tr = wallPainter.GetComponentInChildren<WallNode>().transform;
				Undo.RegisterCreatedObjectUndo(tr.gameObject, $"placed wall line from {dragStartPos} to {dragEndPos}");
				placedNodes.Add(tr);
				tr.position = pos;
				tr.parent = null;
			}
		}
		Debug.Log($"Succesfully placed {no} nodes. Failed to place {allPositions.Count - no}.");
		return placedNodes;
	}

	private List<WallNode> PlaceWallNodesLine(Vector3 A, Vector3 B)
	{
		List<WallNode> placedNodes = new List<WallNode>();
		Vector3 AB = A;
		AB.z = B.z;

		Vector3 BA = A;
		BA.x = B.x;

		List<Vector3> allPositions = new List<Vector3>();
		allPositions.AddRange(GetWallNodeList(A, B));

		int no = 0;
		foreach (Vector3 pos in allPositions)
		{
			if (CanPlaceWallNode(pos))
			{
				no++;
				Object go = PrefabUtility.InstantiatePrefab(wallPainter.wallNodePrefab, wallPainter.transform);

				WallNode tr = wallPainter.GetComponentInChildren<WallNode>();
				Undo.RegisterCreatedObjectUndo(tr.gameObject, $"placed wall line from {dragStartPos} to {dragEndPos}");
				placedNodes.Add(tr);
				tr.SetupWallFirstTime(GetSnapAxis(A, B));
				tr.transform.position = pos;
				tr.transform.parent = null;
			}
		}
		Debug.Log($"Succesfully placed {no} nodes. Failed to place {allPositions.Count - no}.");
		return placedNodes;

	}

	private void DrawWallSegment(Vector3 A, Vector3 B, Color color, float height = 3f)
	{
		Color c = Handles.color;
		Handles.color = color;
		Color col = color;
		col.a = 0.25f;
		Handles.DrawSolidRectangleWithOutline(new Vector3[] { A, A + Vector3.up * height, B + Vector3.up * height, B }, (col + Color.black) * 0.5f, color);
		Handles.color = c;
	}

	private void DrawCircleHandle()
	{
		Vector3 handlePos = GetSnappedHandlePos();
		Handles.color = (isPlacing) ? Color.blue : Color.yellow;
		Handles.DrawWireDisc(handlePos, Vector3.up, 0.5f, 0.1f);
		Handles.DrawSolidDisc(handlePos, Vector3.up, 0.1f);

		string selectionType = (isBoxSelection) ? "box" : "line";
		string text = (isPlacing) ? $"Release to place {selectionType} selection end": $"Click and drag to place {selectionType} selection start";
		Handles.Label(handlePos + Vector3.up * 0.5f, text);

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