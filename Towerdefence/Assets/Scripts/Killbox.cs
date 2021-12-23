using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
public class Killbox : MonoBehaviour
{
	private void Awake()
	{
		Rigidbody rb = GetComponent<Rigidbody>();
		rb.useGravity = false;
		rb.constraints = RigidbodyConstraints.FreezeAll;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.TryGetComponent<IHittable>(out IHittable hittable))
		{
			hittable.Die();
		}
	}
}
