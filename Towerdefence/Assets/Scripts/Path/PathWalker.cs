using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathWalker : MonoBehaviour
{
	[SerializeField] private float rotationSpeed = 100f;
	[SerializeField] private Path path = null;

    private int pathNodes = 0;
    [SerializeField] private float movementSpeed = 2f;

	// some control variable to freeze movement
	private bool isMoving = false;

	private int currentPathIndex = 0;
	private NodeInfo currentTargetNode = null;

	[SerializeField] private Enemy connectedEnemy = null;


	private void Awake()
	{
		isMoving = false;
	}

	Quaternion targetRotation = Quaternion.identity;

	private void Update()
	{
		if (!isMoving)
			return;

		transform.position = Vector3.MoveTowards(transform.position, currentTargetNode.to, movementSpeed * Time.deltaTime);
		transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
		if (transform.position == currentTargetNode.to)
		{
			if (currentTargetNode.nextNode != null)
			{
				currentPathIndex++;
				currentTargetNode = currentTargetNode.nextNode;
				transform.position = currentTargetNode.from;

				targetRotation = Quaternion.LookRotation(currentTargetNode.GetMovementDirection());

				//transform.forward = -currentTargetNode.GetMovementDirection();
			}
			else
				GoalReached();
		}

	}

	[ContextMenu("Start movement")]
	public void StartMovement(float speed, Path path)
	{
		this.path = path;

		movementSpeed = speed;
		currentTargetNode = path.StartNode;
		transform.position = currentTargetNode.from;
		transform.forward = currentTargetNode.GetMovementDirection();
		targetRotation = Quaternion.LookRotation(currentTargetNode.GetMovementDirection());


		isMoving = true;
	}

	public void Cleanup()
	{
		isMoving = false;
	}

	private void GoalReached()
	{
		isMoving = false;
		connectedEnemy.ReachedEndOfPath();
	}
}
