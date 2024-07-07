using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class LoadSceneOnClick : MonoBehaviour
{


	public void LoadByIndex(int sceneIndex)
	{

		Dropdown[] Parameters = GetComponents<Dropdown>();
		foreach (Dropdown dropdown in Parameters)
		{
			Dropdown.OptionData options = dropdown.options[dropdown.value];
			PlayerPrefs.SetString(dropdown.ToString(), options.text);
		}
		SceneManager.LoadScene(sceneIndex);

	}
}
