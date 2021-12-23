using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupManager : MonoBehaviour
{
	// might need to vary per wave, maybe some guarantee
    [SerializeField] private int powerupSpawnOdds = 10;

	[SerializeField] private List<PowerupOdds> oddsRegistry = new List<PowerupOdds>();

	public List<Vector2> ranges = null;

	[SerializeField] private BasePowerup powerupPrefab = null;

	private void Awake()
	{
		SetOdds();
	}

	private void OnValidate()
	{
		SetOdds();
	}
	
	private void SetOdds()
	{
		int totalOdds = 0;

		foreach (PowerupOdds p in oddsRegistry)
		{
			totalOdds += p.spawnWeight;
		}

		if (totalOdds <= 0)
			return;

		ranges = new List<Vector2>();

		float point = 0.0f;
		foreach (PowerupOdds p in oddsRegistry)
		{
			float oddsRange = (float)p.spawnWeight / (float)totalOdds;
			p.Init(point, point + oddsRange);

			ranges.Add(new Vector2(point, point + oddsRange));

			point += oddsRange;
		}
	}

	public void GetPowerup(Vector3 startPos)
	{
		int canSpawn = UnityEngine.Random.Range(0, (int)100);

		if (canSpawn > powerupSpawnOdds)
			return;

		float rdm = Mathf.Clamp01(UnityEngine.Random.Range(0.0f, 1.0f));

		foreach (var item in oddsRegistry)
		{
			if (item.IsInRange(rdm))
			{
				SpawnPowerup(item.GetPowerupType(), startPos);
				return;
			}
		}
	}

	// needs to be pooled
	private void SpawnPowerup(PowerupType t, Vector3 spawnPos)
	{
		BasePowerup powerup = Instantiate(powerupPrefab) as BasePowerup;
		powerup.Init(t, spawnPos);
	}
}

[System.Serializable]
public class PowerupOdds
{
	[SerializeField] private PowerupType type = PowerupType.Default;
	public int spawnWeight = 10;

	private float lowerOdd = 0.0f;
	private float higherOdd = 0.0f;

	public void Init(float low, float high)
	{
		lowerOdd = Mathf.Min(Mathf.Clamp01(low), Mathf.Clamp01(high));
		higherOdd = Mathf.Max(Mathf.Clamp01(low), Mathf.Clamp01(high));
	}

	public bool IsInRange(float f)
	{
		return f >= lowerOdd && f < higherOdd;
	}

	public PowerupType GetPowerupType()
	{
		return type;
	}
}
