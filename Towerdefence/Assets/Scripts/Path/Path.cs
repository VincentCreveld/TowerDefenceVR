using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    [SerializeField] private Transform[] path = null;

    private float pathLength = 0.0f;
    private List<float> segmentLengths = null;
	private List<NodeInfo> pathInfo = null;

	public int NodeCount => path.Length;
	public float PathLength => pathLength;

	public NodeInfo StartNode => pathInfo[0];
	public NodeInfo EndNode => pathInfo[pathInfo.Count - 1];

	private void Awake()
	{
		segmentLengths = new List<float>();
		pathInfo = new List<NodeInfo>();
		pathLength = 0.0f;
		for (int i = 1; i < path.Length; i++)
		{
			float len = Vector3.Distance(path[i - 1].position, path[i].position);
			pathLength += len;
			segmentLengths.Add(len);
			pathInfo.Add(new NodeInfo(i - 1, path[i - 1].position, path[i].position, len));
		}

		for (int i = 1; i < pathInfo.Count; i++)
		{
			pathInfo[i - 1].nextNode = pathInfo[i];
		}
	}

	public NodeInfo GetPathSegmentInfo(int i)
	{
		if (i < 0 || i > pathInfo.Count - 1)
			return null;

		return pathInfo[i];
	}

	public NodeInfo GetNodeOnDistance(out float newProgress, float distance, NodeInfo currentNode, float currentProgress)
	{
		int segment = currentNode.segment;
		int returnSegment = segment;
		newProgress = 0.0f;

		float distanceTravelledInCurrentNode = Vector3.Distance(currentNode.from, Vector3.Lerp(currentNode.from, currentNode.to, currentProgress));
		
		float distanceLeft = distance - distanceTravelledInCurrentNode;
		
		while(returnSegment > 0 && distanceLeft > 0f)
		{
			float dis = pathInfo[returnSegment - 1].length;
			if(distanceLeft - dis > 0)
			{
				distanceLeft -= dis;
				returnSegment--;
			}
			else
			{
				float leftOver = Mathf.Abs(distanceLeft - dis);
				newProgress = Mathf.InverseLerp(0, pathInfo[returnSegment - 1].length, leftOver);
				return pathInfo[returnSegment - 1];
			}
		}
				
		return pathInfo[returnSegment];
	}
}

public class NodeInfo
{
	public int segment;
	public Vector3 from, to;
	public float length;

	public NodeInfo nextNode = null;
	public NodeInfo(int segment, Vector3 from, Vector3 to, float length)
	{
		this.from = from;
		this.to = to;
		this.length = length;
		this.segment = segment;
	}

	public Vector3 GetProgressInNode(float progress)
	{
		return Vector3.LerpUnclamped(from, to, progress);
	}

	public Vector3 GetMovementDirection()
	{
		return (to - from).normalized;
	}
}
