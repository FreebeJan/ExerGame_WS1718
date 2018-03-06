﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseItem : MonoBehaviour {
	
	private float applicationTime = 0;
	public float lifespan = 10.0f; // Lifespan in seconds
	
	public virtual void applyEffect(GameObject player) 
	{ 
		applicationTime = Time.time;
	}
	
	public virtual void revertEffect(GameObject player) { }
	public virtual bool isDone() 
	{ 
		return (Time.time > (lifespan + applicationTime)); 
	}
}