using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class WallConnectionManager : MonoBehaviour
{
	[SerializeField] private List<WallNode> walls = new List<WallNode>();
	[SerializeField] private List<ConnectionBlocker> connectionBlockers = new List<ConnectionBlocker>();

#if UNITY_EDITOR
	private void Update()
	{
		// TODO: should only be done when selected. Needs to invoke check on walls it connects to

		UpdateWalls();
		
	}
#endif

	private void UpdateWalls()
	{
		transform.SetAsFirstSibling();

		walls = FindObjectsOfType<WallNode>().ToList();

		foreach (WallNode wall in walls)
		{
			wall.SetupWall();
			wall.transform.SetParent(transform, true);
		}

		connectionBlockers = FindObjectsOfType<ConnectionBlocker>().ToList();
		foreach (var item in connectionBlockers)
		{
			item.transform.SetParent(transform, true);
		}
	}
}
