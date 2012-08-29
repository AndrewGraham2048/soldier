using UnityEngine;
using System.Collections;

[AddComponentMenu("Third Person Camera/Mouse Orbit")]
public class OverheadCamera : MonoBehaviour
{
	public Transform target;
	public Vector3 targetOffset = Vector3.zero;
	public float distance = 4.0f;
	
	public LayerMask lineOfSightMask = 0;
	public float closerRadius = 0.2f;
	public float closerSnapLag = 0.2f;
	
	public float xSpeed = 200.0f;
	public float ySpeed = 80.0f;
	
	public float cameraTilt = 75.0f;
	
	private float currentDistance = 10.0f;
	private float x = 0.0f;
	private float y = 0.0f;
	private float distanceVelocity = 0.0f;
	
	
	
	// Use this for initialization
	void Start ()
	{
	    var angles = transform.eulerAngles;
	    x = angles.y;
	    y = angles.x;
		currentDistance = distance;
		
		// Make the rigid body not change rotation
	   	if (rigidbody)
			rigidbody.freezeRotation = true;
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	
	void LateUpdate () 
	{
	    if (target) 
		{
	        x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
	        y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
	 		
	 		y = cameraTilt;
	 		x = 0;
	 		       
	        var rotation = Quaternion.Euler(y, x, 0);
	        var targetPos = target.position + targetOffset;
	        var direction = rotation * -Vector3.forward;
			
	        var targetDistance = AdjustLineOfSight(targetPos, direction);
			currentDistance = Mathf.SmoothDamp(currentDistance, targetDistance, ref distanceVelocity, closerSnapLag * .3f);
	        
	        transform.rotation = rotation;
	        transform.position = targetPos + direction * currentDistance;
	    }
	}
	
	float AdjustLineOfSight (Vector3 target, Vector3 direction)
	{
		RaycastHit hit;
		if (Physics.Raycast (target, direction, out hit, distance, lineOfSightMask.value))
			return hit.distance - closerRadius;
		else
			return distance;
	}
	
	static float ClampAngle (float angle, float min, float max) 
	{
		if (angle < -360)
			angle += 360;
		if (angle > 360)
			angle -= 360;
		return Mathf.Clamp (angle, min, max);
	}
	
}

