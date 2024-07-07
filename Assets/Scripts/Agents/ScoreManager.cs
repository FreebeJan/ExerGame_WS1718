using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour 
{
	public int Score = 0;
	public float Multiplier = 10.0f;
	public bool DisplayScore = false;
	
	private Vector3 LastPosition = new Vector3(); 
	
	void Start()
	{
		LastPosition = gameObject.transform.position;
	}
	
	void Update()
	{
		var diff = LastPosition - gameObject.transform.position;
		LastPosition = gameObject.transform.position;
		
		Score += (int) (diff.magnitude * Multiplier);

		//Debug.Log(string.Format("Score courant : {0}", Score));
		// Debug.Log(string.Format("Duration courant : {0}", (int)Time.realtimeSinceStartup));
		PlayerPrefs.SetFloat("CurrentScore", Score);
		PlayerPrefs.SetInt("CurrentDuration", (int)Time.realtimeSinceStartup);
	}
	
	void OnGUI()
	{
		if(DisplayScore)
			GUI.Label(new Rect(25, 25, 100, 30), "Score: " + Score);
	}
}
 
