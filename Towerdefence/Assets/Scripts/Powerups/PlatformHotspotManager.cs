using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlatformHotspotManager : MonoBehaviour
{
	[SerializeField] private List<Transform> hotspots = null;

	private void Awake()
	{
		hotspots = Shuffle(hotspots);
	}

	public Vector3 GetHotspotPosition()
	{
		int rdm = UnityEngine.Random.Range(0, hotspots.Count);
		return hotspots[rdm].position;
	}

	public List<T> Shuffle<T>(List<T> _list)
	{
		for (int i = 0; i < _list.Count; i++)
		{
			T temp = _list[i];
			int randomIndex = Random.Range(i, _list.Count);
			_list[i] = _list[randomIndex];
			_list[randomIndex] = temp;
		}

		return _list;
	}
}
