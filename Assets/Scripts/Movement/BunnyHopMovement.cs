using UnityEngine;
using System.Collections;

public class BunnyHopMovement : Movement
{
	public override Vector3 calculateAdditionalVelocity()
	{
		bool onGround = checkGround();

		//Get input and make it a vector
		Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
		Vector3 camRotation = new Vector3(0f, camObj.transform.rotation.eulerAngles.y, camObj.transform.rotation.eulerAngles.z);
		Vector3 inputVelocity = Quaternion.Euler(camRotation) * new Vector3(input.x * speed, 0f, input.y * speed);
		Vector3 additionalVelocity = new Vector3(inputVelocity.x, 0f, inputVelocity.z);
		
		//Limit new velocity to the speed maximum
		Vector3 currentVelocity = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z);
		float max = 1 - (currentVelocity.magnitude / maxSpeed);
		float velocityDot = Vector3.Dot(currentVelocity, additionalVelocity);
		Vector3 modifiedVelocity = additionalVelocity * max;
		Vector3 correctVelocity = Vector3.Lerp(additionalVelocity, modifiedVelocity, velocityDot);
		
		//Get jump velocity
		int jump = 0;
		if(Time.time < lastJumpPress + 0.2f && onGround) { jump = 1; lastJumpPress = -1f; }

		//Return
		return new Vector3(correctVelocity.x, jump * jumpForce, correctVelocity.z);
	}
}
