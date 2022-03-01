using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WallPainter : MonoBehaviour
{
	[SerializeField] private Color gizmoColor = new Color();
	[SerializeField] private GameObject collisionPlane = null;
	// add snapping for every 0.25
	[HideInInspector] public float yGridPos = 0.0f;
	[HideInInspector] public bool drawGrid = false;
	[HideInInspector] public int gridSize = 20;
	public LayerMask rayFloorMask = 0;
	public WallNode wallNodePrefab = null;

	public void UpdatePlane()
	{
		collisionPlane.transform.position = Vector3.up * yGridPos;
	}

	public void SetYOffset(float val)
	{
		yGridPos = Mathf.Round(val / 0.25f) * 0.25f;
	}

	private void OnDrawGizmos()
	{
		if (!drawGrid)
			return;

		Color c = Gizmos.color;
		Gizmos.color = gizmoColor;
		Gizmos.DrawCube(collisionPlane.transform.position, collisionPlane.transform.localScale);
		Gizmos.color = c;

		collisionPlane.transform.localScale = new Vector3(gridSize, 0.0001f, gridSize);

		Color intLineColor = Color.black;
		intLineColor.a = 0.5f;
		Color halfLineColor = Color.black;
		halfLineColor.a = 0.25f;
		Color quarterLineColor = Color.black;
		quarterLineColor.a = 0.1f;
		for (float x = 0; x < collisionPlane.transform.localScale.x; x++)
		{
			for (int l = 0; l < 4; l++)
			{
				Vector3 pos = new Vector3((-collisionPlane.transform.localScale.x * 0.5f) + (x + (0.25f * l)), yGridPos, 0.0f);
				Vector3 scale = new Vector3(0.05f, 0.05f, collisionPlane.transform.localScale.x);
				switch (l)
				{
					case 0:
						Gizmos.color = intLineColor;
						Vector3 s = scale;
						s.x *= 1f;
						Gizmos.DrawCube(pos, s);
						break;
					case 2:
						Gizmos.color = halfLineColor;
						Vector3 s2 = scale;
						s2.x *= 0.75f;
						Gizmos.DrawCube(pos, s2);
						break;
					default:
						// 0.25f lines
						// Gizmos.color = quarterLineColor;
						// Vector3 s3 = scale;
						// s3.x *= 0.4f;
						// Gizmos.DrawCube(pos, s3);
						break;
				}
			}
		}

		Vector3 sc = new Vector3(0.05f, 0.05f, collisionPlane.transform.localScale.x);
		Gizmos.color = intLineColor;
		Vector3 s4 = sc;
		s4.x *= 1f;
		Gizmos.DrawCube(new Vector3((collisionPlane.transform.localScale.x * 0.5f), yGridPos, 0.0f), s4);

		sc = new Vector3(collisionPlane.transform.localScale.z, 0.05f, 0.05f);
		s4 = sc;
		s4.z *= 1f;
		Gizmos.DrawCube(new Vector3(0.0f, yGridPos, (collisionPlane.transform.localScale.z * 0.5f)), s4);

		for (int z = 0; z < collisionPlane.transform.localScale.z; z++)
		{
			for (int l = 0; l < 4; l++)
			{
				Vector3 pos = new Vector3(0.0f, yGridPos, (-collisionPlane.transform.localScale.z * 0.5f) + (z + (0.25f * l)));
				Vector3 scale = new Vector3(collisionPlane.transform.localScale.z, 0.05f, 0.05f);
				switch (l)
				{
					case 0:
						Gizmos.color = intLineColor;
						Vector3 s = scale;
						s.z *= 1f;
						Gizmos.DrawCube(pos, s);
						break;
					case 2:
						Gizmos.color = halfLineColor;
						Vector3 s2 = scale;
						s2.z *= 0.75f;
						Gizmos.DrawCube(pos, s2);
						break;
					default:
						// 0.25f lines
						// Gizmos.color = quarterLineColor;
						// Vector3 s3 = scale;
						// s3.z *= 0.4f;
						// Gizmos.DrawCube(pos, s3);
						break;
				}
			}
		}
	}

	public void SetDrawGrid(bool toggle)
	{
		drawGrid = toggle;
	}
}
