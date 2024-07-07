using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SeedPanel : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	public void AddSeedName (string txt) {
        GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
        if (!gameController.getSeedList().Contains(txt)){
            gameController.addSeed(txt);
        }
        PlayerPrefs.SetString("GameSeed", txt);

		
	}
}
