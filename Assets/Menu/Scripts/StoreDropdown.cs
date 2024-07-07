using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreDropdown : MonoBehaviour
{

	public string ValueName;

	// Use this for initialization
	void Start()
	{
		RememberValue();
	}

	public void RememberValue()
	{
		Dropdown dd = GetComponent<Dropdown>();
		PlayerPrefs.SetString(ValueName, dd.options[dd.value].text);
	}
}
