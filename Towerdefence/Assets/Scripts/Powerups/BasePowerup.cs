using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PowerupType
{
	Default,
	Health,
	Shotgun,
	Burst,
	Automatic,
	Explosive
}

public class BasePowerup : MonoBehaviour
{

	[SerializeField] private ParabolaMovement movement = null;
	[SerializeField] private float averageFlyHeight = 4f;
	[SerializeField] private float travelTime = 3f;

	public bool isOriented = false;
	[ContextMenu("Do movement")]
	public void SpawnPowerup()
	{
		Init(PowerupType.Health, transform.position);
	}

	public void Init(PowerupType type, Vector3 startPos)
	{
		float randomMod = UnityEngine.Random.Range(-0.3f, 0.3f);

		movement.StartMovement(startPos, WaveManager.Instance.GetHotspotPosition(), averageFlyHeight + randomMod, travelTime - randomMod, isOriented);
	}
}
