using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WallSegmentManager : MonoBehaviour
{
	public enum WallSegmentType
	{
		None,
		_1x3FullWall,
		_05x3FullWall,
		_1x3VentWall,
		_1x3DoorWall,
		_05x1Wall,
		_1x1Wall
	}

	[SerializeField] private Transform _1x3FullWall = null;
	[SerializeField] private Transform _05x3FullWall = null;
	[SerializeField] private Transform _1x3VentWall = null;
	[SerializeField] private Transform _1x3DoorWall = null;
	[SerializeField] private Transform _05x1Wall = null;
	[SerializeField] private Transform _1x1Wall = null;

	public float WallLength { get; private set; } = 1f;
	public float PreferredWallLength { get; private set; } = 1f;
	public float WallSpace { get => wallSpace; set { wallSpace = value; CheckWallLengths(); } }
	public bool CanFullWall = false;

	private void CheckWallLengths()
	{
		int num = Mathf.RoundToInt(wallSpace);
		num = num - (num % 25);
		CanFullWall = num >= 100;

		if (!CanFullWall && (preferredType != WallSegmentType._05x1Wall || preferredType != WallSegmentType._05x3FullWall))
			if (preferredType == WallSegmentType._1x1Wall)
				segmentType = WallSegmentType._05x1Wall;
			else
				segmentType = WallSegmentType._05x3FullWall;
		else
			segmentType = preferredType;
	}

	private float wallSpace = 0f;

	[SerializeField] private WallSegmentType segmentType = WallSegmentType.None;
	public WallSegmentType SegmentType { set { SetSegmentType(value); } get { return segmentType; } }

	private void SetSegmentType(WallSegmentType value, bool isForced = false)
	{
		segmentType = value;
		if(!isForced)
		{
			preferredType = value;
			CheckWallLengths();
		}
	}

	private WallSegmentType preferredType = WallSegmentType._1x3FullWall;

#if UNITY_EDITOR
	private void Update()
	{
		_1x3FullWall.gameObject.SetActive(segmentType == WallSegmentType._1x3FullWall);
		_05x3FullWall.gameObject.SetActive(segmentType == WallSegmentType._05x3FullWall);
		_1x3VentWall.gameObject.SetActive(segmentType == WallSegmentType._1x3VentWall);
		_1x3DoorWall.gameObject.SetActive(segmentType == WallSegmentType._1x3DoorWall);
		_05x1Wall.gameObject.SetActive(segmentType == WallSegmentType._05x1Wall);
		_1x1Wall.gameObject.SetActive(segmentType == WallSegmentType._1x1Wall);

		switch (segmentType)
		{
			case WallSegmentType._1x3FullWall:
				WallLength = 1f;
				break;
			case WallSegmentType._05x3FullWall:
				WallLength = 0.5f;
				break;
			case WallSegmentType._1x3VentWall:
				WallLength = 1f;
				break;
			case WallSegmentType._1x3DoorWall:
				WallLength = 1f;
				break;
			case WallSegmentType._05x1Wall:
				WallLength = 0.5f;
				break;
			case WallSegmentType._1x1Wall:
				WallLength = 1f;
				break;
			default:
				WallLength = 0f;
				break;
		}

		switch (preferredType)
		{
			case WallSegmentType._1x3FullWall:
				PreferredWallLength = 1f;
				break;
			case WallSegmentType._05x3FullWall:
				PreferredWallLength = 0.5f;
				break;
			case WallSegmentType._1x3VentWall:
				PreferredWallLength = 1f;
				break;
			case WallSegmentType._1x3DoorWall:
				PreferredWallLength = 1f;
				break;
			case WallSegmentType._05x1Wall:
				PreferredWallLength = 0.5f;
				break;
			case WallSegmentType._1x1Wall:
				PreferredWallLength = 1f;
				break;
			default:
				PreferredWallLength = 0f;
				break;
		}
	}
#endif
}
