using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class MyEnemyPoliceGuy : MonoBehaviour
{
	public float attackTurnTime = 0.7f;
	public float rotateSpeed = 120.0f;
	public float attackDistance = 17.0f;
	public float extraRunTime = 2.0f;
	public float damage = 1.0f;
	
	public float attackSpeed = 5.0f;
	public float attackRotateSpeed = 20.0f;
	
	public float idleTime = 1.6f;
	
	Vector3 punchPosition = new Vector3 (0.4f, 0.0f, 0.7f);
	public float punchRadius = 1.1f;
	public Transform bulletPrefab;
	
	// sounds
	public AudioClip idleSound;	// played during "idle" state.
	public AudioClip attackSound;	// played during the seek and attack modes.
	
	private float attackAngle = 10.0f;
	private bool isAttacking = false;
	private float lastPunchTime = 0.0f;
	private float bulletFireTimer = 0.0f;
	
	public Transform target;
	
	private Rigidbody rigidBody;
	
	private LevelStatus levelStateMachine;


	// Use this for initialization
	IEnumerator Start ()
	{
		// Cache a reference to the rigidbody
		rigidBody = (Rigidbody)GetComponent(typeof(Rigidbody));
	
		// Cache a link to LevelStatus state machine script:
		levelStateMachine = (LevelStatus)GameObject.Find("/Level").GetComponent(typeof(LevelStatus));
	
		if (!levelStateMachine)
		{
			Debug.Log("EnemyPoliceGuy: ERROR! NO LEVEL STATUS SCRIPT FOUND.");
		}
	
		if (!target)
			target = GameObject.FindWithTag("Player").transform;
		
		animation.wrapMode = WrapMode.Loop;
	
		// Setup animations
		animation.Play("idle");
		animation["threaten"].wrapMode = WrapMode.Once;
		animation["turnjump"].wrapMode = WrapMode.Once;
		animation["gothit"].wrapMode = WrapMode.Once;
		animation["gothit"].layer = 1;
		
		// initialize audio clip. Make sure it's set to the "idle" sound.
		audio.clip = idleSound;
		
		yield return new WaitForSeconds(Random.value);
		
		// Just attack for now
		while (true)	
		{
			Debug.Log("loop");
			// Don't do anything when idle. And wait for player to be in range!
			// This is the perfect time for the player to attack us
			yield return StartCoroutine("Idle");
	
			// Prepare, turn to player and attack him
			yield return StartCoroutine("Attack");
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	
	
	IEnumerator Idle ()
	{
		Debug.Log("idle");
		// if idling sound isn't already set up, set it and start it playing.
		if (idleSound)
		{
			if (audio.clip != idleSound)
			{
				audio.Stop();
				audio.clip = idleSound;
				audio.loop = true;
				audio.Play();	// play the idle sound.
			}
		}
		
		// Don't do anything when idle
		// The perfect time for the player to attack us
		yield return new WaitForSeconds(idleTime);
	
		// And if the player is really far away.
		// We just idle around until he comes back
		// unless we're dying, in which case we just keep idling.
		while (true)
		{
			rigidBody.velocity = new Vector3(0,0,0);
			//characterController.SimpleMove(Vector3.zero);
			yield return new WaitForSeconds(0.2f);
			
			var offset = transform.position - target.position;
			
			// if player is in range again, stop lazyness
			// Good Hunting!		
			if (offset.magnitude < attackDistance)
				return true;
		}
	} 
	
	float RotateTowardsPosition (Vector3 targetPos, float rotateSpeed)
	{
		// Compute relative point and get the angle towards it
		Vector3 relative = transform.InverseTransformPoint(targetPos);
		float angle = Mathf.Atan2 (relative.x, relative.z) * Mathf.Rad2Deg;
	
		// Clamp it with the max rotation speed
		float maxRotation = rotateSpeed * Time.deltaTime;
		float clampedAngle = Mathf.Clamp(angle, -maxRotation, maxRotation);
		// Rotate
		transform.Rotate(0, clampedAngle, 0);
		// Return the current angle
		return angle;
	}
	
	
	void FireBullet()
	{
		Transform pt;
		pt = GameObject.Find("player").transform;
	    Vector3 relativePos = pt.position - transform.position;
	    Quaternion rotation = Quaternion.LookRotation(relativePos);
		Transform thing = (Transform)Instantiate( bulletPrefab, transform.position + new Vector3(0,1,0), rotation );
		bullet link;
		link = (bullet)thing.GetComponent(typeof(bullet));
		link.SetEnemyBullet(true);
	}
	
	
	void UpdateBulletFiring()
	{
		bulletFireTimer += Time.deltaTime;
		if (bulletFireTimer > 0.1f)
		{
			bulletFireTimer = 0.0f;
			FireBullet();
		}
	}
	
	IEnumerator Attack ()
	{
		isAttacking = true;
		
		if (attackSound)
		{
			if (audio.clip != attackSound)
			{
				audio.Stop();	// stop the idling audio so we can switch out the audio clip.
				audio.clip = attackSound;
				audio.loop = true;	// change the clip, then play
				audio.Play();
			}
		}
		
		// Already queue up the attack run animation but set it's blend wieght to 0
		// it gets blended in later
		// it is looping so it will keep playing until we stop it.
		animation.Play("attackrun");
		
		// First we wait for a bit so the player can prepare while we turn around
		// As we near an angle of 0, we will begin to move
		float angle;
		angle = 180.0f;
		float time;
		time = 0.0f;
		Vector3 direction;
		while (angle > 5)	// || time < attackTurnTime)
		{
			time += Time.deltaTime;
			angle = Mathf.Abs(RotateTowardsPosition(target.position, rotateSpeed));
			var move = Mathf.Clamp01((90 - angle) / 90);
			
			// depending on the angle, start moving
			animation["attackrun"].weight = animation["attackrun"].speed = move;
			direction = transform.TransformDirection(Vector3.forward * attackSpeed * move);
			//characterController.SimpleMove(direction);
			//rigidBody.velocity = direction;
			rigidBody.AddForce(direction);
			
			yield return true;
		}
		
		// Run towards player
		//Debug.Log("RUN! RUN! RUN!");
		float timer = 0.0f;
		bool lostSight = false;
		while (timer < extraRunTime)
		{
			UpdateBulletFiring();
		
			angle = RotateTowardsPosition(target.position, attackRotateSpeed);
				
			// The angle of our forward direction and the player position is larger than 50 degrees
			// That means he is out of sight
			if (Mathf.Abs(angle) > 40)
				lostSight = true;
				
			// If we lost sight then we keep running for some more time (extraRunTime). 
			// then stop attacking 
			if (lostSight)
				timer += Time.deltaTime;	
			
			// Just move forward at constant speed
			direction = transform.TransformDirection(Vector3.forward * attackSpeed);
			//characterController.SimpleMove(direction);
			//rigidBody.velocity = direction;
			rigidBody.AddForce(direction);
	
			// Keep looking if we are hitting our target
			// If we are, knock them out of the way dealing damage
			Vector3 pos = transform.TransformPoint(punchPosition);
			if(Time.time > lastPunchTime + 0.3 && (pos - target.position).magnitude < punchRadius)
			{
				// deal damage
				//target.SendMessage("ApplyDamage", damage);
				// knock the player back and to the side
				Vector3 slamDirection = transform.InverseTransformDirection(target.position - transform.position);
				slamDirection.y = 0;
				slamDirection.z = 1;
				if (slamDirection.x >= 0)
					slamDirection.x = 1;
				else
					slamDirection.x = -1;
				target.SendMessage("Slam", transform.TransformDirection(slamDirection));
				lastPunchTime = Time.time;
			}
	
			// We are not actually moving forward.
			// This probably means we ran into a wall or something. Stop attacking the player.
			//if (characterController.velocity.magnitude < attackSpeed * 0.3)
	//		if (rigidBody.velocity.magnitude < attackSpeed * 0.3)
	//			break;
			
			// yield for one frame
			yield return true;
		}
	
		isAttacking = false;
		
		// Now we can go back to playing the idle animation
		animation.CrossFade("idle");
	}
	
	void ApplyDamage ()
	{
		animation.CrossFade("gothit");
	}
	
	void OnDrawGizmosSelected ()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere (transform.TransformPoint(punchPosition), punchRadius);
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere (transform.position, attackDistance);
	}
	
}

