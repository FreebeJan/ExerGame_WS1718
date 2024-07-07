using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseItem : MonoBehaviour {
	
	private float applicationTime = 0;
	public GameObject visibleMesh = null; // The GameObject holding the graphics so it can be hidden
	public float lifespan = 10.0f; // Lifespan in seconds
	
	public virtual void applyEffect(GameObject player) 
	{
		var sound = GetComponent<AudioSource>();
		if(sound != null)
		{
			//Debug.Log("PLAY");
			sound.Play();
		}
		
		applicationTime = Time.time;
	}
	
	public virtual void revertEffect(GameObject player) { }
	public virtual bool isDone() 
	{ 
		return (Time.time > (lifespan + applicationTime)); 
	}
}
