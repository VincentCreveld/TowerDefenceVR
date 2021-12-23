using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum EnemyType
{
	Normal,
	Fast,
	Tanky
}

// needs to split wavemanager and scenemanager functionalities
public class WaveManager : MonoBehaviour
{
	public static WaveManager Instance = null;
	
	// needs some display
	[SerializeField] private float downTimeBetweenWaves = 15f;

	private int waveCount = 0;

	private List<Enemy> liveEnemies = new List<Enemy>();

	[SerializeField] private List<WaveData> waveRegistry = null;
	[SerializeField] private Path path = null;
	[SerializeField] private BaseHealth baseHealth = null;
	[SerializeField] private PlatformHotspotManager hotspotManager = null;
	[SerializeField] private PowerupManager powerupManager = null;

	private float downTimerLeft = 0.0f;
	private bool timerRunning = false;

	private bool gameIsRunning = true;


	private void Awake()
	{
		if(Instance != null)
			Instance = this;
		else if(Instance != this && Instance != null)
		{
			Instance.enabled = false;
			Instance = this;
		}
		else
		{
			Instance = this;
		}
	}

	private void Start()
	{
		baseHealth.GameOverEvent += OnGameOverEvent;
	}

	private void Update()
	{
		if(gameIsRunning)
			CheckGameState();
	}
	
	private void CheckGameState()
	{
		if (liveEnemies.Count > 0)
		{
			timerRunning = false;

			return;
		}
		else
		{
			if(!timerRunning)
			{
				if (waveCount >= waveRegistry.Count)
					EndGame();
				else
					downTimerLeft = downTimeBetweenWaves;
				timerRunning = true;
			}
			else
			{
				downTimerLeft -= Time.deltaTime;

				if (downTimerLeft <= 0.0f)
				{
					if (waveCount >= waveRegistry.Count)
						EndGame();
					else
						SpawnWave();

					timerRunning = false;
					downTimerLeft = 0.0f;
				}
			}

		}
	}

	private void EndGame()
	{
		Debug.Log("Game over, no more waves");
		gameIsRunning = false;
	}

	private void OnGameOverEvent()
	{
		List<Enemy> enemiesToDie = new List<Enemy>();
		foreach (Enemy e in liveEnemies)
		{
			enemiesToDie.Add(e);
		}

		foreach (Enemy e in enemiesToDie)
		{
			e.Die();
		}
	}

	[ContextMenu("Spawn wave")]
	public void SpawnWave()
	{
		StartCoroutine(SpawnWaveSegmented(waveRegistry[waveCount]));
		waveCount++;
	}

	private IEnumerator SpawnWaveSegmented(WaveData wave)
	{
		int seg = 0;
		foreach (EnemyCount segment in wave.waveData)
		{
			float timer = 0f;
			int count = 0;
			while(count < wave.waveData[seg].count)
			{
				timer += Time.deltaTime;
				yield return null;
				if(timer > wave.waveData[seg].spawnDelay || count == 0)
				{
					SpawnEnemy(wave.waveData[seg].type);
					timer = 0f;
					count++;
				}
			}
			seg++;
		}
	}

	[SerializeField] private Enemy enemyPrefab = null;

	// needs some form of pooling
	private void SpawnEnemy(EnemyType type)
	{
		Enemy en = Instantiate(enemyPrefab) as Enemy;
		en.Init(type, ReturnEnemy, path);
		liveEnemies.Add(en);
	}

	private void ReturnEnemy(Enemy en, bool isProperDeath)
	{
		if (liveEnemies.Contains(en))
			liveEnemies.Remove(en);

		if (!isProperDeath)
			baseHealth.DealDamage(en.currentType);
	}

	public Vector3 GetHotspotPosition()
	{
		return hotspotManager.GetHotspotPosition();
	}

	public void RequestPowerupSpawn(Vector3 spawnPos)
	{
		powerupManager.GetPowerup(spawnPos);
	}
}


