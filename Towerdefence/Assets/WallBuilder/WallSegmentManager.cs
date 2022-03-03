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
		fullWall,
		halfFullWall,
		ventWall,
		doorWall,
		halfLowWall,
		lowWall
	}

	[SerializeField] private Transform FullWall = null;
	[SerializeField] private Transform HalfFullWall = null;
	[SerializeField] private Transform VentWall = null;
	[SerializeField] private Transform DoorWall = null;
	[SerializeField] private Transform HalfShortFullWall = null;
	[SerializeField] private Transform ShortFullWall = null;

	public float WallLength { get; private set; } = 1f;
	public float PreferredWallLength { get; private set; } = 1f;
	public float WallSpace { get => wallSpace; set { wallSpace = value; CheckWallLengths(); } }
	public bool CanFullWall = false;

	public bool IsLowWall => segmentType == WallSegmentType.halfLowWall || segmentType == WallSegmentType.lowWall;

	private void CheckWallLengths()
	{
		int num = Mathf.RoundToInt(wallSpace);
		num = num - (num % 25);
		CanFullWall = num >= 100;

		if(preferredType != WallSegmentType.None)
		{

			if (!CanFullWall && (preferredType != WallSegmentType.halfLowWall || preferredType != WallSegmentType.halfFullWall))
				if (preferredType == WallSegmentType.lowWall)
					segmentType = WallSegmentType.halfLowWall;
				else
					segmentType = WallSegmentType.halfFullWall;
			else
				segmentType = preferredType;
		}
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

	private WallSegmentType preferredType = WallSegmentType.None;

#if UNITY_EDITOR
	private void Update()
	{
		FullWall.gameObject.SetActive(segmentType == WallSegmentType.fullWall);
		HalfFullWall.gameObject.SetActive(segmentType == WallSegmentType.halfFullWall);
		VentWall.gameObject.SetActive(segmentType == WallSegmentType.ventWall);
		DoorWall.gameObject.SetActive(segmentType == WallSegmentType.doorWall);
		HalfShortFullWall.gameObject.SetActive(segmentType == WallSegmentType.halfLowWall);
		ShortFullWall.gameObject.SetActive(segmentType == WallSegmentType.lowWall);

		switch (segmentType)
		{
			case WallSegmentType.fullWall:
				WallLength = 1f;
				break;
			case WallSegmentType.halfFullWall:
				WallLength = 0.5f;
				break;
			case WallSegmentType.ventWall:
				WallLength = 1f;
				break;
			case WallSegmentType.doorWall:
				WallLength = 1f;
				break;
			case WallSegmentType.halfLowWall:
				WallLength = 0.5f;
				break;
			case WallSegmentType.lowWall:
				WallLength = 1f;
				break;
			default:
				WallLength = 0f;
				break;
		}

		switch (preferredType)
		{
			case WallSegmentType.fullWall:
				PreferredWallLength = 1f;
				break;
			case WallSegmentType.halfFullWall:
				PreferredWallLength = 0.5f;
				break;
			case WallSegmentType.ventWall:
				PreferredWallLength = 1f;
				break;
			case WallSegmentType.doorWall:
				PreferredWallLength = 1f;
				break;
			case WallSegmentType.halfLowWall:
				PreferredWallLength = 0.5f;
				break;
			case WallSegmentType.lowWall:
				PreferredWallLength = 1f;
				break;
			default:
				PreferredWallLength = 0f;
				break;
		}
	}
#endif
}
