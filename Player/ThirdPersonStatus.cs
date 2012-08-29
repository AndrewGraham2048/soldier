using UnityEngine;
using System.Collections;

public class ThirdPersonStatus : MonoBehaviour
{
	public int health = 6;
	public int maxHealth = 6;
	public int lives = 4;
	
	// sound effects.
	public AudioClip struckSound;
	public AudioClip deathSound;
	
	private LevelStatus levelStateMachine;		// link to script that handles the level-complete sequence.
	
	private int remainingItems;	// total number to pick up on this level. Grabbed from LevelStatus.

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	
	void Awake()
	{
		
		levelStateMachine = (LevelStatus)FindObjectOfType(typeof(LevelStatus));
		if (!levelStateMachine)
			Debug.Log("No link to Level Status");
		
		remainingItems = levelStateMachine.itemsNeeded;
	}

	// Utility function used by HUD script:
	public int GetRemainingItems()
	{
		return remainingItems;
	}
	
	void ApplyDamage (int damage)
	{
		if (struckSound)
			AudioSource.PlayClipAtPoint(struckSound, transform.position);	// play the 'player was struck' sound.
	
		health -= damage;
		if (health <= 0)
		{
			SendMessage("Die");
		}
	}
	
	
	void AddLife (int powerUp)
	{
		lives += powerUp;
		health = maxHealth;
	}
	
	void AddHealth (int powerUp)
	{
		health += powerUp;
		
		if (health>maxHealth)		// We can only show six segments in our HUD.
		{
			health=maxHealth;	
		}		
	}
	
	
	void FoundItem (int numFound)
	{
		remainingItems-= numFound;
	
	// NOTE: We are deliberately not clamping this value to zero. 
	// This allows for levels where the number of pickups is greater than the target number needed. 
	// This also lets us speed up the testing process by temporarily reducing the collecatbles needed. 
	// Our HUD will clamp to zero for us.
	
	}
	
	
	void FalloutDeath ()
	{
		Die();
		return;
	}
	
	void Die ()
	{
		// play the death sound if available.
		if (deathSound)
		{
			AudioSource.PlayClipAtPoint(deathSound, transform.position);
	
		}
			
		lives--;
		health = maxHealth;
		
		if(lives < 0)
			Application.LoadLevel("GameOver");
		
		return;
/*		
		// If we've reached here, the player still has lives remaining, so respawn.
		Vector3 respawnPosition = Respawn.currentRespawn.transform.position;
		Camera.main.transform.position = respawnPosition - (transform.forward * 4) + Vector3.up;	// reset camera too
		// Hide the player briefly to give the death sound time to finish...
		SendMessage("HidePlayer");
		
		// Relocate the player. We need to do this or the camera will keep trying to focus on the (invisible) player where he's standing on top of the FalloutDeath box collider.
		transform.position = respawnPosition + Vector3.up;
	
		yield return new WaitForSeconds(1.6f);	// give the sound time to complete. 
		
		// (NOTE: "HidePlayer" also disables the player controls.)
	
		SendMessage("ShowPlayer");	// Show the player again, ready for...	
		// ... the respawn point to play it's particle effect
		Respawn.currentRespawn.FireEffect ();
*/
	}
}

