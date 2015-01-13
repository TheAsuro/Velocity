using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BunnyHopMovement : Movement
{
	private bool onGround = false;

	public override Vector3 overrideVelocity(Vector3 currentVelocity)
	{
		onGround = checkGround();
		float speed = currentVelocity.magnitude; 

		if(onGround && !Input.GetButton("Jump"))
		{
			if(speed > 2f)
			{
				speed *= (1f - friction * Time.deltaTime);
			}
			else
			{
				speed -= 2f * friction * Time.deltaTime;
			}

			speed = Mathf.Max(0f, speed);
		}

		return currentVelocity.normalized * speed;
	}

	public override Vector3 calculateAdditionalVelocity(Vector2 input)
	{
		onGround = checkGround();

		//Different acceleration values for ground and air
		float curAccel = accel;
		if(!onGround)
			curAccel = airAccel;

		//Ground speed
		float curMaxSpeed = maxSpeed;

		//Air speed
		if(!onGround)
			curMaxSpeed = maxAirSpeed;

		//Crouched speed on ground
		else if(crouched)
			curMaxSpeed = maxSpeed / 3f;

		//Get input and make it a vector
		Vector3 camRotation = new Vector3(0f, camObj.transform.rotation.eulerAngles.y, camObj.transform.rotation.eulerAngles.z);
		Vector3 inputVelocity = Quaternion.Euler(camRotation) * new Vector3(input.x * curAccel, 0f, input.y * curAccel);

		//Ignore vertical component of rotated input
		Vector3 alignedInputVelocity = new Vector3(inputVelocity.x, 0f, inputVelocity.z) * Time.deltaTime;
		
		//Get current velocity
		Vector3 currentVelocity = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z);

		//How close the current speed to max velocity is (1 = not moving, 0 = at/over max speed)
		float max = Mathf.Max(0f, 1 - (currentVelocity.magnitude / curMaxSpeed));

		//How perpendicular the input to the current velocity is (0 = 90°)
		float velocityDot = Vector3.Dot(currentVelocity, alignedInputVelocity);

		//Scale the input to the max speed
		Vector3 modifiedVelocity = alignedInputVelocity * max;

		//The more perpendicular the input is, the more the input velocity will be applied
		Vector3 correctVelocity = Vector3.Lerp(alignedInputVelocity, modifiedVelocity, velocityDot);

		//Apply jump
		correctVelocity += getJumpVelocity(rigidbody.velocity.y);

		//Return
		return correctVelocity;
	}

	private Vector3 getJumpVelocity(float yVelocity)
	{
		Vector3 jumpVelocity = Vector3.zero;

		//Calculate jump
		if(Time.time < lastJumpPress + jumpPressDuration && yVelocity < jumpForce && checkGround())
		{
			lastJumpPress = -1f;
			GameInfo.info.playSound("jump");
			jumpVelocity = new Vector3(0f, jumpForce - yVelocity, 0f);
		}

		return jumpVelocity;
	}

	public override void FixedMoveUpdate()
	{
		
	}
}
