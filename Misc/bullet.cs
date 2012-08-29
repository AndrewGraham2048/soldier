using UnityEngine;
using System.Collections;

public class bullet : MonoBehaviour
{
	public float bulletLife;
	public float bulletSpeed;
	private float startTime;
	public float damage = 1.0f;
	private bool isEnemyBullet = false;

	// Use this for initialization
	void Start ()
	{
	
	}
	
	void Awake()
	{
		startTime = Time.time;
		Vector3 vel = new Vector3(0.0f,0.0f,bulletSpeed);
		vel = rigidbody.rotation * vel;
	   	rigidbody.velocity = vel; //Vector3(0,0,bulletSpeed);	//spawnPoint.rigidbody.velocity + spawnTransform.TransformDirection( Vector3.up * velocity );
	
		if (audio) 
		{
			audio.Play();
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		if ((Time.time - startTime) >= bulletLife)
		{
			Destroy(gameObject);
		}
	}
	
	public void SetEnemyBullet(bool b)
	{
		isEnemyBullet = b;
		Debug.Log("TEST!!");
	}
	
	void OnTriggerEnter(Collider col) 
	{
		Debug.Log("Bullet hit something!");
		Debug.Log(col.tag);
		
		if (((col.tag == "Enemy") && !isEnemyBullet))	// || ((col.tag == "Player") && isEnemyBullet))
		{
			//Instantiate(hitEffect, hit.point, Quaternion.identity);
			col.SendMessage("ApplyDamage", damage, SendMessageOptions.DontRequireReceiver);
			//lastHitTime = Time.time;
			Destroy(gameObject);
		}
		else if ((col.tag != "Player") && (col.tag != "Enemy") && (col.tag != "Bullet")) 
			Destroy(gameObject);
		
	    // Play a sound if the coliding objects had a big impact.        
	//    if (collision.relativeVelocity.magnitude > 2)
	//        audio.Play();
	}

}

