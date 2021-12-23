using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseHealth : MonoBehaviour
{
	[SerializeField] private float health = 100f;

	public System.Action GameOverEvent = null;

    public void DealDamage(EnemyType type)
	{
		switch (type)
		{
			case EnemyType.Normal:
				health -= 5.0f;
				break;
			case EnemyType.Fast:
				health -= 3.0f;
				break;
			case EnemyType.Tanky:
				health -= 15.0f;
				break;
			default:
				health -= 5.0f;
				break;
		}

		if (health < 0f)
		{
			health = 0f;
			GameOver();
		}
	}

	public void GameOver()
	{
		Debug.Log("Game over");
		GameOverEvent?.Invoke();
	}
}
