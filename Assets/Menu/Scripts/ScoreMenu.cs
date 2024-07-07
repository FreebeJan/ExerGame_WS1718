using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ScoreMenu : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
        List<float> scoreList = gameController.getScoreList();
        List<int> durationList = gameController.getDurationList();

        for (int j = 0; j < scoreList.Count; j++)
        {
            GameObject.Find(string.Format("Score{0}", j + 1)).GetComponent<Text>().text = scoreList[j].ToString();
            GameObject.Find(string.Format("Time{0}", j + 1)).GetComponent<Text>().text = TimeSpan.FromSeconds(durationList[j]).ToString();
        }

    }
}
