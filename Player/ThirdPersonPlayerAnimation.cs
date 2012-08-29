using UnityEngine;
using System.Collections;

[AddComponentMenu ("Third Person Player/Third Person Player Animation")]
public class ThirdPersonPlayerAnimation : MonoBehaviour
{
	public float runSpeedScale = 1.0f;
	public float walkSpeedScale = 1.0f;

	// Use this for initialization
	void Start ()
	{
		// By default loop all animations
		animation.wrapMode = WrapMode.Loop;
	
		animation["run"].layer = -1;
		animation["walk"].layer = -1;
		animation["idle"].layer = -2;
		animation.SyncLayer(-1);
	
		animation["ledgefall"].layer = 9;	
		animation["ledgefall"].wrapMode = WrapMode.Loop;
	
	
		// The jump animation is clamped and overrides all others
		animation["jump"].layer = 10;
		animation["jump"].wrapMode = WrapMode.ClampForever;
	
		animation["jumpfall"].layer = 10;	
		animation["jumpfall"].wrapMode = WrapMode.ClampForever;
	
		// This is the jet-pack controlled descent animation.
		animation["jetpackjump"].layer = 10;	
		animation["jetpackjump"].wrapMode = WrapMode.ClampForever;
	
		animation["jumpland"].layer = 10;	
		animation["jumpland"].wrapMode = WrapMode.Once;
	
		animation["walljump"].layer = 11;	
		animation["walljump"].wrapMode = WrapMode.Once;
	
		// we actually use this as a "got hit" animation
		animation["buttstomp"].speed = 0.15f;
		animation["buttstomp"].layer = 20;
		animation["buttstomp"].wrapMode = WrapMode.Once;	
		var punch = animation["punch"];
		punch.wrapMode = WrapMode.Once;
	
		// We are in full control here - don't let any other animations play when we start
		animation.Stop();
		animation.Play("idle");
	}
	
	// Update is called once per frame
	void Update ()
	{
		ThirdPersonController playerController = GetComponent<ThirdPersonController>();
		float currentSpeed = playerController.GetSpeed();
	
		// Fade in run
		if (currentSpeed > playerController.walkSpeed)
		{
			animation.CrossFade("run");
			// We fade out jumpland quick otherwise we get sliding feet
			animation.Blend("jumpland", 0);
		}
		// Fade in walk
		else if (currentSpeed > 0.1f)
		{
			animation.CrossFade("walk");
			// We fade out jumpland realy quick otherwise we get sliding feet
			animation.Blend("jumpland", 0);
		}
		// Fade out walk and run
		else
		{
			animation.Blend("walk", 0.0f, 0.3f);
			animation.Blend("run", 0.0f, 0.3f);
			animation.Blend("run", 0.0f, 0.3f);
		}
		
		animation["run"].normalizedSpeed = runSpeedScale;
		animation["walk"].normalizedSpeed = walkSpeedScale;
		
		if (playerController.IsJumping ())
		{
			if (playerController.IsControlledDescent())
			{
				animation.CrossFade ("jetpackjump", 0.2f);
			}
			else if (playerController.HasJumpReachedApex ())
			{
				animation.CrossFade ("jumpfall", 0.2f);
			}
			else
			{
				animation.CrossFade ("jump", 0.2f);
			}
		}
		// We fell down somewhere
		else if (!playerController.IsGroundedWithTimeout())
		{
			animation.CrossFade ("ledgefall", 0.2f);
		}
		// We are not falling down anymore
		else
		{
			animation.Blend ("ledgefall", 0.0f, 0.2f);
		}
	}
	
	void DidLand () 
	{
		animation.Play("jumpland");
	}
	
	void DidButtStomp () 
	{
		animation.CrossFade("buttstomp", 0.1f);
		animation.CrossFadeQueued("jumpland", 0.2f);
	}
	
	IEnumerator Slam ()
	{
		animation.CrossFade("buttstomp", 0.2f);
		ThirdPersonController playerController = GetComponent<ThirdPersonController>();
		while(!playerController.IsGrounded())
		{
			yield return true;	
		}
		animation.Blend("buttstomp", 0, 0);
	}
	
	
	void DidWallJump ()
	{
		// Wall jump animation is played without fade.
		// We are turning the character controller 180 degrees around when doing a wall jump so the animation accounts for that.
		// But we really have to make sure that the animation is in full control so 
		// that we don't do weird blends between 180 degree apart rotations
		animation.Play ("walljump");
	}

}

