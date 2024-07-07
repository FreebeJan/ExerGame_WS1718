using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{


    public Sprite ButtonSprite;
    public Font jupiter;
    private bool isPaused = false; //Lets you know if the game is paused or not.
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            isPaused = !isPaused;

        if (isPaused)
            Time.timeScale = 0f; //Time stops
        else
            Time.timeScale = 1.0f; //Time is resuming
    }

    void OnGUI()
    {
        if (isPaused)
        {

            GUIContent buttonSettings = new GUIContent();

            GUI.skin.button.normal.background = (Texture2D)ButtonSprite.texture;
            GUI.skin.button.hover.background = (Texture2D)ButtonSprite.texture;
            GUI.skin.button.active.background = (Texture2D)ButtonSprite.texture;
            GUI.skin.button.fontSize = 65;
            GUI.skin.button.font = jupiter;


            buttonSettings.text = "Continue";
            // If the button "Continue" is pressed, the game is resuming
            if (GUI.Button(new Rect(Screen.width / 2 - 300, Screen.height / 2 - 80, 600, 80), buttonSettings))
            {
                isPaused = false;
            }

            buttonSettings.text = "Quit";
            // If the button "Quit" is pressed, the game returns to the MainMenu
            if (GUI.Button(new Rect(Screen.width / 2 - 300, Screen.height / 2 + 80, 600, 80), buttonSettings))
            {
                isPaused = false;
                SceneManager.LoadScene("MenuPanel");

            }
        }
    }
}
