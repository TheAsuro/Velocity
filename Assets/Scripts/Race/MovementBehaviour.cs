using Game;
using UI;
using UnityEngine;

namespace Race
{
    public class MovementBehaviour : MonoBehaviour
    {
        private const float NOCLIP_SPEED = 10f;

        public float accel = 200f;
        public float airAccel = 200f;
        public float maxSpeed = 6.4f;
        public float maxAirSpeed = 0.6f;
        public float friction = 8f;
        public float jumpForce = 5f;
        public float maxStepHeight = 0.201f;
        public LayerMask groundLayers;

        private GameObject camObj;
        public bool crouched = false;
        public float lastJumpPress = -1f;
        public float jumpPressDuration = 0.1f;
        private bool onGround = false;

        private bool frozen = false;

        private bool jumpKeyPressed = false;
        private bool crouchKeyPressed = false;

        private Vector3 lastFrameVelocity = Vector3.zero;

        private void Awake()
        {
            camObj = transform.FindChild("Camera").gameObject;
        }

        private void Start()
        {
            DebugWindow debugWindow = GameMenu.SingletonInstance.GetDebugWindow();
            debugWindow.AddDisplayAction(() => "XZ-Speed: " + GetXzVelocityString(), gameObject);
            debugWindow.AddDisplayAction(() => "Y-Speed: " + GetYVelocityString(), gameObject);
            debugWindow.AddDisplayAction(() => "Speed 'limit': " + GetMaxSpeedString(), gameObject);
            debugWindow.AddDisplayAction(() => "Crouched: " + GetCrouchedString(), gameObject);
            debugWindow.AddDisplayAction(() => "On Ground: " + GetGroundString(), gameObject);
        }

        private void Update()
        {
            if (frozen)
                return;

            //Set key states
            jumpKeyPressed = Input.GetButton("Jump");
            crouchKeyPressed = Input.GetButton("Crouch");

            if (jumpKeyPressed)
            {
                lastJumpPress = Time.time;
            }
            SetCrouched(crouchKeyPressed);
        }

        public void Freeze()
        {
            frozen = true;
            GetComponent<Rigidbody>().isKinematic = true;
        }

        public void Unfreeze()
        {
            GetComponent<Rigidbody>().isKinematic = false;
            frozen = false;
        }

        private void FixedUpdate()
        {
            Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            //Friction
            Vector3 tempVelocity = CalculateFriction(GetComponent<Rigidbody>().velocity);

            //Add movement
            tempVelocity += CalculateMovement(input, tempVelocity);

            //Apply
            if (!GetComponent<Rigidbody>().isKinematic)
            {
                GetComponent<Rigidbody>().velocity = tempVelocity;
            }

            lastFrameVelocity = GetComponent<Rigidbody>().velocity;
        }

        public bool Noclip
        {
            set
            {
                GetComponent<Rigidbody>().useGravity = !value;
                GetComponent<Collider>().enabled = !value;
            }
            get { return !GetComponent<Rigidbody>().useGravity; }
        }

        public Vector3 CalculateFriction(Vector3 currentVelocity)
        {
            // Noclip
            if (Noclip)
                return Vector3.zero;

            onGround = CheckGround();
            float speed = currentVelocity.magnitude;

            //Code from https://flafla2.github.io/2015/02/14/bunnyhop.html
            if (!onGround || Input.GetButton("Jump") || speed == 0f)
                return currentVelocity;

            float drop = speed * friction * Time.deltaTime;
            return currentVelocity * (Mathf.Max(speed - drop, 0f) / speed);
        }

        //Do movement input here
        public Vector3 CalculateMovement(Vector2 input, Vector3 velocity)
        {
            //Noclip
            if (Noclip)
                return camObj.transform.rotation * new Vector3(input.x * NOCLIP_SPEED, 0f, input.y * NOCLIP_SPEED);

            onGround = CheckGround();

            //Different acceleration values for ground and air
            float curAccel = accel;
            if (!onGround)
                curAccel = airAccel;

            //Ground speed
            float curMaxSpeed = maxSpeed;

            //Air speed
            if (!onGround)
                curMaxSpeed = maxAirSpeed;

            //Crouched speed on ground
            else if (crouched)
                curMaxSpeed = maxSpeed / 3f;

            //Get rotation input and make it a vector
            Vector3 camRotation = new Vector3(0f, camObj.transform.rotation.eulerAngles.y, 0f);
            Vector3 inputVelocity = Quaternion.Euler(camRotation) *
                                    new Vector3(input.x * curAccel, 0f, input.y * curAccel);

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
            correctVelocity += GetJumpVelocity(velocity.y);

            //Return
            return correctVelocity;
        }

        private Vector3 GetJumpVelocity(float yVelocity)
        {
            Vector3 jumpVelocity = Vector3.zero;

            //Calculate jump
            if (Time.time < lastJumpPress + jumpPressDuration && yVelocity < jumpForce && CheckGround())
            {
                lastJumpPress = -1f;
                // TODO @SOUND @NICE play jumping sound
                jumpVelocity = new Vector3(0f, jumpForce - yVelocity, 0f);
            }

            return jumpVelocity;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag.Equals("Teleporter"))
            {
                Teleporter tp = other.GetComponent<Teleporter>();
                transform.position = tp.target;
                if (tp.applyRotation)
                {
                    transform.rotation = tp.targetRotation;
                }
                if (tp.cancelVelocity)
                {
                    GetComponent<Rigidbody>().velocity = Vector3.zero;
                }
            }
        }

        private void OnCollisionEnter(Collision col)
        {
            bool doStepUp = true;
            float footHeight = transform.position.y - GetComponent<Collider>().bounds.extents.y;

            foreach (ContactPoint p in col.contacts)
            {
                Debug.DrawLine(p.point, p.point + p.normal, Color.red, 1f);

                // TODO @PHYISCS @HACK this whole thing is BS, I can do better
                // TODO this assumes box is aabb, but this isn't always true
                if (p.otherCollider is BoxCollider)
                {
                    if (footHeight + maxStepHeight < p.otherCollider.transform.position.y +
                        p.otherCollider.bounds.extents.y)
                        doStepUp = false;
                }
                else if (p.otherCollider is MeshCollider)
                {
                    // TODO do stuff here
                    doStepUp = false;
                }
            }

            if (doStepUp)
            {
                // TODO check if there is space for the player
                transform.position = new Vector3(transform.position.x,
                    col.collider.transform.position.y + col.collider.bounds.extents.y +
                    GetComponent<Collider>().bounds.extents.y + 0.001f, transform.position.z);
                GetComponent<Rigidbody>().velocity = lastFrameVelocity;
            }
        }

        //Spawn at a specific checkpoint
        public void SpawnPlayer(Checkpoint spawn)
        {
            if (spawn != null)
            {
                transform.position = spawn.GetSpawnPos();
                camObj.transform.rotation = spawn.GetSpawnRot();
                GetComponent<Rigidbody>().velocity = Vector3.zero;
                lastJumpPress = -1f;
            }
            else
                throw new System.InvalidOperationException("Tried to spawn, but no spawnpoint selected.");
        }

        public bool GetJumpKeyPressed()
        {
            return jumpKeyPressed;
        }

        public bool CheckGround()
        {
            Vector3 pos = new Vector3(transform.position.x,
                transform.position.y - GetComponent<Collider>().bounds.extents.y + 0.05f, transform.position.z);
            Vector3 radiusVector = new Vector3(GetComponent<Collider>().bounds.extents.x, 0f, 0f);
            return CheckCylinder(pos, radiusVector, -0.1f, 8);
        }

        // TODO @HACK that doesn't even work properly
        /// <summary>
        /// Doesn't actually check the volume of a cylinder, instead executes a given number of raycasts in a circle
        /// </summary>
        /// <param name="origin">center of the circle from which will be cast</param>
        /// <param name="radiusVector">radius of the circle</param>
        /// <param name="verticalLength">Distance of the raycast, from the center of the player up/down depending on sign</param>
        /// <param name="rayCount">number of vertices the "circle" will have</param>
        /// <param name="dist">Returns shortest distance to ground, or -1f if no ground was touched</param>
        /// <param name="slopeCheck">If true, adds additional check for ground steepness</param>
        /// <returns>true if ground was touched, false if not, also false if slopeCheck is active and the slope is too steep</returns>
        private bool CheckCylinder(Vector3 origin, Vector3 radiusVector, float verticalLength, int rayCount,
            out float dist, bool slopeCheck = true)
        {
            bool tempHit = false;
            float tempDist = -1f;

            for (int i = -1; i < rayCount; i++)
            {
                RaycastHit hit;
                bool hasHit = false;
                float verticalDirection = Mathf.Sign(verticalLength);

                if (i == -1) //Check directly from origin
                {
                    hasHit = Physics.Raycast(origin, Vector3.up * verticalDirection, out hit, Mathf.Abs(verticalLength),
                        groundLayers);
                }
                else //Check in a circle around the origin
                {
                    Vector3 radius = Quaternion.Euler(new Vector3(0f, i * (360f / rayCount), 0f)) * radiusVector;
                    Vector3 circlePoint = origin + radius;

                    hasHit = Physics.Raycast(circlePoint, Vector3.up * verticalDirection, out hit,
                        Mathf.Abs(verticalLength), groundLayers);
                }

                //Collided with something
                if (hasHit)
                {
                    //Assign tempDist to the shortest distance
                    if (tempDist == -1f)
                        tempDist = hit.distance;
                    else if (tempDist > hit.distance)
                        tempDist = hit.distance;

                    //Only return true if the angle is 40° or lower (if slopeCheck is active)
                    if (!slopeCheck || hit.normal.y > 0.75f)
                    {
                        tempHit = true;
                    }
                }
            }

            dist = tempDist;

            return tempHit;
        }

        private bool CheckCylinder(Vector3 origin, Vector3 radiusVector, float verticalLength, int rayCount,
            bool slopeCheck = true)
        {
            float dist;
            return CheckCylinder(origin, radiusVector, verticalLength, rayCount, out dist, slopeCheck);
        }

        private void SetCrouched(bool state)
        {
            MeshCollider col = (MeshCollider) GetComponent<Collider>();

            if (!crouched && state)
            {
                //crouch
                col.transform.localScale = new Vector3(col.transform.localScale.x, 0.5f, col.transform.localScale.z);
                transform.position += new Vector3(0f, 0.5f, 0f);
                camObj.transform.localPosition += new Vector3(0f, -0.25f, 0f);
                crouched = true;
            }
            else if (crouched && !state)
            {
                //extend down if not on ground
                Vector3 lowerPos = transform.position +
                                   new Vector3(0f, (GetComponent<Collider>().bounds.extents.y * -1f) + 0.05f, 0f);
                Vector3 lowerRadiusVector = new Vector3(GetComponent<Collider>().bounds.extents.x, 0f, 0f);
                if (!CheckCylinder(lowerPos, lowerRadiusVector, -1.05f, 8, false))
                {
                    col.transform.localScale = new Vector3(col.transform.localScale.x, 1f, col.transform.localScale.z);
                    transform.position += new Vector3(0f, -0.5f, 0f);
                    camObj.transform.localPosition += new Vector3(0f, 0.25f, 0f);
                    crouched = false;
                }
                else
                {
                    //extend up if there is space
                    Vector3 upperPos = transform.position +
                                       new Vector3(0f, GetComponent<Collider>().bounds.extents.y - 0.05f, 0f);
                    Vector3 upperRadiusVector = new Vector3(GetComponent<Collider>().bounds.extents.x, 0f, 0f);

                    if (!CheckCylinder(upperPos, upperRadiusVector, 1.05f, 8, false))
                    {
                        col.transform.localScale =
                            new Vector3(col.transform.localScale.x, 1f, col.transform.localScale.z);
                        transform.position += new Vector3(0f, 0.5f, 0f);
                        camObj.transform.localPosition += new Vector3(0f, 0.25f, 0f);
                        crouched = false;
                    }
                }
            }
        }

        public string GetXzVelocityString()
        {
            float mag = new Vector3(GetComponent<Rigidbody>().velocity.x, 0f, GetComponent<Rigidbody>().velocity.z)
                .magnitude;
            return mag.ToString("0.00");
        }

        public string GetYVelocityString()
        {
            return GetComponent<Rigidbody>().velocity.y.ToString("0.00");
        }

        private string GetMaxSpeedString()
        {
            return maxSpeed.ToString("0.00");
        }

        private string GetCrouchedString()
        {
            return crouched.ToString();
        }

        private string GetGroundString()
        {
            return CheckGround().ToString();
        }

        public float XzVelocity
        {
            get
            {
                return new Vector3(GetComponent<Rigidbody>().velocity.x, 0f, GetComponent<Rigidbody>().velocity.z)
                    .magnitude;
            }
        }
    }
}