using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugWeapon : MonoBehaviour
{
	public AmmoType type = AmmoType.Regular;

	public ProjectileBullet prefab = null;

	public Transform fwd = null;

	[SerializeField] private float cooldown = 0.1f;
	private float cdTimeLeft = 0.0f;
	private bool isOnCooldown = false;

	[SerializeField] private int burstAmount = 3;
	[SerializeField] private float roundsPerMinute = 10;
	private bool isBursting = false;

	[ContextMenu("Shoot")]
	public void Shoot()
	{
		switch (type)
		{
			case AmmoType.Burst:
				StartCoroutine(DoBurst());
				break;
			case AmmoType.Shotgun:
				for (int i = 0; i < 15; i++)
				{
					ProjectileBullet b = Instantiate(prefab) as ProjectileBullet;
					Vector3 randomVector = new Vector3(UnityEngine.Random.Range(-0.15f, 0.15f), UnityEngine.Random.Range(-0.15f, 0.15f), UnityEngine.Random.Range(-0.15f, 0.15f));
					b.Init(fwd.position, (fwd.forward + randomVector).normalized, type);
				}
				break;
			default:
				ProjectileBullet bullet = Instantiate(prefab) as ProjectileBullet;
				bullet.Init(fwd.position, fwd.forward, type);
				break;
		}
		isOnCooldown = true;
	}

	private IEnumerator DoBurst()
	{
		isBursting = true;
		for (int i = 0; i < burstAmount; i++)
		{
			yield return new WaitForSeconds(60f / roundsPerMinute);
			ProjectileBullet bullet = Instantiate(prefab) as ProjectileBullet;
			bullet.Init(fwd.position, fwd.forward, type);
		}
		isBursting = false;
	}

	private void Update()
	{
		if(type == AmmoType.Automatic)
		{
			if (Input.GetKey(KeyCode.Space) && !isOnCooldown)
			{
				Shoot();
			}

			if (isOnCooldown)
			{
				cdTimeLeft -= Time.deltaTime;
				if (cdTimeLeft <= 0.0f)
				{
					cdTimeLeft += 60f / roundsPerMinute;
					isOnCooldown = false;
				}
			}
		}
		else
		{
			if (Input.GetKeyDown(KeyCode.Space) && !isOnCooldown)
			{
				Shoot();
			}

			if(isOnCooldown && !isBursting)
			{
				cdTimeLeft -= Time.deltaTime;
				if (cdTimeLeft <= 0.0f)
				{
					cdTimeLeft += cooldown;
					isOnCooldown = false;
				}
			}

		}
	}
}
