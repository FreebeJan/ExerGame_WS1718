﻿using System.Collections;
using System.Collections.Generic;
using Assets.World.Paths;

using UnityEngine;
using UnityEngine.UI;

public class QTESys : MonoBehaviour
{

    private Vector3 IntersectionPoint;
    private List<PathWithDirection> PathChoices;
    private PathWithDirection CurrentPath;
    private int numberOfChoices;
    //private int QTEGen;
    private int CorrectKey;
    private int CountingDown;
    private bool displayLetter = false;
    private bool displayMessage = false;
    private bool passMessage = false;

    private bool runningQTE = false;

    private List<int> listOfQTE;
    private int ReturnQTE = -1;
    public void QTE_Initialisation(int number, List<PathWithDirection> Choices, PathWithDirection current, Vector3 point)
    {
        this.IntersectionPoint = point;
        this.PathChoices = Choices;
        this.CurrentPath = current;
        int QTEGen;
        Debug.Log("INITIALISATION");
        runningQTE = true;
        this.numberOfChoices = number;
        listOfQTE = new List<int>();
        for (int i = 0; i < numberOfChoices; i++)
        {
            QTEGen = UnityEngine.Random.Range(1, numberOfChoices);
            listOfQTE.Add(QTEGen);
            Debug.Log(listOfQTE);
        }
        displayLetter = true;
        CountingDown = 1;
        StartCoroutine(CountDown());

       
    }

    public void showLetter()
    {
        displayLetter = true;
    }

    public void hideLetter()
    {
        displayLetter = false;
    }

    public void showMessage()
    {
        displayMessage = true;
    }

    public void hideMessage()
    {
        displayMessage = false;
    }

    public bool isFinished()
    {
        return runningQTE == false;
    }

    public int getReturn()
    {
        return ReturnQTE;
    }
    public void stop()
    {
        Debug.Log("STOP");
        runningQTE = false;
        hideMessage();
        hideLetter();
    }

    public void Update()
    {

        //Debug.Log(string.Format("Runnning ? {0}", runningQTE));
        /*
        Debug.Log(string.Format("DisplayLetter ? {0}", displayLetter));
        Debug.Log(string.Format("DisplayMessage ? {0}", displayMessage));*/
        if (runningQTE)
        {
            if (isInterestingKeysDown())
            {
                if (listOfQTE.Contains(1))
                {
                    if (Input.GetKeyDown(KeyCode.A))
                    {
                        CorrectKey = 1;
                        StartCoroutine(KeyPressing());
                        ReturnQTE = listOfQTE.IndexOf(1);
                    }
                    else
                    {
                        CorrectKey = 2;
                        StartCoroutine(KeyPressing());
                    }
                }
                if (listOfQTE.Contains(2))
                {
                    if (Input.GetKeyDown(KeyCode.Z))
                    {
                        CorrectKey = 1;
                        StartCoroutine(KeyPressing());
                        ReturnQTE = listOfQTE.IndexOf(2);
                    }
                    else
                    {
                        CorrectKey = 2;
                        StartCoroutine(KeyPressing());
                    }
                }
                if (listOfQTE.Contains(3))
                {
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        CorrectKey = 1;
                        StartCoroutine(KeyPressing());
                        ReturnQTE = listOfQTE.IndexOf(3);
                    }
                    else
                    {
                        CorrectKey = 2;
                        StartCoroutine(KeyPressing());
                    }
                }
                if (listOfQTE.Contains(4))
                {
                    if (Input.GetKeyDown(KeyCode.R))
                    {
                        CorrectKey = 1;
                        StartCoroutine(KeyPressing());
                        ReturnQTE = listOfQTE.IndexOf(4);
                    }
                    else
                    {
                        CorrectKey = 2;
                        StartCoroutine(KeyPressing());
                    }
                }
                StartCoroutine(KeyPressing());
                runningQTE = false;
            }

        }
    }
    IEnumerator KeyPressing()
    {
        if (CorrectKey == 1)
        {
            CountingDown = 2;
            passMessage = true;
            showMessage();
            yield return new WaitForSeconds(1.5f);
            CorrectKey = 0;
            hideMessage();
            hideLetter();
            yield return new WaitForSeconds(1.5f);
            CountingDown = 1;

        }
        if (CorrectKey == 2)
        {
            CountingDown = 2;
            passMessage = false;
            showMessage();
            yield return new WaitForSeconds(1.5f);
            CorrectKey = 0;
            hideMessage();
            hideLetter();
            yield return new WaitForSeconds(1.5f);
            CountingDown = 1;
            ReturnQTE = 0;

        }
    }

    IEnumerator CountDown()
    {
        yield return new WaitForSeconds(3.5f);
        if (CountingDown == 1)
        {
            CountingDown = 2;
            passMessage = false;
            showMessage();
            yield return new WaitForSeconds(1.5f);
            CorrectKey = 0;
            hideMessage();
            hideLetter();
            yield return new WaitForSeconds(1.5f);
            CountingDown = 1;
        }

    }
    void OnGUI()
    {
        int QTEGen;
        if (displayLetter)
        {
            Debug.Log(string.Format("NumberOfChoices : {0}", numberOfChoices));
            Debug.Log(string.Format("Screen size : {0};{1}", Screen.width, Screen.height));
            for (int i = 0; i < numberOfChoices; i++)
            {
                QTEGen = listOfQTE[i];
                float angle = computeAngle(i+1); // Degrees
                Debug.Log(string.Format("Angle {0} in Degree : {1}", i, angle));
                angle = angle * Mathf.PI/180;
                Debug.Log(string.Format("Angle {0} in Radian : {1}", i, angle));
                Debug.Log(string.Format("Cos / Sin -> {0} / {1}", Mathf.Cos(angle), Mathf.Sin(angle)));
                Debug.Log(string.Format("Position -> {0} / {1}", Screen.width/2 + Screen.width/3*Mathf.Cos(angle), Screen.height / 2 + Screen.height/3*Mathf.Sin(angle)));
                // 1 -> l/2 -40                         | - |
                // 2 -> l/4 -40 / 3l/4 -4               | - | - |
                // 3 -> l/4 -40 / l/2 -40 / 3l/4 -40    | - - - | 
                // 4 -> l/5 / 2l/5 / 3l/5 / 4l/5        | - - | - - |

                if (QTEGen == 1)
                {
                    GUI.Button(new Rect(Screen.width/2 + Screen.width/3*Mathf.Cos(angle), Screen.height / 2 + Screen.height/3*Mathf.Sin(angle), 80, 40), "[A]");
                }
                if (QTEGen == 2)
                {
                    GUI.Button(new Rect(Screen.width/2 + Screen.width/3*Mathf.Cos(angle), Screen.height / 2 + Screen.height/3*Mathf.Sin(angle), 80, 40), "[Z]");
                }
                if (QTEGen == 3)
                {
                    GUI.Button(new Rect(Screen.width/2 + Screen.width/3*Mathf.Cos(angle), Screen.height / 2 + Screen.height/3*Mathf.Sin(angle), 80, 40), "[E]");
                }
                if (QTEGen == 4)
                {
                    GUI.Button(new Rect(Screen.width/2 + Screen.width/3*Mathf.Cos(angle), Screen.height / 2 + Screen.height/3*Mathf.Sin(angle), 80, 40), "[R]");
                }

            }

        }
        if (displayMessage)
        {
            if (passMessage)
            {
                GUI.Button(new Rect(Screen.width / 2 - 60, Screen.height / 2 - 50, 120, 100), "PASS !");
            }
            else
            {
                GUI.Button(new Rect(Screen.width / 2 - 60, Screen.height / 2 - 50, 120, 100), "FAIL !");
            }
        }
    }



    private bool isInterestingKeysDown()
    {
        return Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.R);
    }


    private float computeAngle(int i){
        Vector3 v_player = CurrentPath.path.WorldWaypoints[Mathf.Max(CurrentPath.path.WorldWaypoints.Length-11, 0)]  - IntersectionPoint;

        Vector3 v_path = new Vector3();

        if (MapTools.Aprox(IntersectionPoint, PathChoices[i].path.WorldWaypoints[0])){
            v_path = PathChoices[i].path.WorldWaypoints[Mathf.Min(PathChoices[i].path.WorldWaypoints.Length-1, 9)] - IntersectionPoint;
        }else{
            v_path = PathChoices[i].path.WorldWaypoints[Mathf.Min(PathChoices[i].path.WorldWaypoints.Length-1, 9)] - IntersectionPoint;   
        }

        return Vector3.SignedAngle(v_player, v_path, Vector3.up);
       
    }



}
