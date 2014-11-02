using UnityEngine;
using System.Collections;

public abstract class Movement : MonoBehaviour
{
	public float speed = 1f;
	public float airSpeed = 1f;
	public float maxSpeed = 10f;
	public float frictionMultiplier = 0.9f;
	public float jumpForce = 1f;
	public LayerMask groundLayers;

	public GameObject camObj;
	public bool crouched = false;
	public float lastJumpPress = -1f;
	public float jumpPressDuration = 0.1f;

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

		//Add movement
		Vector3 additionalVelocity = calculateAdditionalVelocity(input);

		//Friction and other stuff
		Vector3 tempVelocity = rigidbody.velocity + additionalVelocity;
		tempVelocity = overrideVelocity(tempVelocity);
			
		//Apply
		if(!rigidbody.isKinematic)
		{
			rigidbody.velocity = tempVelocity;
		}

		FixedMoveUpdate();
	}

	public virtual Vector3 calculateAdditionalVelocity(Vector2 input)
	{
		return Vector3.zero;
	}

	public virtual Vector3 overrideVelocity(Vector3 input)
	{
		return input;
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
	
	//Spawns the player at the last checkpoint
	private void respawnPlayer(bool resetAtStart)
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
	private bool checkCylinder(Vector3 origin, Vector3 radiusVector, float verticalLength, int rayCount)
	{
		bool tempHit = false;

		for(int i = 0; i < rayCount; i++)
		{
			Vector3 radius = Quaternion.Euler(new Vector3(0f, i * (360f / rayCount), 0f)) * radiusVector;
			Vector3 circlePoint = origin + radius;
			float verticalDirection = Mathf.Sign(verticalLength);

			RaycastHit hit;
			bool hasHit = Physics.Raycast(circlePoint, Vector3.up * verticalDirection, out hit, Mathf.Abs(verticalLength), groundLayers);
			//Collided with something
			if(hasHit)
			{
				//Only return true if the angle is 40° or lower
				if(hit.normal.y > 0.75f)
				{
					tempHit = true;
				}
			}
		}

		if(tempHit) { return true; }
		return false;
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
			if(!checkCylinder(lowerPos, lowerRadiusVector, -1.05f, 8))
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
				if(!checkCylinder(upperPos, upperRadiusVector, 1.05f, 8))
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
