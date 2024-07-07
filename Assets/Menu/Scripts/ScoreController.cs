using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreController : MonoBehaviour
{

    public static ScoreController control;

    private List<float> scoreList = new List<float>();
    private List<int> durationList = new List<int>();

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

        float newScore = PlayerPrefs.GetFloat("CurrentScore");
        int newDuration = PlayerPrefs.GetInt("CurrentDuration");

        GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
        scoreList = gameController.getScoreList();
        durationList = gameController.getDurationList();

        if (newScore != 0)
        {
            Debug.Log(string.Format("SCORE MENU : {0} -- {1}", newScore, newDuration));
            int i = 0;
            bool rankFound = false;

            while (!rankFound && i < scoreList.Count)
            {
                // If the (i-1)th score is weaker than ours, we insert ours at the (i-1)th position
                if (scoreList[i] < newScore)
                {
                    scoreList.Insert(i, newScore);
                    durationList.Insert(i, newDuration);
                    rankFound = true;


                }
                // If the (i-1)th score is equal to ours we compare the duration
                else if (scoreList[i] == newScore)
                {
                    // Durations are stored by increase order
                    if (durationList[i] > newDuration)
                    {
                        scoreList.Insert(i + 1, newScore);
                        durationList.Insert(i + 1, newDuration);
                    }
                    else
                    {
                        scoreList.Insert(i, newScore);
                        durationList.Insert(i, newDuration);
                    }
                    rankFound = true;
                }


                if (scoreList.Count > 10)
                {
                    scoreList.RemoveAt(scoreList.Count - 1);
                    durationList.RemoveAt(scoreList.Count - 1);
                }
                i = i + 1;
            }

            // if there're less than 10 scores and our score isn't displayed yet --> add at the end
            if (i < 10 && !rankFound)
            {
                scoreList.Add(newScore);
                durationList.Add(newDuration);
            }

            gameController.setScoreList(scoreList);
            gameController.setDurationList(durationList);


            PlayerPrefs.SetFloat("CurrentScore", 0);
            PlayerPrefs.SetInt("CurrentDuration", 0);
        }
    }
}


