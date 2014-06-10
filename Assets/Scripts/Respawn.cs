using UnityEngine;
using System.Collections;

//Player resets to one of these points if r is pressed, handeled by GameInfo
public class Respawn : MonoBehaviour
{
	public bool isFistSpawn = false;

	void Start()
	{
		if(isFistSpawn)
		{
			GameInfo.info.setSpawn(this);
		}
	}

	public Vector3 getSpawnPos()
	{
		return transform.position;
	}
	
	public Quaternion getSpawnRot()
	{
		return transform.rotation;
	}
}
