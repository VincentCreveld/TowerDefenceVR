using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WallNode : MonoBehaviour
{
	public enum WallState
	{
		None,
		ShortWall,
		LongWall,
		TSplit,
		XSplit,
		Corner
	}

	public enum WallSegmentLength
	{
		None,
		Half,
		Full,
		Double
	}

	[Header("zPos Wall vars")]
	[SerializeField] private WallSegmentManager zPosWall;
	[SerializeField] private WallNode zPosConnectedNode = null;
	private WallSegmentLength zPosLength = WallSegmentLength.None;
	[SerializeField] private WallSegmentManager.WallSegmentType zPosPreferredWallSegmentType = WallSegmentManager.WallSegmentType.fullWall;

	[Header("xPos Wall vars")]
	[SerializeField] private WallSegmentManager xPosWall;
	[SerializeField] private WallNode xPosConnectedNode = null;
	private WallSegmentLength xPosLength = WallSegmentLength.None;
	[SerializeField] private WallSegmentManager.WallSegmentType xPosPreferredWallSegmentType = WallSegmentManager.WallSegmentType.fullWall;

	[Header("zNeg Wall vars (unused)")]
	[SerializeField] private WallSegmentManager zNegWall;
	[SerializeField] private WallNode zNegConnectedNode = null;
	private WallSegmentLength zNegLength = WallSegmentLength.None;
	private WallSegmentManager.WallSegmentType zNegPreferredWallSegmentType = WallSegmentManager.WallSegmentType.fullWall;

	[Header("xNeg Wall vars (unused)")]
	[SerializeField] private WallSegmentManager xNegWall;
	[SerializeField] private WallNode xNegConnectedNode = null;
	private WallSegmentLength xNegLength = WallSegmentLength.None;
	private WallSegmentManager.WallSegmentType xNegPreferredWallSegmentType = WallSegmentManager.WallSegmentType.fullWall;

	[Header("Misc vars")]
	[SerializeField] private Transform singleCorner;
	[SerializeField] private Transform flatCorner;
	[SerializeField] private Transform lowFlatCorner;
	[SerializeField] private Transform lowSingleCorner;
	[SerializeField] private Transform connectionObject;
	[SerializeField] private LayerMask raycastLayer;
	[SerializeField] private LayerMask connectionObjectLayer;

	private bool isCorner = false;
	public int wallCount = 0;
	public int lowWallCount = 0;
	public int highWallCount = 0;

	public WallState wallState { get; private set; } = WallState.None;

	public void CheckAllCorners()
	{
		connectionObject.transform.localPosition = Vector3.zero;
		transform.rotation = Quaternion.identity;

		Ray zPos = new Ray(connectionObject.position, new Vector3(0, 0, 1));
		Ray zNeg = new Ray(connectionObject.position, new Vector3(0, 0, -1));
		Ray xPos = new Ray(connectionObject.position, new Vector3(1, 0, 0));
		Ray xNeg = new Ray(connectionObject.position, new Vector3(-1, 0, 0));

		RaycastHit hit;
		float rayLength = zPosWall.PreferredWallLength;
		if (rayLength <= 0)
			rayLength = 1f;
		if (Physics.Raycast(zPos, out hit, rayLength, raycastLayer))
		{
			zPosConnectedNode = null;

			zPosLength = WallSegmentLength.None;
			
			if ((1 << hit.transform.gameObject.layer) == connectionObjectLayer)
			{
				zPosConnectedNode = hit.transform.gameObject.GetComponentInParent<WallNode>();
				
				float length = Mathf.RoundToInt(Vector3.Distance(transform.position, hit.transform.position) * 100);
				length = length - (length % 25);

				zPosLength = (length <= zPosWall.WallLength * 100) ? WallSegmentLength.Full : (length == zPosWall.WallLength * 50) ? WallSegmentLength.Half : WallSegmentLength.None;

				zPosWall.WallSpace = length;
			}
		}
		else
		{
			zPosConnectedNode = null;
			zPosLength = WallSegmentLength.None;
		}

		rayLength = zNegWall.PreferredWallLength;
		if (rayLength <= 0)
			rayLength = 1f;
		if (Physics.Raycast(zNeg, out hit, rayLength, raycastLayer))
		{
			zNegConnectedNode = null;
			zNegLength = WallSegmentLength.None;
			
			if ((1 << hit.transform.gameObject.layer) == connectionObjectLayer)
			{
				zNegConnectedNode = hit.transform.gameObject.GetComponentInParent<WallNode>();
				
				float length = Mathf.RoundToInt(Vector3.Distance(transform.position, hit.transform.position) * 100);
				length = length - (length % 25);

				zNegLength = (length <= zNegWall.WallLength * 100) ? WallSegmentLength.Full : (length == zNegWall.WallLength * 50) ? WallSegmentLength.Half : WallSegmentLength.None;

				zNegWall.WallSpace = length;
			}
		}
		else
		{
			zNegConnectedNode = null;
			zNegLength = WallSegmentLength.None;
		}

		rayLength = xPosWall.PreferredWallLength;
		if (rayLength <= 0)
			rayLength = 1f;
		if (Physics.Raycast(xPos, out hit, rayLength, raycastLayer))
		{
			xPosConnectedNode = null;
			xPosLength = WallSegmentLength.None;

			if ((1 << hit.transform.gameObject.layer) == connectionObjectLayer)
			{
				xPosConnectedNode = hit.transform.gameObject.GetComponentInParent<WallNode>();
				
				float length = Mathf.RoundToInt(Vector3.Distance(transform.position, hit.transform.position) * 100);
				length = length - (length % 25);

				xPosLength = (length <= xPosWall.WallLength * 100) ? WallSegmentLength.Full : (length == xPosWall.WallLength * 50) ? WallSegmentLength.Half : WallSegmentLength.None;

				xPosWall.WallSpace = length;
			}
		}
		else
		{
			xPosConnectedNode = null;
			xPosLength = WallSegmentLength.None;
		}

		rayLength = xNegWall.PreferredWallLength;
		if (rayLength <= 0)
			rayLength = 1f;
		if (Physics.Raycast(xNeg, out hit, rayLength, raycastLayer))
		{
			xNegConnectedNode = null;
			xNegLength = WallSegmentLength.None;

			if ((1 << hit.transform.gameObject.layer) == connectionObjectLayer)
			{
				xNegConnectedNode = hit.transform.gameObject.GetComponentInParent<WallNode>();
				
				float length = Mathf.RoundToInt(Vector3.Distance(transform.position, hit.transform.position) * 100);
				length = length - (length % 25);

				xNegLength = (length <= xNegWall.WallLength * 100) ? WallSegmentLength.Full : (length == xNegWall.WallLength * 50) ? WallSegmentLength.Half : WallSegmentLength.None;

				xNegWall.WallSpace = length;
			}
		}
		else
		{
			xNegConnectedNode = null;
			xNegLength = WallSegmentLength.None;
		}
	}

	[ContextMenu("CheckCorners")]
	public void SetupWall()
	{
		zPosWall.SegmentType = zPosPreferredWallSegmentType;
		xPosWall.SegmentType = xPosPreferredWallSegmentType;

		CheckAllCorners();

		// negative walls should never have to be enabled. needs refactor to clean this up

		xPosWall.gameObject.SetActive(xPosConnectedNode != null);
		//xNegWall.gameObject.SetActive(xNegConnectedNode != null && xNegLength == WallSegmentLength.Full && (xNegConnectedNode.xPosConnectedNode !=  null && xNegConnectedNode.xPosConnectedNode != this));
		zPosWall.gameObject.SetActive(zPosConnectedNode != null);
		//zNegWall.gameObject.SetActive(zNegConnectedNode != null && zNegLength == WallSegmentLength.Full && (zNegConnectedNode.xPosConnectedNode != null && zNegConnectedNode.xPosConnectedNode != this));

		SetCorners();
	}

	private void SetCorners()
	{
		wallCount = 0;
		highWallCount = 0;
		highWallCount = (xPosConnectedNode != null) ? highWallCount + 1 : highWallCount;
		highWallCount = (xNegConnectedNode != null) ? highWallCount + 1 : highWallCount;
		highWallCount = (zPosConnectedNode != null) ? highWallCount + 1 : highWallCount;
		highWallCount = (zNegConnectedNode != null) ? highWallCount + 1 : highWallCount;

		lowWallCount = 0;
		lowWallCount = (xPosConnectedNode != null && xPosWall.IsLowWall) ? lowWallCount + 1 : lowWallCount;
		lowWallCount = (xNegConnectedNode != null && (xNegConnectedNode.xPosWall != null && xNegConnectedNode.xPosWall.IsLowWall)) ? lowWallCount + 1 : lowWallCount;
		lowWallCount = (zPosConnectedNode != null && zPosWall.IsLowWall) ? lowWallCount + 1 : lowWallCount;
		lowWallCount = (zNegConnectedNode != null && (zNegConnectedNode.zPosWall != null && zNegConnectedNode.zPosWall.IsLowWall)) ? lowWallCount + 1 : lowWallCount;

		isCorner = false;

		wallCount = highWallCount;
		if (highWallCount >= 2 && lowWallCount >= 1)
			wallCount -= lowWallCount;

		// check if other wall is low. Otherwise apply wall border
		if (wallCount <= 0 || wallCount > 2)
		{
			singleCorner.gameObject.SetActive(false);
			flatCorner.gameObject.SetActive(false);

			switch (wallCount)
			{
				case 3:
					wallState = WallState.TSplit;
					break;
				case 4:
					wallState = WallState.XSplit;
					break;
				default:
					wallState = WallState.None;
					break;
			}
		}
		else if (wallCount == 1)
		{
			singleCorner.gameObject.SetActive(false);
			lowSingleCorner.gameObject.SetActive(false);
			flatCorner.gameObject.SetActive(true);
			lowFlatCorner.gameObject.SetActive(false);

			Vector3 dir = Vector3.zero;
			dir = (xPosConnectedNode != null && (xPosConnectedNode.xNegWall != null && (dir == Vector3.zero || !xPosConnectedNode.xNegWall.IsLowWall))) ? xPosWall.transform.forward : dir;
			dir = (xNegConnectedNode != null && (xNegConnectedNode.xPosWall != null && (dir == Vector3.zero || !xNegConnectedNode.xPosWall.IsLowWall))) ? xNegWall.transform.forward : dir;
			dir = (zPosConnectedNode != null && (zPosConnectedNode.zNegWall != null && (dir == Vector3.zero || !zPosConnectedNode.zNegWall.IsLowWall))) ? zPosWall.transform.forward : dir;
			dir = (zNegConnectedNode != null && (zNegConnectedNode.zPosWall != null && (dir == Vector3.zero || !zNegConnectedNode.zPosWall.IsLowWall))) ? zNegWall.transform.forward : dir;

			WallSegmentManager.WallSegmentType wallType = WallSegmentManager.WallSegmentType.None;
			if(highWallCount > wallCount)
			{
				if (zPosConnectedNode != null)
					wallType = (wallType == WallSegmentManager.WallSegmentType.None || wallType == WallSegmentManager.WallSegmentType.lowWall || wallType == WallSegmentManager.WallSegmentType.halfLowWall) ? (zPosWall.SegmentType != WallSegmentManager.WallSegmentType.None) ? zPosWall.SegmentType : wallType : wallType;

				if (zNegConnectedNode != null)
					wallType = (wallType == WallSegmentManager.WallSegmentType.None || wallType == WallSegmentManager.WallSegmentType.lowWall || wallType == WallSegmentManager.WallSegmentType.halfLowWall) ? (zNegWall.SegmentType != WallSegmentManager.WallSegmentType.None) ? zNegWall.SegmentType : wallType : wallType;

				if (xPosConnectedNode != null)
					wallType = (wallType == WallSegmentManager.WallSegmentType.None || wallType == WallSegmentManager.WallSegmentType.lowWall || wallType == WallSegmentManager.WallSegmentType.halfLowWall) ? (xPosWall.SegmentType != WallSegmentManager.WallSegmentType.None) ? xPosWall.SegmentType : wallType : wallType;

				if (xNegConnectedNode != null)
					wallType = (wallType == WallSegmentManager.WallSegmentType.None || wallType == WallSegmentManager.WallSegmentType.lowWall || wallType == WallSegmentManager.WallSegmentType.halfLowWall) ? (xNegWall.SegmentType != WallSegmentManager.WallSegmentType.None) ? xNegWall.SegmentType : wallType : wallType;
			}
			else
			{
				wallType = (zPosConnectedNode != null) ? zPosWall.SegmentType : wallType;
				wallType = (zNegConnectedNode != null && zNegConnectedNode.zPosWall != null) ? zNegConnectedNode.zPosWall.SegmentType : wallType;
				wallType = (xPosConnectedNode != null) ? xPosWall.SegmentType : wallType;
				wallType = (xNegConnectedNode != null && xNegConnectedNode.xPosWall != null) ? xNegConnectedNode.xPosWall.SegmentType : wallType;
			}	


			bool isLowWall = (wallType == WallSegmentManager.WallSegmentType.halfLowWall || wallType == WallSegmentManager.WallSegmentType.lowWall);
			flatCorner.gameObject.SetActive(!isLowWall);
			lowFlatCorner.gameObject.SetActive(isLowWall);

			flatCorner.forward = -dir.normalized;
			lowFlatCorner.forward = -dir.normalized;
			isCorner = true;
			wallState = WallState.ShortWall;
		}
		else if(wallCount == 2)
		{
			// means the wall is straight
			if((zPosConnectedNode != null) == (zNegConnectedNode != null) && (xPosConnectedNode != null) == (xNegConnectedNode != null))
			{
				singleCorner.gameObject.SetActive(false);
				lowSingleCorner.gameObject.SetActive(false);
				flatCorner.gameObject.SetActive(false);
				lowFlatCorner.gameObject.SetActive(false);
				wallState = WallState.LongWall;
			}
			else
			{
				singleCorner.gameObject.SetActive(false);
				lowSingleCorner.gameObject.SetActive(false);
				flatCorner.gameObject.SetActive(false);
				lowFlatCorner.gameObject.SetActive(false);

				Vector3 dir = Vector3.zero;
				dir = (xPosConnectedNode != null && (xPosConnectedNode.xNegWall != null && !xPosConnectedNode.xNegWall.IsLowWall)) ? dir + xPosWall.transform.forward : dir;
				dir = (xNegConnectedNode != null && (xNegConnectedNode.xPosWall != null && !xNegConnectedNode.xPosWall.IsLowWall)) ? dir + xNegWall.transform.forward : dir;
				dir = (zPosConnectedNode != null && (zPosConnectedNode.zNegWall != null && !zPosConnectedNode.zNegWall.IsLowWall)) ? dir + zPosWall.transform.forward : dir;
				dir = (zNegConnectedNode != null && (zNegConnectedNode.zPosWall != null && !zNegConnectedNode.zPosWall.IsLowWall)) ? dir + zNegWall.transform.forward : dir;

				bool hasHighWall = !(zPosWall.IsLowWall && zNegWall.IsLowWall && xPosWall.IsLowWall && xNegWall.IsLowWall);
				bool hasLowWall = zPosWall.IsLowWall || zNegWall.IsLowWall || xPosWall.IsLowWall || xNegWall.IsLowWall;

				singleCorner.gameObject.SetActive(hasHighWall);
				lowSingleCorner.gameObject.SetActive(!hasHighWall && hasLowWall);

				singleCorner.forward = dir.normalized;
				lowSingleCorner.forward = -dir.normalized;
				isCorner = true;
				wallState = WallState.Corner;
			}
		}
	}

	private void OnDrawGizmos()
	{
		Color c = Gizmos.color;
		Color y = Color.yellow;
		y.a = 0.5f;
		Gizmos.color = y;
		Gizmos.DrawCube(connectionObject.transform.position, connectionObject.localScale);
		Gizmos.color = c;
	}

	private void OnDrawGizmosSelected()
	{
		GUIStyle style = new GUIStyle();
		style.normal.textColor = Color.black;

		Handles.Label(connectionObject.transform.position + (Vector3.up) * 0.25f, wallState.ToString(), style);

		Color c1 = Gizmos.color;
		Color b = Color.blue;
		b.a = 0.5f;
		Gizmos.color = b;

		Handles.Label(connectionObject.transform.position + (Vector3.up) * 0.25f + Vector3.forward * 0.5f, $"zPos wall \n WallType: {zPosPreferredWallSegmentType.ToString()}", style);
		Gizmos.DrawCube(connectionObject.transform.position + Vector3.forward * 0.5f, connectionObject.localScale);
		Gizmos.DrawCube(connectionObject.transform.position + Vector3.forward, connectionObject.localScale * 0.5f);

		
		Gizmos.DrawCube(connectionObject.transform.position - Vector3.forward, connectionObject.localScale * 0.5f);
		Gizmos.color = c1;

		Color c2 = Gizmos.color;
		Color r = Color.red;
		r.a = 0.5f;
		Gizmos.color = r;

		Handles.Label(connectionObject.transform.position + (Vector3.up) * 0.25f + Vector3.right * 0.5f, $"xPos wall \n WallType: {xPosPreferredWallSegmentType.ToString()}", style);
		Gizmos.DrawCube(connectionObject.transform.position + Vector3.right * 0.5f, connectionObject.localScale);
		Gizmos.DrawCube(connectionObject.transform.position + Vector3.right, connectionObject.localScale * 0.5f);

		Gizmos.DrawCube(connectionObject.transform.position - Vector3.right, connectionObject.localScale * 0.5f);

		Gizmos.color = c2;

	}
}
