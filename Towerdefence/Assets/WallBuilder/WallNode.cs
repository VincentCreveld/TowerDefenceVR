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

	[SerializeField] private WallSegmentManager xPosWall;
	[SerializeField] private WallSegmentManager xNegWall;
	[SerializeField] private WallSegmentManager zPosWall;
	[SerializeField] private WallSegmentManager zNegWall;
	[SerializeField] private Transform singleCorner;
	[SerializeField] private Transform flatCorner;
	[SerializeField] private Transform connectionObject;
	[SerializeField] private LayerMask raycastLayer;
	[SerializeField] private LayerMask connectionObjectLayer;

	[SerializeField] private bool zPosConnected = false;
	[SerializeField] private bool zNegConnected = false;
	[SerializeField] private bool xPosConnected = false;
	[SerializeField] private bool xNegConnected = false;

	[SerializeField] private WallSegmentLength zPosLength = WallSegmentLength.None;
	[SerializeField] private WallSegmentLength zNegLength = WallSegmentLength.None;
	[SerializeField] private WallSegmentLength xPosLength = WallSegmentLength.None;
	[SerializeField] private WallSegmentLength xNegLength = WallSegmentLength.None;

	[SerializeField] private WallSegmentManager.WallSegmentType zPosPreferredWallSegmentType = WallSegmentManager.WallSegmentType._1x3FullWall;
	[SerializeField] private WallSegmentManager.WallSegmentType zNegPreferredWallSegmentType = WallSegmentManager.WallSegmentType._1x3FullWall;
	[SerializeField] private WallSegmentManager.WallSegmentType xPosPreferredWallSegmentType = WallSegmentManager.WallSegmentType._1x3FullWall;
	[SerializeField] private WallSegmentManager.WallSegmentType xNegPreferredWallSegmentType = WallSegmentManager.WallSegmentType._1x3FullWall;

	private bool isCorner = false;
	private int wallCount = 0;

	public WallState wallState { get; private set; } = WallState.None;

	public void CheckAllCorners()
	{
		connectionObject.transform.localPosition = Vector3.zero;

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
			zPosConnected = false;
			zPosLength = WallSegmentLength.None;
			
			if ((1 << hit.transform.gameObject.layer) == connectionObjectLayer)
			{
				zPosConnected = true;
				float length = Mathf.RoundToInt(Vector3.Distance(transform.position, hit.transform.position) * 100);
				length = length - (length % 25);

				zPosLength = (length <= zPosWall.WallLength * 100) ? WallSegmentLength.Full : (length == zPosWall.WallLength * 50) ? WallSegmentLength.Half : WallSegmentLength.None;

				zPosWall.WallSpace = length;
			}
		}
		else
		{
			zPosConnected = false;
			zPosLength = WallSegmentLength.None;
		}

		rayLength = zNegWall.PreferredWallLength;
		if (rayLength <= 0)
			rayLength = 1f;
		if (Physics.Raycast(zNeg, out hit, rayLength, raycastLayer))
		{
			zNegConnected = false;
			zNegLength = WallSegmentLength.None;
			
			if ((1 << hit.transform.gameObject.layer) == connectionObjectLayer)
			{
				zNegConnected = true;
				float length = Mathf.RoundToInt(Vector3.Distance(transform.position, hit.transform.position) * 100);
				length = length - (length % 25);

				zNegLength = (length <= zNegWall.WallLength * 100) ? WallSegmentLength.Full : (length == zNegWall.WallLength * 50) ? WallSegmentLength.Half : WallSegmentLength.None;

				zNegWall.WallSpace = length;
			}
		}
		else
		{
			zNegConnected = false;
			zNegLength = WallSegmentLength.None;
		}

		rayLength = xPosWall.PreferredWallLength;
		if (rayLength <= 0)
			rayLength = 1f;
		if (Physics.Raycast(xPos, out hit, rayLength, raycastLayer))
		{
			xPosConnected = false;
			xPosLength = WallSegmentLength.None;

			if ((1 << hit.transform.gameObject.layer) == connectionObjectLayer)
			{
				xPosConnected = true;
				float length = Mathf.RoundToInt(Vector3.Distance(transform.position, hit.transform.position) * 100);
				length = length - (length % 25);

				xPosLength = (length <= xPosWall.WallLength * 100) ? WallSegmentLength.Full : (length == xPosWall.WallLength * 50) ? WallSegmentLength.Half : WallSegmentLength.None;

				xPosWall.WallSpace = length;
			}
		}
		else
		{
			xPosConnected = false;
			xPosLength = WallSegmentLength.None;
		}

		rayLength = xNegWall.PreferredWallLength;
		if (rayLength <= 0)
			rayLength = 1f;
		if (Physics.Raycast(xNeg, out hit, rayLength, raycastLayer))
		{
			xNegConnected = false;
			xNegLength = WallSegmentLength.None;

			if ((1 << hit.transform.gameObject.layer) == connectionObjectLayer)
			{
				xNegConnected = true;
				float length = Mathf.RoundToInt(Vector3.Distance(transform.position, hit.transform.position) * 100);
				length = length - (length % 25);

				xNegLength = (length <= xNegWall.WallLength * 100) ? WallSegmentLength.Full : (length == xNegWall.WallLength * 50) ? WallSegmentLength.Half : WallSegmentLength.None;

				xNegWall.WallSpace = length;
			}
		}
		else
		{
			xNegConnected = false;
			xNegLength = WallSegmentLength.None;
		}
	}

	[ContextMenu("CheckCorners")]
	public void SetupWall()
	{
		zPosWall.SegmentType = zPosPreferredWallSegmentType;
		zNegWall.SegmentType = zNegPreferredWallSegmentType;
		xPosWall.SegmentType = xPosPreferredWallSegmentType;
		xNegWall.SegmentType = xNegPreferredWallSegmentType;
		CheckAllCorners();




		xPosWall.gameObject.SetActive(xPosConnected);
		xNegWall.gameObject.SetActive(xNegConnected && xNegLength == WallSegmentLength.Full);
		zPosWall.gameObject.SetActive(zPosConnected);
		zNegWall.gameObject.SetActive(zNegConnected && zNegLength == WallSegmentLength.Full);

		SetCorners();
	}

	[ContextMenu("OptimiseWalls")]
	private void OptimiseWalls()
	{
		
	}

	private void SetCorners()
	{
		wallCount = 0;
		wallCount = (zPosConnected) ? wallCount + 1 : wallCount;
		wallCount = (zNegConnected) ? wallCount + 1 : wallCount;
		wallCount = (xPosConnected) ? wallCount + 1 : wallCount;
		wallCount = (xNegConnected) ? wallCount + 1 : wallCount;

		isCorner = false;

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
			flatCorner.gameObject.SetActive(true);

			Vector3 dir = Vector3.zero;
			dir = (zPosConnected) ? zPosWall.transform.forward : dir;
			dir = (zNegConnected) ? zNegWall.transform.forward : dir;
			dir = (xPosConnected) ? xPosWall.transform.forward : dir;
			dir = (xNegConnected) ? xNegWall.transform.forward : dir;

			flatCorner.forward = -dir.normalized;
			isCorner = true;
			wallState = WallState.ShortWall;

		}
		else if(wallCount == 2)
		{
			// means the wall is straight
			if(zPosConnected == zNegConnected && xPosConnected == xNegConnected)
			{
				singleCorner.gameObject.SetActive(false);
				flatCorner.gameObject.SetActive(false);
				wallState = WallState.LongWall;
			}
			else
			{
				singleCorner.gameObject.SetActive(true);
				flatCorner.gameObject.SetActive(false);

				Vector3 dir = Vector3.zero;
				dir += (zPosConnected) ? zPosWall.transform.forward : Vector3.zero;
				dir += (zNegConnected) ? zNegWall.transform.forward : Vector3.zero;
				dir += (xPosConnected) ? xPosWall.transform.forward : Vector3.zero;
				dir += (xNegConnected) ? xNegWall.transform.forward : Vector3.zero;

				singleCorner.forward = dir.normalized;
				isCorner = true;
				wallState = WallState.Corner;

			}
		}
	}

	private void OnDrawGizmos()
	{
		Color h = Gizmos.color;
		Handles.color = Color.blue;
		Handles.Label(connectionObject.transform.position + (Vector3.up) * 0.25f, wallState.ToString());
		Handles.color = h;
		Color c = Gizmos.color;
		Gizmos.color = Color.blue;
		Gizmos.DrawCube(connectionObject.transform.position, connectionObject.localScale);
		Gizmos.color = c;
	}
}
