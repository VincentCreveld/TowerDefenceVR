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

	[SerializeField] private Transform xPosWall;
	[SerializeField] private Transform xNegWall;
	[SerializeField] private Transform zPosWall;
	[SerializeField] private Transform zNegWall;
	[SerializeField] private Transform singleCorner;
	[SerializeField] private Transform flatCorner;
	[SerializeField] private Transform connectionObject;
	[SerializeField] private LayerMask raycastLayer;
	[SerializeField] private LayerMask connectionObjectLayer;

	[SerializeField] private bool zPosConnected = false;
	[SerializeField] private bool zNegConnected = false;
	[SerializeField] private bool xPosConnected = false;
	[SerializeField] private bool xNegConnected = false;

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
		if (Physics.Raycast(zPos, out hit, 1f, raycastLayer))
			zPosConnected = ((1<<hit.transform.gameObject.layer) == connectionObjectLayer);
		else
			zPosConnected = false;

		if (Physics.Raycast(zNeg, out hit, 1f, raycastLayer))
			zNegConnected =  ((1 << hit.transform.gameObject.layer) == connectionObjectLayer);
		else
			zNegConnected = false;

		if (Physics.Raycast(xPos, out hit, 1f, raycastLayer))
			xPosConnected = ((1 << hit.transform.gameObject.layer) == connectionObjectLayer);
		else
			xPosConnected = false;

		if (Physics.Raycast(xNeg, out hit, 1f, raycastLayer))
			xNegConnected = ((1 << hit.transform.gameObject.layer) == connectionObjectLayer);
		else
			xNegConnected = false;
	}

	[ContextMenu("CheckCorners")]
	public void SetupWall()
	{
		CheckAllCorners();

		xPosWall.gameObject.SetActive(xPosConnected);
		xNegWall.gameObject.SetActive(xNegConnected);
		zPosWall.gameObject.SetActive(zPosConnected);
		zNegWall.gameObject.SetActive(zNegConnected);

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
			dir = (zPosConnected) ? zPosWall.forward : dir;
			dir = (zNegConnected) ? zNegWall.forward : dir;
			dir = (xPosConnected) ? xPosWall.forward : dir;
			dir = (xNegConnected) ? xNegWall.forward : dir;

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
				dir += (zPosConnected) ? zPosWall.forward : Vector3.zero;
				dir += (zNegConnected) ? zNegWall.forward : Vector3.zero;
				dir += (xPosConnected) ? xPosWall.forward : Vector3.zero;
				dir += (xNegConnected) ? xNegWall.forward : Vector3.zero;

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
