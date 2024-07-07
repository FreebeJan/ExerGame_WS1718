using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SoundManager : MonoBehaviour
{

	// Use this for initialization

	public Slider mainVolumeSlider;
	public Slider musicVolumeSlider;
	public Slider effectsVolumeSlider;
	public GameObject MusicObject;
	public GameObject HiglightSoundObject;
	public GameObject BackSoundObject;
	public GameObject SelectSoundObject;

	void Start()
	{
		mainVolumeSlider.value = 0.5f;
		musicVolumeSlider.value = 0.5f;
		effectsVolumeSlider.value = 0.5f;
	}
	private float previousMainValue;
	private float previousEffectValue;
	private float previousMusicValue;
	// Update is called once per frame
	void Update()
	{
		if (mainVolumeSlider.value != previousMainValue)
		{
			musicVolumeSlider.value = 0.5f;
			effectsVolumeSlider.value = 0.5f;
			previousEffectValue = 0.5f;
			previousMusicValue = 0.5f;
			MusicObject.GetComponent<AudioSource>().volume = mainVolumeSlider.value;
			HiglightSoundObject.GetComponent<AudioSource>().volume = mainVolumeSlider.value;
			BackSoundObject.GetComponent<AudioSource>().volume = mainVolumeSlider.value;
			SelectSoundObject.GetComponent<AudioSource>().volume = mainVolumeSlider.value;
			previousMainValue = mainVolumeSlider.value;
		}
		else
		{
			if (effectsVolumeSlider.value != previousEffectValue || musicVolumeSlider.value != previousMusicValue)
			{
				previousMainValue = 0.5f;
				mainVolumeSlider.value = 0.5f;
				MusicObject.GetComponent<AudioSource>().volume = musicVolumeSlider.value;
				HiglightSoundObject.GetComponent<AudioSource>().volume = effectsVolumeSlider.value;
				BackSoundObject.GetComponent<AudioSource>().volume = effectsVolumeSlider.value;
				SelectSoundObject.GetComponent<AudioSource>().volume = effectsVolumeSlider.value;
				previousEffectValue = effectsVolumeSlider.value;
				previousMusicValue = musicVolumeSlider.value;
			}
		}
	}
}
