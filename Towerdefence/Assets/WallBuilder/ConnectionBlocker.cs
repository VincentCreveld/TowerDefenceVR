using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionBlocker : MonoBehaviour
{
	[SerializeField] private Transform col = null;

	public WallSegmentManager.WallSegmentType WallSegmentType = WallSegmentManager.WallSegmentType.None;

	private void OnDrawGizmos()
	{
		Color c = Gizmos.color;
		Gizmos.color = Color.red;
		Gizmos.DrawCube(col.position, col.localScale);
		Gizmos.color = c;
	}
}
