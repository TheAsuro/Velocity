using UnityEngine;
using System.Collections;

public class PlayerEffects : MonoBehaviour
{
	private bool moveToPos = false;
	private Vector3 goalPos = Vector3.zero;
	private float startTime = 0f;
	private float moveDuration = 1f;

	public void movePlayerTowardsPosition(Vector3 position, float duration)
	{
		goalPos = position;
		startTime = Time.time;
		moveDuration = duration;
		moveToPos = true;
	}

	public void stopMoveToPos()
	{
		moveToPos = false;
	}

	void Update()
	{
		if(moveToPos)
		{
			transform.position = Vector3.Slerp(transform.position, goalPos, (Time.time - startTime) / moveDuration);
		}
	}
}
