using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BunnyHopMovement : Movement
{
	private List<int> collidingObjects = new List<int>();
	private bool applyFriction = false;
	private bool collidedLastFrame = false;
	private int frameCounter = 0;

	public override Vector3 calculateAdditionalVelocity(Vector2 input)
	{
		//Get input and make it a vector
		Vector3 camRotation = new Vector3(0f, camObj.transform.rotation.eulerAngles.y, camObj.transform.rotation.eulerAngles.z);
		Vector3 inputVelocity = Quaternion.Euler(camRotation) * new Vector3(input.x * speed, 0f, input.y * speed);
		Vector3 additionalVelocity = new Vector3(inputVelocity.x, 0f, inputVelocity.z);
		
		//Limit new velocity to the speed maximum
		Vector3 currentVelocity = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z);
		float max = 1 - (currentVelocity.magnitude / maxSpeed);
		float velocityDot = Vector3.Dot(currentVelocity, additionalVelocity);
		Vector3 modifiedVelocity = additionalVelocity * max;
		Vector3 correctVelocity = Vector3.Lerp(additionalVelocity, modifiedVelocity, velocityDot);

		//Return
		return new Vector3(correctVelocity.x, getJumpVelocity(rigidbody.velocity.y), correctVelocity.z);
	}

	public override Vector3 calculateFriction(Vector3 input)
	{
		Vector2 temp = new Vector2(input.x, input.z);

		if(applyFriction)
		{
			temp *= frictionMultiplier;
		}

		return new Vector3(temp.x, input.y, temp.y);
	}

	private float getJumpVelocity(float yVelocity)
	{
		bool onGround = checkGround();

		if(Time.time < lastJumpPress + jumpPressDuration && yVelocity < jumpForce && onGround)
		{
			lastJumpPress = -1f;
			frameCounter = 0;
			GameInfo.info.playSound("jump");
			return jumpForce - yVelocity;
		}
		else
		{
			return 0f;
		}
	}

	public override void FixedMoveUpdate()
	{
		if(collidedLastFrame)
		{
			frameCounter++;
		}
		else
		{
			frameCounter = 0;
		}

		if(collidingObjects.Count == 0)
		{
			collidedLastFrame = false;
		}
		else
		{
			collidedLastFrame = true;
		}

		if(frameCounter > 3)
		{
			applyFriction = true;
		}
		else
		{
			applyFriction = false;
		}
	}

	void OnCollisionEnter(Collision col)
	{
		foreach(ContactPoint contact in col)
		{
			if(contact.normal.y > 0.7f && !collidingObjects.Contains(contact.otherCollider.gameObject.GetInstanceID()))
			{
				collidingObjects.Add(contact.otherCollider.gameObject.GetInstanceID());
			}
		}
	}

	void OnCollisionExit(Collision col)
	{
		foreach(ContactPoint contact in col)
		{
			if(collidingObjects.Contains(contact.otherCollider.gameObject.GetInstanceID()))
			{
				collidingObjects.Remove(contact.otherCollider.gameObject.GetInstanceID());
			}
		}
	}
}
