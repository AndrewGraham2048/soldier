using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class JetPackParticleController : MonoBehaviour
{
	private float litAmount = 0.00f;

	// Use this for initialization
	IEnumerator Start ()
	{
		ThirdPersonController playerController = GetComponent(typeof(ThirdPersonController)) as ThirdPersonController;
		
		// The script ensures an AudioSource component is always attached.
		
		// First, we make sure the AudioSource component is initialized correctly:
		audio.loop = false;
		audio.Stop();
		
		
		// Init the particles to not emit and switch off the spotlights:
		Component[] particles = GetComponentsInChildren<ParticleEmitter>();
		Light childLight = GetComponentInChildren<Light>();
		
		foreach (ParticleEmitter p in particles)
		{
			p.emit = false;
		}
		childLight.enabled = false;
	
		// Once every frame  update particle emission and lights
		while (true)
		{
			var isFlying = playerController.IsJumping();
					
			// handle thruster sound effect
			if (isFlying)
			{
				if (!audio.isPlaying)
				{
					audio.Play();
				}
			}
			else
			{
				audio.Stop();
			}
			
			
			foreach (ParticleEmitter p in particles)
			{
				p.emit = isFlying;
			}
			
			if(isFlying)
				litAmount = Mathf.Clamp01(litAmount + Time.deltaTime * 2);
			else
				litAmount = Mathf.Clamp01(litAmount - Time.deltaTime * 2);
			childLight.enabled = isFlying;
			childLight.intensity = litAmount;
						
			yield return true;
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
}

