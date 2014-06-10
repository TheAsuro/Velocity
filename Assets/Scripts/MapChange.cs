using UnityEngine;
using System.Collections;

public class MapChange : MonoBehaviour
{
	public string mapName;

	void OnTriggerEnter(Collider col)
	{
		if(col.tag.Equals("Player"))
		{
			changeMap();
		}
	}

	private void changeMap()
	{
		Application.LoadLevel(mapName);
	}
}
