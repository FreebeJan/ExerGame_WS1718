using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationBehavior : MonoBehaviour 
{
	// Speed in degrees per second
	public float Speed = 10.0f;
	void Start () 
	{
		
	}
	
	void Update () 
	{
		gameObject.transform.rotation = gameObject.transform.rotation * Quaternion.Euler(0, 0, Time.deltaTime * Speed);
	}
}
