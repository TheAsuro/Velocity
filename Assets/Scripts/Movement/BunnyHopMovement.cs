using UnityEngine;
using System.Collections;

public class BunnyHopMovement : Movement
{
	public override Vector3 calculateAdditionalVelocity(float frametime)
	{
		bool onGround = checkGround();

		// Make autojump
		int jump = 0;
		if (Time.time < lastJumpPress + 0.1f && onGround) { jump = 1; lastJumpPress = -1f; onGround = false; }

		//Get input and make it a vector
		Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
		Vector3 camRotation = new Vector3(0f, camObj.transform.rotation.eulerAngles.y, camObj.transform.rotation.eulerAngles.z);
		Vector3 inputVelocity = Quaternion.Euler(camRotation) * new Vector3(input.x * speed, 0f, input.y * speed);

		Vector3 wishvel = new Vector3(inputVelocity.x, 0f, inputVelocity.z);
		float wishspeed = wishvel.magnitude;
		if (wishspeed > speed)
		{
			float fraction = speed / wishspeed;
			wishvel *= fraction;
			wishspeed = speed;
		}

		float wishspd = wishspeed;
		if (!onGround)
		{
			if (wishspd > 30 * UPSToSpeed)
				wishspd = 30 * UPSToSpeed;
		}
		else
		{
			if (wishspd > maxSpeed)
				wishspd = maxSpeed;
		}

		Vector3 originalVelocity = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z);
		Vector3 newVel = originalVelocity;

		// Friction
		if (onGround)
		{
			float origSpeed = newVel.magnitude;
			if (origSpeed >= 0.1 * UPSToSpeed)
			{
				float friction = 4;
				float control = (origSpeed < 100 * UPSToSpeed) ? 100 * UPSToSpeed : origSpeed;
				float drop = control * friction * frametime;
				float newspeed = origSpeed - drop;
				if (newspeed < 0)
					newspeed = 0;

				newspeed /= origSpeed;
				newVel *= newspeed;
			}
		}

		// Acceleration
		float curspeed = Vector3.Dot(newVel, wishvel.normalized);
		float addspeed = wishspd - curspeed;
		//Debug.Log("Wishspd: " + wishspd + " curspeed: " + curspeed + " addspeed: " + addspeed);

		if (addspeed > 0)
		{
			float A = 10f * wishspeed * frametime;
			if (A > addspeed)
				A = addspeed;

			newVel += A * wishvel.normalized;
		}

		//Return
		return new Vector3(newVel.x - originalVelocity.x, jump * jumpForce, newVel.z - originalVelocity.z);
	}
}
