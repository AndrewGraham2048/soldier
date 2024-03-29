using UnityEngine;
using System.Collections;

public class ThirdPersonController : MonoBehaviour
{
	// The speed when walking
	public float walkSpeed = 3.0f;
	// after trotAfterSeconds of walking we trot with trotSpeed
	public float trotSpeed = 4.0f;
	// when pressing "Fire3" button (cmd) we start running
	public float runSpeed = 6.0f;
	
	public float inAirControlAcceleration = 3.0f;
	
	// How high do we jump when pressing jump and letting go immediately
	public float jumpHeight = 0.5f;
	// We add extraJumpHeight meters on top when holding the button down longer while jumping
	public float extraJumpHeight = 2.5f;
	
	// The gravity for the character
	public float gravity = 20.0f;
	// The gravity in controlled descent mode
	public float controlledDescentGravity = 2.0f;
	public float speedSmoothing = 10.0f;
	public float rotateSpeed = 500.0f;
	public float trotAfterSeconds = 3.0f;
	
	public bool canJump = true;
	public bool canControlDescent = true;
	public bool canWallJump = false;
	
	private float jumpRepeatTime = 0.05f;
	private float wallJumpTimeout = 0.15f;
	private float jumpTimeout = 0.15f;
	private float groundedTimeout = 0.25f;
	
	// The camera doesnt start following the target immediately but waits for a split second to avoid too much waving around.
	private float lockCameraTimer = 0.0f;
	
	// The current move direction in x-z
	private Vector3 moveDirection = Vector3.zero;
	// The current vertical speed
	private float verticalSpeed = 0.0f;
	// The current x-z move speed
	private float moveSpeed = 0.0f;
	
	// The last collision flags returned from controller.Move
	private CollisionFlags collisionFlags; 
	
	// Are we jumping? (Initiated with jump button and not grounded yet)
	private bool jumping = false;
	private bool jumpingReachedApex = false;
	
	// Are we moving backwards (This locks the camera to not do a 180 degree spin)
	private bool movingBack = false;
	// Is the user pressing any keys?
	private bool isMoving = false;
	// When did the user start walking (Used for going into trot after a while)
	private float walkTimeStart = 0.0f;
	// Last time the jump button was clicked down
	private float lastJumpButtonTime = -10.0f;
	// Last time we performed a jump
	private float lastJumpTime = -1.0f;
	// Average normal of the last touched geometry
	private Vector3 wallJumpContactNormal;
	private float wallJumpContactNormalHeight;
	
	// the height we jumped from (Used to determine for how long to apply extra jump power after jumping.)
	private float lastJumpStartHeight = 0.0f;
	// When did we touch the wall the first time during this jump (Used for wall jumping)
	private float touchWallJumpTime = -1.0f;
	
	private Vector3 inAirVelocity = Vector3.zero;
	
	private float lastGroundedTime = 0.0f;
	
	private float lean = 0.0f;
	private bool slammed = false;
	
	private bool isControllable = true;

	// Use this for initialization
	void Start ()
	{
	
	}
	

	
	
	void Awake ()
	{
		moveDirection = transform.TransformDirection(Vector3.forward);
	}
	
	// This next function responds to the "HidePlayer" message by hiding the player. 
	// The message is also 'replied to' by identically-named functions in the collision-handling scripts.
	// - Used by the LevelStatus script when the level completed animation is triggered.
	
	void HidePlayer()
	{
		GameObject.Find("rootJoint").GetComponent<SkinnedMeshRenderer>().enabled = false; // stop rendering the player.
		isControllable = false;	// disable player controls.
	}
	
	// This is a complementary function to the above. We don't use it in the tutorial, but it's included for
	// the sake of completeness. (I like orthogonal APIs; so sue me!)
	
	void ShowPlayer()
	{
		GameObject.Find("rootJoint").GetComponent<SkinnedMeshRenderer>().enabled = true; // start rendering the player again.
		isControllable = true;	// allow player to control the character again.
	}
	
	
	void UpdateSmoothedMovementDirection ()
	{
		var cameraTransform = Camera.main.transform;
		var grounded = IsGrounded();
		
		// Forward vector relative to the camera along the x-z plane	
		var forward = cameraTransform.TransformDirection(Vector3.forward);
		forward.y = 0;
		forward = forward.normalized;
	
		// Right vector relative to the camera
		// Always orthogonal to the forward vector
		var right = new Vector3(forward.z, 0, -forward.x);
	
		Vector3 accelerator = Input.acceleration;
		float v = (accelerator.x+0.35f)*4.0f;
		float h = (-accelerator.y)*4.0f;
	//	var v = Input.GetAxisRaw("Vertical");
	//	var h = Input.GetAxisRaw("Horizontal");
	
		// Are we moving backwards or looking backwards
		if (v < -0.2)
			movingBack = true;
		else
			movingBack = false;
		
		var wasMoving = isMoving;
		isMoving = Mathf.Abs (h) > 0.1 || Mathf.Abs (v) > 0.1;
			
		// Target direction relative to the camera
		var targetDirection = h * right + v * forward;
		
		// Grounded controls
		if (grounded)
		{
			// Lock camera for short period when transitioning moving & standing still
			lockCameraTimer += Time.deltaTime;
			if (isMoving != wasMoving)
				lockCameraTimer = 0.0f;
	
			// We store speed and direction seperately,
			// so that when the character stands still we still have a valid forward direction
			// moveDirection is always normalized, and we only update it if there is user input.
			if (targetDirection != Vector3.zero)
			{
				// If we are really slow, just snap to the target direction
				if (moveSpeed < walkSpeed * 0.9 && grounded)
				{
					moveDirection = targetDirection.normalized;
				}
				// Otherwise smoothly turn towards it
				else
				{
					moveDirection = Vector3.RotateTowards(moveDirection, targetDirection, rotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000);
					
					moveDirection = moveDirection.normalized;
				}
			}
			
			// Smooth the speed based on the current target direction
			var curSmooth = speedSmoothing * Time.deltaTime;
			
			// Choose target speed
			//* We want to support analog input but make sure you cant walk faster diagonally than just forward or sideways
			var targetSpeed = Mathf.Min(targetDirection.magnitude, 1.0f);
		
			// Pick speed modifier
			if (Input.GetButton ("Fire3"))
			{
				targetSpeed *= runSpeed;
			}
			else if (Time.time - trotAfterSeconds > walkTimeStart)
			{
				targetSpeed *= trotSpeed;
			}
			else
			{
				targetSpeed *= walkSpeed;
			}
			
			moveSpeed = Mathf.Lerp(moveSpeed, targetSpeed, curSmooth);
			
			// Reset walk time start when we slow down
			if (moveSpeed < walkSpeed * 0.3f)
				walkTimeStart = Time.time;
		}
		// In air controls
		else
		{
			// Lock camera while in air
			if (jumping)
				lockCameraTimer = 0.0f;
	
			if (isMoving)
				inAirVelocity += targetDirection.normalized * Time.deltaTime * inAirControlAcceleration;
		}
	}
	
	
	void ApplyWallJump ()
	{
		// We must actually jump against a wall for this to work
		if (!jumping)
			return;
	
		// Store when we first touched a wall during this jump
		if (collisionFlags == CollisionFlags.CollidedSides)
		{
			touchWallJumpTime = Time.time;
		}
	
		// The user can trigger a wall jump by hitting the button shortly before or shortly after hitting the wall the first time.
		var mayJump = lastJumpButtonTime > touchWallJumpTime - wallJumpTimeout && lastJumpButtonTime < touchWallJumpTime + wallJumpTimeout;
		if (!mayJump)
			return;
		
		// Prevent jumping too fast after each other
		if (lastJumpTime + jumpRepeatTime > Time.time)
			return;
		
			
		if (Mathf.Abs(wallJumpContactNormal.y) < 0.2f)
		{
			wallJumpContactNormal.y = 0.0f;
			moveDirection = wallJumpContactNormal.normalized;
			// Wall jump gives us at least trotspeed
			moveSpeed = Mathf.Clamp(moveSpeed * 1.5f, trotSpeed, runSpeed);
		}
		else
		{
			moveSpeed = 0;
		}
		
		verticalSpeed = CalculateJumpVerticalSpeed (jumpHeight);
		DidJump();
		SendMessage("DidWallJump", null, SendMessageOptions.DontRequireReceiver);
	}
	
	void ApplyJumping ()
	{
		// Prevent jumping too fast after each other
		if (lastJumpTime + jumpRepeatTime > Time.time)
			return;
	
		if (IsGrounded()) {
			// Jump
			// - Only when pressing the button down
			// - With a timeout so you can press the button slightly before landing		
			if (canJump && Time.time < lastJumpButtonTime + jumpTimeout) {
				verticalSpeed = CalculateJumpVerticalSpeed (jumpHeight);
				SendMessage("DidJump", SendMessageOptions.DontRequireReceiver);
			}
		}
	}
	
	
	void ApplyGravity ()
	{
		if (isControllable)	// don't move player at all if not controllable.
		{
			// Apply gravity
			var jumpButton = Input.GetButton("Jump");
			
			// * When falling down we use controlledDescentGravity (only when holding down jump)
			var controlledDescent = canControlDescent && verticalSpeed <= 0.0f && jumpButton && jumping;
			
			// When we reach the apex of the jump we send out a message
			if (jumping && !jumpingReachedApex && verticalSpeed <= 0.0f)
			{
				jumpingReachedApex = true;
				SendMessage("DidJumpReachApex", SendMessageOptions.DontRequireReceiver);
			}
		
			// * When jumping up we don't apply gravity for some time when the user is holding the jump button
			//   This gives more control over jump height by pressing the button longer
			var extraPowerJump =  IsJumping () && verticalSpeed > 0.0f && jumpButton && transform.position.y < lastJumpStartHeight + extraJumpHeight;
			
			if (controlledDescent)			
				verticalSpeed -= controlledDescentGravity * Time.deltaTime;
			else if (extraPowerJump)
				return;
			else if (IsGrounded ())
				verticalSpeed = 0.0f;
			else
				verticalSpeed -= gravity * Time.deltaTime;
		}
	}
	
	float CalculateJumpVerticalSpeed (float targetJumpHeight)
	{
		// From the jump height and gravity we deduce the upwards speed 
		// for the character to reach at the apex.
		return Mathf.Sqrt(2 * targetJumpHeight * gravity);
	}
	
	void DidJump ()
	{
		jumping = true;
		jumpingReachedApex = false;
		lastJumpTime = Time.time;
		lastJumpStartHeight = transform.position.y;
		touchWallJumpTime = -1;
		lastJumpButtonTime = -10;
	}
		
	
	// Update is called once per frame
	void Update ()
	{
		if (!isControllable)
		{
			// kill all inputs if not controllable.
			Input.ResetInputAxes();
		}
	
		if (Input.GetButtonDown ("Jump"))
		{
			lastJumpButtonTime = Time.time;
		}
	
		UpdateSmoothedMovementDirection();
		
		// Apply gravity
		// - extra power jump modifies gravity
		// - controlledDescent mode modifies gravity
		ApplyGravity ();
	
		// Perform a wall jump logic
		// - Make sure we are jumping against wall etc.
		// - Then apply jump in the right direction)
		if (canWallJump)
			ApplyWallJump();
	
		// Apply jumping logic
		ApplyJumping ();
		
		// Calculate actual motion
		var movement = moveDirection * moveSpeed + new Vector3 (0, verticalSpeed, 0) + inAirVelocity;
		movement *= Time.deltaTime;
		
		// Move the controller
		CharacterController controller = GetComponent<CharacterController>();
		wallJumpContactNormal = Vector3.zero;
		collisionFlags = controller.Move(movement);
		
		// Set rotation to the move direction
		if (IsGrounded())
		{
			if(slammed) // we got knocked over by an enemy. We need to reset some stuff
			{
				slammed = false;
				controller.height = 2;
				Vector3 pos = transform.position;
				pos.y += 0.75f;
				transform.position = pos;
			}
			
			transform.rotation = Quaternion.LookRotation(moveDirection);
				
		}	
		else
		{
			if(!slammed)
			{
				var xzMove = movement;
				xzMove.y = 0;
				if (xzMove.sqrMagnitude > 0.001f)
				{
					transform.rotation = Quaternion.LookRotation(xzMove);
				}
			}
		}	
		
		// We are in jump mode but just became grounded
		if (IsGrounded())
		{
			lastGroundedTime = Time.time;
			inAirVelocity = Vector3.zero;
			if (jumping)
			{
				jumping = false;
				SendMessage("DidLand", SendMessageOptions.DontRequireReceiver);
			}
		}
	}
	
	void OnControllerColliderHit (ControllerColliderHit hit )
	{
	//	Debug.DrawRay(hit.point, hit.normal);
		if (hit.moveDirection.y > 0.01f) 
			return;
		wallJumpContactNormal = hit.normal;
	}
	
	public float GetSpeed () {
		return moveSpeed;
	}
	
	public bool IsJumping () {
		return jumping && !slammed;
	}
	
	public bool IsGrounded () {
		return (collisionFlags & CollisionFlags.CollidedBelow) != 0;
	}
	
	void SuperJump (float height)
	{
		verticalSpeed = CalculateJumpVerticalSpeed (height);
		collisionFlags = CollisionFlags.None;
		SendMessage("DidJump", SendMessageOptions.DontRequireReceiver);
	}
	
	void SuperJump (float height, Vector3 jumpVelocity)
	{
		verticalSpeed = CalculateJumpVerticalSpeed (height);
		inAirVelocity = jumpVelocity;
		
		collisionFlags = CollisionFlags.None;
		SendMessage("DidJump", SendMessageOptions.DontRequireReceiver);
	}
	
	void Slam (Vector3 direction)
	{
		verticalSpeed = CalculateJumpVerticalSpeed (1);
		inAirVelocity = direction * 6;
		direction.y = 0.6f;
		Quaternion.LookRotation(-direction);
		CharacterController controller = GetComponent<CharacterController>();
		controller.height = 0.5f;
		slammed = true;
		collisionFlags = CollisionFlags.None;
		SendMessage("DidJump", SendMessageOptions.DontRequireReceiver);
	}
	
	Vector3 GetDirection () {
		return moveDirection;
	}
	
	public bool IsMovingBackwards () {
		return movingBack;
	}
	
	public float GetLockCameraTimer () 
	{
		return lockCameraTimer;
	}
	
	public bool IsMoving ()
	{
		return Mathf.Abs(Input.GetAxisRaw("Vertical")) + Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.5f;
	}
	
	public bool HasJumpReachedApex ()
	{
		return jumpingReachedApex;
	}
	
	public bool IsGroundedWithTimeout ()
	{
		return lastGroundedTime + groundedTimeout > Time.time;
	}
	
	public bool IsControlledDescent ()
	{
		// * When falling down we use controlledDescentGravity (only when holding down jump)
		var jumpButton = Input.GetButton("Jump");
		return canControlDescent && verticalSpeed <= 0.0f && jumpButton && jumping;
	}
	
	void Reset ()
	{
		gameObject.tag = "Player";
	}
}

