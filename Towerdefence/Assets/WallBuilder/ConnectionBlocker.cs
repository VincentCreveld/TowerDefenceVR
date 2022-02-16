using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionBlocker : MonoBehaviour
{
	[SerializeField] private Transform col = null;
	private void OnDrawGizmos()
	{
		Color c = Gizmos.color;
		Gizmos.color = Color.red;
		Gizmos.DrawCube(col.position, col.localScale);
		Gizmos.color = c;
	}
}
