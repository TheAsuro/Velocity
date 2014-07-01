using UnityEngine;
using System.Collections;

//Player resets to one of these points if r is pressed, handeled by WorldInfo
public class Respawn : MonoBehaviour
{
	public bool isFistSpawn = false;
	public bool respawnPositionIsRelative = true;
	public bool respawnRotationIsRelative = true;
	public Vector3 respawnPosition;
	public float respawnYRotation = 0f;

	void Start()
	{
		if(isFistSpawn)
		{
			WorldInfo.info.setSpawn(this);
		}
	}

	void OnTriggerEnter(Collider col)
	{
		if(col.gameObject.tag.Equals("Player"))
		{
			WorldInfo.info.setSpawn(this);
		}
	}

	public Vector3 getSpawnPos()
	{
		if(respawnPositionIsRelative)
		{
			return transform.position + respawnPosition;
		}
		else
		{
			return respawnPosition;
		}
	}

	public Quaternion getSpawnRot()
	{
		Vector3 respawnRotation = new Vector3(0f, respawnYRotation, 0f);
		if(respawnRotationIsRelative)
		{
			return transform.rotation * Quaternion.Euler(respawnPosition);
		}
		else
		{
			return Quaternion.Euler(respawnRotation);
		}
	}
}
