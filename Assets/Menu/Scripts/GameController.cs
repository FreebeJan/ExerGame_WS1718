using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    public static GameController control;
    private List<float> scoreList = new List<float>();
    private List<int> durationList = new List<int>();

    private List<string> seedList = new List<string>();
    void Start()
    {
        if (control == null)
        {
            DontDestroyOnLoad(gameObject);
            control = this;
        }
        else if (control != this){
            Destroy(gameObject);
        }

    }

    void Update(){


        for (int i = 0 ; i < seedList.Count ; i++){
            Debug.Log(string.Format("seedList[{0}] = {1}", i, seedList[i]));
        }
    }
    public List<float> getScoreList(){ 
        
        return this.scoreList;
    }
    public List<int> getDurationList(){
        return this.durationList;
    }

    public void setScoreList(List<float> list){
        this.scoreList = list;
    }
     public void setDurationList(List<int> list){
        this.durationList = list;
    }

    public List<string> getSeedList(){
        return this.seedList;
    }

    public void addSeed(string name){
        this.seedList.Add(name);
    }

}
