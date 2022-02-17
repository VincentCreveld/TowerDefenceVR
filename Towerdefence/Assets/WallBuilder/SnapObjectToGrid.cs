using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SnapObjectToGrid : MonoBehaviour
{
	[SerializeField] private Vector3 gridSize = new Vector3(0.5f, 0.5f, 0.5f);
    void Update()
    {
		Vector3 pos = new Vector3(
		Mathf.Round(transform.position.x / gridSize.x) * gridSize.x,
		Mathf.Round(transform.position.y / gridSize.y) * gridSize.y,
		Mathf.Round(transform.position.z / gridSize.z) * gridSize.z
			);
		transform.position = pos;
	}
}
