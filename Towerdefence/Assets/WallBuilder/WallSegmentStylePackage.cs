using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallSegmentStylePackage : MonoBehaviour
{
	public enum WallStyle
	{
		_1x3_Office,
		_1x4_Marble
	}

	public Transform FullWall = null;
	public Transform HalfFullWall = null;
	public Transform VentWall = null;
	public Transform DoorWall = null;
	public Transform HalfShortFullWall = null;
	public Transform ShortFullWall = null;
}
