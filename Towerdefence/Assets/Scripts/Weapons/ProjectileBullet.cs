using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AmmoType
{
	Regular,
	Automatic,
	Burst,
	Shotgun,
	Explosive
}

public class ProjectileBullet : MonoBehaviour
{
	[SerializeField] private float speed = 1;
	[Tooltip("Life time duration in seconds until object is disabled")]
	[SerializeField] private float lifeTimeDuration = 10f;
	[SerializeField] private LayerMask layerMask = new LayerMask();
	[SerializeField] private TrailRenderer trail = null;

	private Coroutine lifeTimeCoroutine = null;
	private Ray ray;
	private Vector3 prevPosition;
	private Vector3 stepDirection;
	private float stepSize;

	private AmmoType ammoType = AmmoType.Regular;

	[SerializeField] private Transform regularGraphics = null;
	[SerializeField] private Transform automaticGraphics = null;
	[SerializeField] private Transform burstGraphics = null;
	[SerializeField] private Transform shotgunGraphics = null;
	[SerializeField] private Transform explosiveGraphics = null;

	[SerializeField] private Gradient regularTrail = null;
	[SerializeField] private Gradient automaticTrail = null;
	[SerializeField] private Gradient burstTrail = null;
	[SerializeField] private Gradient shotgunTrail = null;
	[SerializeField] private Gradient explosiveTrail = null;

	[SerializeField] private float explosionRange = 1.0f;
	[SerializeField] private GameObject explosionPrefab = null;

	public void Init(Vector3 position, Vector3 direction, AmmoType type)
	{
		transform.position = position;
		transform.forward = direction;
		ammoType = type;
		prevPosition = transform.position;

		regularGraphics.gameObject.SetActive(type == AmmoType.Regular);
		automaticGraphics.gameObject.SetActive(type == AmmoType.Automatic);
		burstGraphics.gameObject.SetActive(type == AmmoType.Burst);
		shotgunGraphics.gameObject.SetActive(type == AmmoType.Shotgun);
		explosiveGraphics.gameObject.SetActive(type == AmmoType.Explosive);

		switch (type)
		{
			case AmmoType.Regular:
				trail.colorGradient = regularTrail;
				trail.startWidth = 0.02f;
				trail.time = 0.2f;
				speed = 15f;
				break;
			case AmmoType.Burst:
				trail.colorGradient = burstTrail;
				trail.startWidth = 0.02f;
				trail.time = 0.2f;
				speed = 18f;
				break;
			case AmmoType.Automatic:
				trail.colorGradient = automaticTrail;
				trail.startWidth = 0.015f;
				trail.time = 0.1f;
				speed = 18f;

				break; 
			case AmmoType.Shotgun:
				trail.colorGradient = shotgunTrail;
				trail.startWidth = 0.015f;
				trail.time = 0.1f;

				speed = UnityEngine.Random.Range(15f, 18f);

				break;
			case AmmoType.Explosive:
				trail.colorGradient = explosiveTrail;
				trail.startWidth = 0.08f;
				trail.time = 0.5f;
				speed = 12f;

				break;
			default:
				trail.colorGradient = regularTrail;
				trail.startWidth = 0.02f;
				trail.time = 0.1f;
				speed = 1f;
				break;
		}

		lifeTimeCoroutine = StartCoroutine(LifeTimeCoroutine(lifeTimeDuration));
	}

	public void LateUpdate()
	{
		Move();
		CheckForCollision();
	}

	protected void Move()
	{
		transform.Translate(transform.forward * speed * Time.deltaTime, Space.World);
	}

	private void CheckForCollision()
	{
		stepDirection = transform.forward.normalized;
		stepSize = (transform.position - prevPosition).magnitude;

		Debug.DrawRay(prevPosition, stepDirection * stepSize, Color.red);

		if (Physics.Raycast(prevPosition, stepDirection, out RaycastHit hitInfo, stepSize, layerMask))
		{
			OnHit(hitInfo);
		}
		else
		{
			prevPosition = transform.position;
		}
	}

	private void OnHit(RaycastHit hitInfo)
	{
		// some edge case for being explosive, needs to spherecast and deal damage that way, no OnHit stuff directly
		if (ammoType == AmmoType.Explosive)
		{
			RemoveBullet();
			return;
		}

		if(hitInfo.collider.gameObject.TryGetComponent<IHittable>(out IHittable hittable))
		{
			hittable.Hit(AmmoTypeDamages.GetAmmoTypeDamage(ammoType));
		}

		// some onhit effect varying per bullet type

		RemoveBullet();
	}

	private void DoExplosiveAmmo(Vector3 pos)
	{
		GameObject explosion = Instantiate(explosionPrefab);
		explosion.transform.localScale = Vector3.one * explosionRange;
		explosion.transform.position = pos;
		explosion.SetActive(true);
		Destroy(explosion, 0.5f);

		RaycastHit[] hits = Physics.SphereCastAll(pos, explosionRange * 0.5f, Vector3.up, explosionRange, layerMask);

		foreach (RaycastHit hit in hits)
		{
			if(hit.collider.gameObject.TryGetComponent<IHittable>(out IHittable hittable))
			{
				float distance = Mathf.InverseLerp(0, explosionRange, Vector3.Distance(pos, hit.point));
				hittable.Hit(GetExplosiveDamage(distance));
			}
		}
	}

	private float GetExplosiveDamage(float distanceFromCenter)
	{
		if (distanceFromCenter < 0.33f)
			return AmmoTypeDamages.EXPLOSIVE_AMMO_DAMAGE;
		else if (distanceFromCenter > 0.67f)
			return AmmoTypeDamages.EXPLOSIVE_AMMO_DAMAGE * 0.75f;
		else
			return AmmoTypeDamages.EXPLOSIVE_AMMO_DAMAGE * 0.5f;
	}

	private IEnumerator LifeTimeCoroutine(float durationInSeconds)
	{
		WaitForSeconds wait = new WaitForSeconds(durationInSeconds);
		yield return wait;

		OnLifeTimeEndEvent();
		lifeTimeCoroutine = null;
	}

	private void OnLifeTimeEndEvent()
	{
		if (lifeTimeCoroutine != null)
			lifeTimeCoroutine = null;
		RemoveBullet();
	}

	private void RemoveBullet()
	{
		if (lifeTimeCoroutine != null)
			StopCoroutine(lifeTimeCoroutine);

		if (ammoType == AmmoType.Explosive)
			DoExplosiveAmmo(transform.position);

		Destroy(gameObject);
	}
}

public static class AmmoTypeDamages
{
	public const float REGULAR_AMMO_DAMAGE = 50f;
	public const float AUTOMATIC_AMMO_DAMAGE = 25f;
	public const float BURST_AMMO_DAMAGE = 60f;
	public const float SHOTGUN_AMMO_DAMAGE = 15f;
	public const float EXPLOSIVE_AMMO_DAMAGE = 200f;

	public static float GetAmmoTypeDamage(AmmoType type)
	{
		switch (type)
		{
			case AmmoType.Regular:
				return REGULAR_AMMO_DAMAGE;
			case AmmoType.Automatic:
				return AUTOMATIC_AMMO_DAMAGE;
			case AmmoType.Burst:
				return BURST_AMMO_DAMAGE;
			case AmmoType.Shotgun:
				return SHOTGUN_AMMO_DAMAGE;
			case AmmoType.Explosive:
				return EXPLOSIVE_AMMO_DAMAGE;
			default:
				return REGULAR_AMMO_DAMAGE;
		}
	}
}
