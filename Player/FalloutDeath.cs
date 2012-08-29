using UnityEngine;
using System.Collections;

[AddComponentMenu("Third Person Props/Fallout Death")]
public class FalloutDeath : MonoBehaviour
{
	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	
	void OnTriggerEnter (Collider other)
	{
	}
	
	void Reset ()
	{
		if (collider == null)
			gameObject.AddComponent("BoxCollider");
		collider.isTrigger = true;
	}
}

