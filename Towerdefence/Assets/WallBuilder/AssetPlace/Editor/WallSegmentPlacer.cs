using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(WallPainter))]
public class WallSegmentPlacer : Editor
{
	static WallPainter wallPainter = null;
	static Camera cam = null;

	private float planeOffset = 0.0f;

	private bool isHittingPlane = false;
	private Vector3 planeHitPos = Vector3.zero;

	public override void OnInspectorGUI()
	{
		if (Selection.activeGameObject != null)
			wallPainter = Selection.activeGameObject.GetComponent<WallPainter>();
		else
			wallPainter = null;

		if (wallPainter == null || Selection.count > 1)
		{
			Debug.LogError("Select only one asset when editing walls.");
			return;
		}

		base.OnInspectorGUI();

		planeOffset = EditorGUILayout.Slider("Plane offset Y ", wallPainter.yGridPos, -10f, 10f);
		//planeOffset = EditorGUILayout.FloatField("Plane offset Y: ", wallPainter.yOffset);
		wallPainter.SetYOffset(planeOffset);

		wallPainter.drawGrid = EditorGUILayout.Toggle("Draw grid gizmo ", wallPainter.drawGrid);

		wallPainter.gridSize = Mathf.Abs(EditorGUILayout.IntField("Grid dimensions ", wallPainter.gridSize));

		
	}

	private void OnSceneGUI()
	{

		if (Selection.activeGameObject != null)
			wallPainter = Selection.activeGameObject.GetComponent<WallPainter>();
		else
			wallPainter = null;
		cam = Camera.current;

		if(wallPainter != null)
		{
			Vector3 pos = Vector3.up * planeOffset;
			wallPainter.UpdatePlane();

			SceneView.duringSceneGui += UpdateRaycast;
		}
		else
		{
			Debug.Log("No wall selected");
		}

		if (isHittingPlane)
		{
			Vector3 handlePos = planeHitPos;
			handlePos.y = wallPainter.yGridPos;
			Handles.DrawWireDisc(handlePos, Vector3.up, 0.5f, 0.1f);
		}

		SceneView.RepaintAll();
	}

	private void UpdateRaycast(SceneView sceneView)
	{
		if(wallPainter == null)
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

		if(wallNodes.Count > 0)
		{
			Debug.Log("hit a wall");
			DoPlaneHit(wallNodes[0].transform.position);
		}
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
		planeHitPos = SnapVector3(pos, 0.25f);
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
}