using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IHittable
{
	[SerializeField] private PathWalker movement = null;
	[SerializeField] private EnemyGraphics graphics = null;
	[SerializeField] private float maxHealth = 100f;
	private float currentHealth = 100f;

	public EnemyType currentType { get; private set; } = EnemyType.Normal;

	private Action<Enemy, bool> callback = null;

	public void Init(EnemyType type, Action<Enemy, bool> callback, Path path)
	{
		EnemyTypeRegistry.EnemyStats stats = EnemyTypeRegistry.GetEnemyStats(type);

		transform.name = type.ToString();

		graphics.SetGraphics(type);

		movement.StartMovement(stats.movementSpeed, path);

		maxHealth = stats.health;
		currentHealth = maxHealth;

		currentType = type;

		this.callback = callback;
	}

	public void Hit(float damage)
	{
		currentHealth -= damage;

		if(currentHealth <= 0f)
		{
			currentHealth = 0f;
			Die();
		}
	}

	public void Die()
	{
		movement.Cleanup();
		graphics.DisableGraphics();
		gameObject.SetActive(false);

		WaveManager.Instance.RequestPowerupSpawn(transform.position);

		callback?.Invoke(this, true);
	}

	// not the same as die
	public void ReachedEndOfPath()
	{
		movement.Cleanup();
		graphics.DisableGraphics();
		gameObject.SetActive(false);

		callback?.Invoke(this, false);
	}
}

public static class EnemyTypeRegistry
{
	public static EnemyStats GetEnemyStats(EnemyType type)
	{
		switch (type)
		{
			case EnemyType.Normal:
				return new EnemyStats(100, 2);
			case EnemyType.Fast:
				return new EnemyStats(50, 4);
			case EnemyType.Tanky:
				return new EnemyStats(300, 1.5f);
			default:
				return new EnemyStats(100, 2);
		}
	}

	public struct EnemyStats
	{
		public float health;
		public float movementSpeed;

		public EnemyStats(float health, float movementSpeed)
		{
			this.health = health;
			this.movementSpeed = movementSpeed;
		}
	}
}
