using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomEditor(typeof(WallPainter))]
public class WallBuilderEditorWindow : EditorWindow
{
	private enum WallBuilderToolBrushes
	{
		None,
		WallLine,
		WallBox,
		RemoveWallLine
	}


	static WallPainter wallPainter = null;
	static Camera cam = null;

	private bool isHittingPlane = false;
	private Vector3 planeHitPos = Vector3.zero;

	bool isPlacing = false;
	private bool isToolActive = false;

	private float planeOffset = 0.0f;

	private Vector3 dragStartPos = new Vector3();
	private Vector3 newSnappedPos = new Vector3();
	private Vector3 dragEndPos = new Vector3();

	private WallBuilderToolBrushes currentBrush = WallBuilderToolBrushes.None;

	#region General editor window implementation
	[MenuItem("Asset builder tool/Editor window")]
	public static void ShowWindow()
	{
		EditorWindow.GetWindow(typeof(WallBuilderEditorWindow));
	}

	// Subscribes OnSceneGUI to scene gui upate cycle
	void OnFocus()
	{
		SceneView.duringSceneGui -= this.OnSceneGUI;
		SceneView.duringSceneGui += this.OnSceneGUI;
	}

	void OnDestroy()
	{
		SceneView.duringSceneGui -= this.OnSceneGUI;
	}
	#endregion

	// Same as update() when  window is open
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
		if (GUILayout.Button("Toggle gizmo grid " + ((wallPainter.drawGrid) ? "off" : "on")))
		{
			wallPainter.SetDrawGrid(!wallPainter.drawGrid);
		}

		planeOffset = EditorGUILayout.Slider("Plane offset Y ", wallPainter.yGridPos, -10f, 10f);
		wallPainter.SetYOffset(planeOffset);


		wallPainter.gridSize = Mathf.Abs(EditorGUILayout.IntField("Grid dimensions ", wallPainter.gridSize));
		EditorGUI.indentLevel--;

		EditorGUILayout.EndVertical();

		EditorGUILayout.BeginVertical();
		{
			EditorGUI.indentLevel++;


			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			EditorGUILayout.LabelField("Current tool: ", EditorStyles.boldLabel);
			EditorGUILayout.LabelField(currentBrush.ToString(), EditorStyles.boldLabel);
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			EditorGUI.indentLevel++;

			EditorGUILayout.BeginHorizontal();
			{ 
			GUILayout.FlexibleSpace();

				EditorGUI.BeginDisabledGroup(currentBrush == WallBuilderToolBrushes.None);
				{
					if (GUILayout.Button("None", GUILayout.MinWidth(50), GUILayout.MaxWidth(100)))
						SetTool(WallBuilderToolBrushes.None);
				}
				EditorGUI.EndDisabledGroup();

				EditorGUI.BeginDisabledGroup(currentBrush == WallBuilderToolBrushes.RemoveWallLine);
				{
					if (GUILayout.Button("Erase", GUILayout.MinWidth(50), GUILayout.MaxWidth(100)))
						SetTool(WallBuilderToolBrushes.RemoveWallLine);
				}
				EditorGUI.EndDisabledGroup();
				GUILayout.FlexibleSpace();

				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();

				EditorGUI.BeginDisabledGroup(currentBrush == WallBuilderToolBrushes.WallLine);
				{
					if (GUILayout.Button("Wall line", GUILayout.MinWidth(50), GUILayout.MaxWidth(100)))
						SetTool(WallBuilderToolBrushes.WallLine);
				}
				EditorGUI.EndDisabledGroup();

				EditorGUI.BeginDisabledGroup(currentBrush == WallBuilderToolBrushes.WallBox);
				{
					if (GUILayout.Button("Wall box", GUILayout.MinWidth(50), GUILayout.MaxWidth(100)))
						SetTool(WallBuilderToolBrushes.WallBox);
				}
				EditorGUI.EndDisabledGroup();
				EditorGUI.indentLevel--;
				EditorGUI.indentLevel--;
			GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndVertical();
	}

	private void OnSceneGUI(SceneView sceneView)
	{
		if (!isToolActive)
			return;
		wallPainter = GameObject.FindObjectOfType<WallPainter>();
		cam = Camera.current;

		if (isToolActive)
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
				if (currentBrush != WallBuilderToolBrushes.None)
					DrawCircleHandle();
			}

			if (Event.current.isMouse && Event.current.button == 0)
			{
				switch (Event.current.type)
				{
					case EventType.MouseDown:
						if (currentBrush != WallBuilderToolBrushes.None)
						{
							dragStartPos = GetSnappedHandlePos();
							if (currentBrush == WallBuilderToolBrushes.WallLine || currentBrush == WallBuilderToolBrushes.RemoveWallLine)
								newSnappedPos = SnapVector3ToAxis(dragStartPos, GetSnappedHandlePos());
							else if (currentBrush == WallBuilderToolBrushes.WallBox)
								newSnappedPos = GetSnappedHandlePos();
							isPlacing = true;
						}
						Event.current.Use();
						break;
					case EventType.MouseUp:
						if (currentBrush != WallBuilderToolBrushes.None)
						{
							isPlacing = false;
							dragEndPos = newSnappedPos;

							if (currentBrush == WallBuilderToolBrushes.WallBox)
								OnEndDragWallBox();
							else if (currentBrush == WallBuilderToolBrushes.WallLine)
								OnEndDragWallLine();
							else if (currentBrush == WallBuilderToolBrushes.RemoveWallLine)
								OnEndDragRemoveWallLine();

						}
						Event.current.Use();
						break;
					case EventType.MouseDrag:
						if (currentBrush == WallBuilderToolBrushes.WallLine || currentBrush == WallBuilderToolBrushes.RemoveWallLine)
						{
							newSnappedPos = SnapVector3ToAxis(dragStartPos, GetSnappedHandlePos());
						}
						else if (currentBrush == WallBuilderToolBrushes.WallBox)
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

		if (isPlacing)
		{
			Handles.color = Color.red;
			Handles.DrawSolidDisc(newSnappedPos, Vector3.up, 0.1f);

			if (currentBrush == WallBuilderToolBrushes.WallLine)
				DrawLineSelection(dragStartPos, newSnappedPos, Color.green);
			else if (currentBrush == WallBuilderToolBrushes.RemoveWallLine)
				DrawLineSelection(dragStartPos, newSnappedPos, Color.red);
			else if (currentBrush == WallBuilderToolBrushes.WallBox)
				DrawBoxSelection(dragStartPos, newSnappedPos);

		}
		SceneView.RepaintAll();
	}

	private void OnEndDragRemoveWallLine()
	{
		List<WallNode> nodes = new List<WallNode>();

		List<Vector3> nodePositions = GetWallNodeListHalfStep(dragStartPos, dragEndPos);

		if(dragStartPos == dragEndPos)
		{
			Debug.DrawRay(dragStartPos, -Vector3.forward * 0.5f, Color.blue, 3f);
			Debug.DrawRay(dragStartPos, -Vector3.right * 0.5f, Color.blue, 3f);
			RaycastHit hit;
			if (Physics.Raycast(dragStartPos, -Vector3.forward, out hit, 0.5f, wallPainter.rayFloorMask))
			{
				WallNode n = GetWallNodeAtPos(hit.point);
				n.SetWallToNone(SnapAxis.Z);
			}
			if (Physics.Raycast(dragStartPos, -Vector3.right, out hit, 0.5f, wallPainter.rayFloorMask))
			{ 
				WallNode n2 = GetWallNodeAtPos(hit.point);
				n2.SetWallToNone(SnapAxis.X);
			}
		}
		else
		{
			SnapAxis axis = GetSnapAxis(dragStartPos, dragEndPos);
			Vector3 directional = (axis == SnapAxis.Z) ? -Vector3.forward : -Vector3.right;

			foreach (Vector3 pos in nodePositions)
			{
				Debug.DrawRay(pos, directional * 0.5f, Color.red, 3f);

				RaycastHit hit;
				if(Physics.Raycast(pos, directional.normalized, out hit, 0.5f, wallPainter.rayFloorMask))
				{
					nodes.Add(GetWallNodeAtPos(hit.point));
				}
			}
			// check all half positions, check for nodes in negative X and Z direction based on directional
			// set positive wall nodes to none
			foreach (var node in nodes)
			{
				node.SetWallToNone(GetSnapAxis(dragStartPos, dragEndPos));
			}
		}

	}

	private void OnEndDragWallLine()
	{
		PlaceWallNodesLine(dragStartPos, dragEndPos);
		EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

		dragEndPos = new Vector3();
		dragStartPos = new Vector3();
	}
	private void OnEndDragWallBox()
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

	private void SetTool(WallBuilderToolBrushes tool)
	{
		currentBrush = tool;
		isPlacing = false;
	}

	#region Raycast implementation
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
	#endregion

	#region Supporter functions
	private bool CanPlaceWallNode(Vector3 pos)
	{
		Collider[] cols = Physics.OverlapSphere(pos + Vector3.up * 0.11f, 0.05f, wallPainter.rayFloorMask);
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
		Collider[] cols = Physics.OverlapSphere(pos + Vector3.up * 0.11f, 0.05f, wallPainter.rayFloorMask);
		foreach (var item in cols)
		{
			if (item.transform.gameObject.GetComponentInParent<WallNode>())
				return item.transform.gameObject.GetComponentInParent<WallNode>();
		}
		return null;
	}

	

	private List<Vector3> GetWallNodeList(Vector3 A, Vector3 B)
	{
		List<Vector3> wallSegmentNodes = new List<Vector3>();

		Vector3 directional = B - A;
		float distance = directional.magnitude;
		bool isHalfWall = distance % 1 != 0;
		int segmentCount = Mathf.RoundToInt((isHalfWall) ? distance - 0.5f : distance);

		Vector3 placePos = A;
		for (int i = 0; i < segmentCount; i++)
		{
			wallSegmentNodes.Add(placePos);
			placePos += directional.normalized;
		}
		if (isHalfWall)
		{
			wallSegmentNodes.Add(placePos);
		}

		wallSegmentNodes.Add(B);

		if(wallSegmentNodes.Count > 1)
		{
			SnapAxis axis = GetSnapAxis(A, B);

			if(axis == SnapAxis.X)
				wallSegmentNodes = wallSegmentNodes.OrderBy(x => x.z).ToList();
			else
				wallSegmentNodes = wallSegmentNodes.OrderBy(x => x.x).ToList();

		}

		return wallSegmentNodes;
	}

	private List<Vector3> GetWallNodeListHalfStep(Vector3 A, Vector3 B)
	{
		List<Vector3> wallSegmentNodes = new List<Vector3>();

		Vector3 directional = B - A;
		float distance = directional.magnitude;
		bool isHalfWall = distance % 1 != 0;
		int segmentCount = Mathf.RoundToInt(distance * 2f);

		Vector3 placePos = A;
		for (int i = 0; i < segmentCount; i++)
		{
			wallSegmentNodes.Add(placePos);
			placePos += directional.normalized * 0.5f;
		}

		wallSegmentNodes.Add(B);

		if (wallSegmentNodes.Count > 1)
		{
			SnapAxis axis = GetSnapAxis(A, B);

			if (axis == SnapAxis.X)
				wallSegmentNodes = wallSegmentNodes.OrderBy(x => x.z).ToList();
			else
				wallSegmentNodes = wallSegmentNodes.OrderBy(x => x.x).ToList();

		}

		return wallSegmentNodes;
	}

	private List<WallNode> PlaceWallNodes(Vector3 A, Vector3 B)
	{
		List<WallNode> placedNodes = new List<WallNode>();

		Vector3 AB = A;
		AB.z = B.z;

		Vector3 BA = A;
		BA.x = B.x;

		placedNodes.AddRange(PlaceWallNodesLine(A, AB));
		placedNodes.AddRange(PlaceWallNodesLine(A, BA));
		placedNodes.AddRange(PlaceWallNodesLine(B, AB));
		placedNodes.AddRange(PlaceWallNodesLine(B, BA));

		return placedNodes;
	}

	private List<WallNode> PlaceWallNodesLine(Vector3 A, Vector3 B)
	{
		List<WallNode> placedNodes = new List<WallNode>();
		Vector3 AB = A;
		AB.z = B.z;

		Vector3 BA = A;
		BA.x = B.x;

		Vector3 directional = B - A;
		float distance = directional.magnitude;

		List<Vector3> allPositions = new List<Vector3>();
		allPositions.AddRange(GetWallNodeList(A, B));

		int no = 0;
		int no2 = 0;
		foreach (Vector3 pos in allPositions)
		{
			if (CanPlaceWallNode(pos))
			{
				// node is surrounded at positions
				if(Physics.Raycast(pos, directional, 0.5f, wallPainter.rayFloorMask) && Physics.Raycast(pos, -directional, 0.5f, wallPainter.rayFloorMask))
				{
					WallNode node = GetWallNodeAtPos(pos);
					if (node != null)
					{
						no2++;
						placedNodes.Add(node);
					}
				}
				else
				{
					no++;
					Object go = PrefabUtility.InstantiatePrefab(wallPainter.wallNodePrefab, wallPainter.transform);

					WallNode tr = wallPainter.GetComponentInChildren<WallNode>();
					Undo.RegisterCreatedObjectUndo(tr.gameObject, $"placed wall line from {dragStartPos} to {dragEndPos}");
					placedNodes.Add(tr);
					tr.transform.position = pos;
					tr.transform.parent = null;
				}
			}
			else
			{
				WallNode node = GetWallNodeAtPos(pos);
				if (node != null)
				{
					no2++;
					placedNodes.Add(node);
				}
			}
		}
		Debug.Log($"Succesfully placed {no} nodes. Failed to place {allPositions.Count - no}. Added {no2} existing nodes to list.");

		for (int i = 0; i < placedNodes.Count - 1; i++)
		{
			placedNodes[i].SetupWallFirstTime(GetSnapAxis(A, B));
		}
		return placedNodes;

	}

	private Vector3 GetSnappedHandlePos()
	{
		Vector3 handlePos = planeHitPos;
		handlePos.y = wallPainter.yGridPos;

		return handlePos;
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
		float ang = Mathf.Round((Mathf.Atan2(currentPos.z - startPos.z, currentPos.x - startPos.x) * Mathf.Rad2Deg));
		if (Mathf.Abs(ang) > 45 && Mathf.Abs(ang) <= 135)
			return SnapAxis.X;
		else
			return SnapAxis.Z;
	}
	#endregion

	#region Handles
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

	private void DrawLineSelection(Vector3 A, Vector3 B, Color col, float thickness = 0.2f)
	{
		Color c = Handles.color;
		Handles.color = Color.yellow;
		Handles.Label((A + B) * 0.5f + Vector3.up * 0.25f, Vector3.Distance(A, B).ToString());
		DrawDummyWall(A, B, col);
		Handles.color = c;
	}

	private void DrawDummyWall(Vector3 A, Vector3 B, Color color, float height = 3f)
	{
		Vector3 directional = B - A;
		float distance = directional.magnitude;
		bool isHalfWall = distance % 1 != 0;
		int segmentCount = Mathf.RoundToInt((isHalfWall) ? distance - 0.5f : distance);

		Vector3 placePos = A;
		for (int i = 0; i < segmentCount; i++)
		{
			Vector3 localTarget = placePos + directional.normalized;

			DrawWallSegment(placePos, localTarget, color, height);
			placePos += directional.normalized;
		}
		if (isHalfWall)
		{
			DrawWallSegment(placePos, B, color, height);
		}
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


		string selectionType = (currentBrush == WallBuilderToolBrushes.WallBox) ? "box" : "line";
		string text = (isPlacing) ? $"Release to place {selectionType} selection end" : $"Click and drag to place {selectionType} selection start";
		Handles.Label(handlePos + Vector3.up * 0.5f, text);

		Handles.color = new Color();
	}
	#endregion
}