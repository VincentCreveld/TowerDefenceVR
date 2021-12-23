using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParabolaMovement : MonoBehaviour
{
	public bool isMoving = false;

	private Vector3 from;
	private Vector3 to;
	private Vector3 tangent;

	private float progress = 0.0f;
	[SerializeField] private float movementSpeed = 1.0f;
	private bool isOriented = false;
	private float heightModifier = 0.0f;


	public void StartMovement(Vector3 startPos, Vector3 endPos, float height, float movementSpeed, bool isOriented = false)
	{
		progress = 0.0f;

		from = startPos;
		to = endPos;
		tangent = Vector3.Lerp(from, to, 0.3f);
		tangent.y = Mathf.Max(from.y, to.y) + height;
		heightModifier = height;

		startPos.y = 0;
		endPos.y = 0;
		float xzDist = Vector3.Distance(startPos, endPos);

		if(xzDist > 0)
			this.movementSpeed = movementSpeed / xzDist;
		else
			this.movementSpeed = movementSpeed;
		this.isOriented = isOriented;

		isMoving = true;
	}

	private void Update()
	{
		if (!isMoving)
			return;

		progress += Time.deltaTime * movementSpeed;

		if (progress >= 1f)
		{
			progress = 1f;
			AtDestination();
			return;
		}
		
		transform.position = GetQuadraticCurvePos(progress, from, to, tangent);
		if(isOriented)
		{
			Vector3 nextPos = GetQuadraticCurvePos((progress + Time.deltaTime * movementSpeed), from, to, tangent);
			transform.rotation = Quaternion.LookRotation((nextPos - transform.position).normalized, Vector3.up);
		}
	}
	
	public Vector3 GetQuadraticCurvePos(float t, Vector3 from, Vector3 to, Vector3 tangent)
	{
		return Vector3.Lerp(Vector3.Lerp(from, tangent, t), Vector3.Lerp(tangent, to, t), t);
	}

	public void AtDestination()
	{
		transform.position = GetQuadraticCurvePos(1f, from, to, tangent);

		isMoving = false;
	}
}
