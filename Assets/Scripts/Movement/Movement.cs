using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour
{
	public float accel = 200f;
	public float airAccel = 200f;
	public float maxSpeed = 6.4f;
	public float maxAirSpeed = 0.6f;
	public float friction = 8f;
	public float jumpForce = 5f;
	public LayerMask groundLayers;

	private GameObject camObj;
	public bool crouched = false;
	public float lastJumpPress = -1f;
	public float jumpPressDuration = 0.1f;
	private bool onGround = false;

	private bool allowJump = true;
	private bool allowRespawn = true;
	private bool allowReset = true;
	private bool allowCrouch = true;
	private bool allowMoveHorizontal = true;
	private bool allowMoveVertical = true;
	private bool frozen = false;

	private bool jumpKeyPressed = false;
	private bool respawnKeyPressed = false;
	private bool resetKeyPressed = false;
	private bool crouchKeyPressed = false;

	void Awake()
	{
		camObj = transform.FindChild("Camera").gameObject;
	}
	
	void Start()
	{
		GameInfo.info.addWindowLine("XZ-Speed: ", getXzVelocityString);
		GameInfo.info.addWindowLine("Y-Speed: ", getYVelocityString);
		GameInfo.info.addWindowLine("Speed 'limit': ", getMaxSpeedString);
		GameInfo.info.addWindowLine("Crouched: ", getCrouchedString);
		GameInfo.info.addWindowLine("On Ground: ", getGroundString);
	}
	
	void Update()
	{
		if(!frozen && !GameInfo.info.isConsoleOpen())
		{
			//Set key states
			if(Input.GetButton("Jump") && allowJump)
				{ jumpKeyPressed = true; } else { jumpKeyPressed = false; }
			if(Input.GetButtonDown("Respawn") && allowRespawn)
				{ respawnKeyPressed = true; } else { respawnKeyPressed = false; }
			if(Input.GetButtonDown("Reset") && allowReset)
				{ resetKeyPressed = true; } else { resetKeyPressed = false; }
			if(Input.GetButton("Crouch") && allowCrouch)
				{ crouchKeyPressed = true; } else { crouchKeyPressed = false; }
			
			if(jumpKeyPressed)
			{
				lastJumpPress = Time.time;
			}
			
			if(respawnKeyPressed)
			{
				respawnPlayer(false);
			}
			if(resetKeyPressed)
			{
				GameInfo.info.reset();
			}
			if(crouchKeyPressed)
			{
				setCrouched(true);
			}
			else
			{
				setCrouched(false);
			}
		}
	}

	public virtual void FixedMoveUpdate()
	{
		if(transform.position.y <= WorldInfo.info.deathHeight)
			respawnPlayer(true);
	}

	public void freeze()
	{
		frozen = true;
		rigidbody.isKinematic = true;
	}

	public void unfreeze()
	{
		rigidbody.isKinematic = false;
		//TODO find less hacky way of updating the rigidbody
		rigidbody.useGravity = false;
		rigidbody.useGravity = true;
		frozen = false;
	}

	void FixedUpdate()
	{
		Vector2 input = new Vector2();

		if(allowMoveHorizontal) { input.x = Input.GetAxis("Horizontal"); }
		if(allowMoveVertical) { input.y = Input.GetAxis("Vertical"); }

		//Friction
		Vector3 tempVelocity = calculateFriction(rigidbody.velocity);

		//Add movement
		tempVelocity += calculateMovement(input, tempVelocity);		
			
		//Apply
		if(!rigidbody.isKinematic)
		{
			rigidbody.velocity = tempVelocity;
		}

		//Kill player if below map
		if(transform.position.y <= WorldInfo.info.deathHeight)
			respawnPlayer(true);
	}

	public virtual Vector3 calculateFriction(Vector3 currentVelocity)
	{
		onGround = checkGround();
		float speed = currentVelocity.magnitude; 

		//Code from https://flafla2.github.io/2015/02/14/bunnyhop.html
		if(onGround && !Input.GetButton("Jump") && speed != 0f)
		{
			float drop = speed * friction * Time.deltaTime;
			return currentVelocity * (Mathf.Max(speed - drop, 0f) / speed);
		}

		return currentVelocity;
	}

	//Do movement input here
	public virtual Vector3 calculateMovement(Vector2 input, Vector3 velocity)
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
		Vector3 currentVelocity = new Vector3(velocity.x, 0f, velocity.z);

		//How close the current speed to max velocity is (1 = not moving, 0 = at/over max speed)
		float max = Mathf.Max(0f, 1 - (currentVelocity.magnitude / curMaxSpeed));

		//How perpendicular the input to the current velocity is (0 = 90°)
		float velocityDot = Vector3.Dot(currentVelocity, alignedInputVelocity);

		//Scale the input to the max speed
		Vector3 modifiedVelocity = alignedInputVelocity * max;

		//The more perpendicular the input is, the more the input velocity will be applied
		Vector3 correctVelocity = Vector3.Lerp(alignedInputVelocity, modifiedVelocity, velocityDot);

		//Apply jump
		correctVelocity += getJumpVelocity(velocity.y);

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

	void OnTriggerEnter(Collider other)
	{
		if(other.tag.Equals("Teleporter"))
		{
			Teleporter tp = other.GetComponent<Teleporter>();
			transform.position = tp.target;
			if(tp.applyRotation)
			{
				transform.rotation = tp.targetRotation;
			}
			if(tp.cancelVelocity)
			{
				rigidbody.velocity = Vector3.zero;
			}
		}
		else if(other.tag.Equals("Kill"))
		{
			respawnPlayer(true);
		}
	}

	//Spawn at a specific checkpoint
	public void spawnPlayer(Respawn spawn)
	{
		if(spawn != null)
		{
			transform.position = spawn.getSpawnPos();
			camObj.transform.rotation = spawn.getSpawnRot();
			rigidbody.velocity = Vector3.zero;
			lastJumpPress = -1f;
		}
		else
		{
			print("Tried to spawn, but no spawnpoint selected. RIP :(");
		}
	}
	
	//Reset and spawn at the last checkpoint
	public void respawnPlayer(bool resetAtStart)
	{
		//Restart race if it is wanted and we would go to the first checkpoint
		if(resetAtStart && WorldInfo.info.getCurrentSpawn() == WorldInfo.info.getFirstSpawn())
		{
			GameInfo.info.reset();
			return;
		}

		//Don't reset the game, just spawn the player at the last (possibly first checkpoint)
		spawnPlayer(WorldInfo.info.getCurrentSpawn());
	}

	public bool getJumpKeyPressed()
	{
		return jumpKeyPressed;
	}
	
	public bool checkGround()
	{
		Vector3 pos = new Vector3(transform.position.x, transform.position.y - collider.bounds.extents.y + 0.05f, transform.position.z);
		Vector3 radiusVector = new Vector3(collider.bounds.extents.x, 0f, 0f);
		return checkCylinder(pos, radiusVector, -0.1f, 8);
	}

	//Doesn't actually check the volume of a cylinder, instead executes a given number of raycasts in a circle
	//origin: center of the circle from which will be cast
	//radiusVector: radius of the circle
	//rayCount: number of vertices the "circle" will have
	private bool checkCylinder(Vector3 origin, Vector3 radiusVector, float verticalLength, int rayCount, out float dist, bool slopeCheck = true)
	{
		bool tempHit = false;
		float tempDist = -1f;

		for(int i = -1; i < rayCount; i++)
		{
			RaycastHit hit;
			bool hasHit = false;
			float verticalDirection = Mathf.Sign(verticalLength);

			if(i == -1) //Check directly from origin
			{
				hasHit = Physics.Raycast(origin, Vector3.up * verticalDirection, out hit, Mathf.Abs(verticalLength), groundLayers);
			}
			else //Check in a circle around the origin
			{
				Vector3 radius = Quaternion.Euler(new Vector3(0f, i * (360f / rayCount), 0f)) * radiusVector;
				Vector3 circlePoint = origin + radius;

				hasHit = Physics.Raycast(circlePoint, Vector3.up * verticalDirection, out hit, Mathf.Abs(verticalLength), groundLayers);
			}
			
			//Collided with something
			if(hasHit)
			{
				//Assign tempDist to the shortest distance
				if(tempDist == -1f)
					tempDist = hit.distance;
				else if(tempDist > hit.distance)
					tempDist = hit.distance;

				//Only return true if the angle is 40° or lower (if slopeCheck is active)
				if(!slopeCheck || hit.normal.y > 0.75f)
				{
					tempHit = true;
				}
			}
		}

		dist = tempDist;

		if(tempHit) { return true; }
		return false;
	}

	private bool checkCylinder(Vector3 origin, Vector3 radiusVector, float verticalLength, int rayCount, bool slopeCheck = true)
	{
		float dist;
		return checkCylinder(origin, radiusVector, verticalLength, rayCount, out dist, slopeCheck);
	}
	
	private void setCrouched(bool state)
	{
		MeshCollider col = (MeshCollider)collider;

		if(!crouched && state)
		{
			//crouch
			col.transform.localScale = new Vector3(col.transform.localScale.x, 0.5f, col.transform.localScale.z);
			transform.position += new Vector3(0f,0.5f,0f);
			camObj.transform.localPosition += new Vector3(0f,-0.25f,0f);
			crouched = true;
		}
		else if(crouched && !state)
		{
			//extend down if not on ground
			Vector3 lowerPos = transform.position + new Vector3(0f, (collider.bounds.extents.y * -1f) + 0.05f, 0f);
			Vector3 lowerRadiusVector = new Vector3(collider.bounds.extents.x, 0f, 0f);
			if(!checkCylinder(lowerPos, lowerRadiusVector, -1.05f, 8, false))
			{
				col.transform.localScale = new Vector3(col.transform.localScale.x, 1f, col.transform.localScale.z);
				transform.position += new Vector3(0f,-0.5f,0f);
				camObj.transform.localPosition += new Vector3(0f,0.25f,0f);
				crouched = false;
			}
			else
			{
				//extend up if there is space
				Vector3 upperPos = transform.position + new Vector3(0f, collider.bounds.extents.y - 0.05f, 0f);
				Vector3 upperRadiusVector = new Vector3(collider.bounds.extents.x, 0f, 0f);

				if(!checkCylinder(upperPos, upperRadiusVector, 1.05f, 8, false))
				{
					col.transform.localScale = new Vector3(col.transform.localScale.x, 1f, col.transform.localScale.z);
					transform.position += new Vector3(0f,0.5f,0f);
					camObj.transform.localPosition += new Vector3(0f,0.25f,0f);
					crouched = false;
				}
			}
		}
	}

	public void setPlayerControls(bool jump, bool respawn, bool reset, bool crouch, bool moveH, bool moveV, bool view)
	{
		allowJump = jump;
		allowRespawn = respawn;
		allowReset = reset;
		allowCrouch = crouch;
		allowMoveHorizontal = moveH;
		allowMoveVertical = moveV;
		
		if(!view)
		{
			GameInfo.info.lockMouseView(false);
		}
		else
		{
			GameInfo.info.unlockMouseView();
		}
	}
		
	private float getVelocity()
	{
		return Vector3.Magnitude(rigidbody.velocity);
	}
	
	public string getXzVelocityString()
	{
		float mag = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z).magnitude;
		string magstr = mag.ToString();
		if(magstr.ToLower().Contains("e"))
		{
			return "0";
		}
		return roundString(magstr, 2);
	}
	
	public string getYVelocityString()
	{
		string v = rigidbody.velocity.y.ToString();
		if(v.ToLower().Contains("e"))
		{
			return "0";
		}
		return roundString(v, 2);
	}
	
	private string getMaxSpeedString()
	{
		return maxSpeed.ToString();
	}
	
	private string getCrouchedString()
	{
		return crouched.ToString();
	}

	private string getGroundString()
	{
		return checkGround().ToString();
	}
	
	private string roundString(string input, int digitsAfterDot)
	{
		if(input.Contains("."))
		{
			return input.Substring(0, input.IndexOf('.') + digitsAfterDot);
		}
		else
		{
			return input;
		}
	}
}
