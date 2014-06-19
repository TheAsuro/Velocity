using UnityEngine;
using System.Collections;

public abstract class Movement : MonoBehaviour
{
	public bool canMove = true;
	public bool autojump = true;
	public float speed = 1f;
	public float maxSpeed = 10f;
	public float jumpForce = 1f;
	public LayerMask groundLayers;

	public GameObject camObj;
	public bool crouched = false;
	public float lastJumpPress = -1f;

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
		if(Input.GetButtonDown("Jump") || autojump && Input.GetButton("Jump"))
		{
			lastJumpPress = Time.time;
		}
		
		if(Input.GetButtonDown("Respawn"))
		{
			respawnPlayer();
		}
		if(Input.GetButtonDown("Reset"))
		{
			resetPlayer();
		}
		if(Input.GetButton("Crouch"))
		{
			setCrouched(true);
		}
		else
		{
			setCrouched(false);
		}
	}

	void FixedUpdate()
	{
		if(canMove)
		{
			Vector3 additionalVelocity = calculateAdditionalVelocity();
			
			//Apply
			if(!rigidbody.isKinematic)
			{
				rigidbody.velocity += additionalVelocity;
			}
		}
	}

	public virtual Vector3 calculateAdditionalVelocity()
	{
		return Vector3.zero;
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
	}

	private void spawnPlayer(Respawn spawn)
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
			print("Tried to respawn, but no spawnpoint selected. RIP :(");
		}
	}
	
	private void respawnPlayer()
	{
		spawnPlayer(GameInfo.info.getCurrentSpawn());
	}

	private void resetPlayer()
	{
		spawnPlayer(GameInfo.info.getFirstSpawn());
	}
	
	public bool checkGround()
	{
		Vector3 origin = new Vector3(transform.position.x, transform.position.y - collider.bounds.extents.y + 0.05f, transform.position.z);
		RaycastHit hit;
		bool hasHit = Physics.Raycast(origin, -Vector3.up, out hit, 0.1f, groundLayers);
		//Collided with something
		if(hasHit)
		{
			//Something's angle is less than 45° relative to the ground
			if(hit.collider.transform.rotation.eulerAngles.x >= 45f)
			{
				return true;
			}
		}
		return false;
	}
	
	private void setCrouched(bool state)
	{
		CapsuleCollider col = (CapsuleCollider)collider;

		if(!crouched && state)
		{
			//crouch
			col.height = 1f;
			transform.position += new Vector3(0f,-0.5f,0f);
			crouched = true;
		}
		else if(crouched && !state)
		{
			//uncrouch
			Ray ray = new Ray(transform.position - new Vector3(0f, -0.5f, 0f), Vector3.up);
			if(!Physics.SphereCast(ray, 0.5f, 2f, groundLayers)) //Do some sort of raycast here
			{
				col.height = 2f;
				transform.position += new Vector3(0f,0.5f,0f);
				crouched = false;
			}
		}
	}
		
	private float getVelocity()
	{
		return Vector3.Magnitude(rigidbody.velocity);
	}
	
	private string getXzVelocityString()
	{
		float mag = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z).magnitude;
		string magstr = mag.ToString();
		if(magstr.ToLower().Contains("e"))
		{
			return "0";
		}
		return roundString(magstr, 2);
	}
	
	private string getYVelocityString()
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
