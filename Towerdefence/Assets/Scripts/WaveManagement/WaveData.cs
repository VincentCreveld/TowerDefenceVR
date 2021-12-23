using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu()]
public class WaveData : ScriptableObject
{
	int waveNo = 0;
	public List<EnemyCount> waveData = new List<EnemyCount>();
}

[System.Serializable]
public class EnemyCount
{
	public EnemyType type = EnemyType.Normal;
	public int count = 1;
	public float spawnDelay = 0.1f;
}