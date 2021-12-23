using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGraphics : MonoBehaviour
{
	[SerializeField] private Transform normalGraphics = null;
	[SerializeField] private Transform fastGraphics = null;
	[SerializeField] private Transform tankGraphics = null;
	[SerializeField] private SphereCollider collider = null;

	public void SetGraphics(EnemyType type)
	{
		normalGraphics.gameObject.SetActive(type == EnemyType.Normal);
		fastGraphics.gameObject.SetActive(type == EnemyType.Fast);
		tankGraphics.gameObject.SetActive(type == EnemyType.Tanky);

		switch (type)
		{
			case EnemyType.Normal:
				collider.radius = normalGraphics.localScale.x * 0.5f;
				break;
			case EnemyType.Fast:
				collider.radius = fastGraphics.localScale.x;
				break;
			case EnemyType.Tanky:
				collider.radius = tankGraphics.localScale.x * 0.5f;
				break;
			default:
				collider.radius = normalGraphics.localScale.x * 0.5f;
				break;
		}
	}

	public void DisableGraphics()
	{
		normalGraphics.gameObject.SetActive(false);
		fastGraphics.gameObject.SetActive(false);
		tankGraphics.gameObject.SetActive(false);
	}
}
