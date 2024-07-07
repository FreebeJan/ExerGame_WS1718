using UnityEngine;
using UnityEngine.PostProcessing;

/// <summary>
/// Adjusts motion blur based on player speed;
/// </summary>
public class MotionBlurAdjust : MonoBehaviour
{

	public PostProcessingProfile profile;
	private BikeState state;
	public float defaultAngle = 20f;
	public int defaultSampleCount = 20;
	public float speedModifier = 1f;
	private int counter = 0;
	public int refreshInterval = 3;

	// Use this for initialization
	void Start()
	{
		state = GetComponent<BikeState>();
	}

	// Update is called once per frame
	void LateUpdate()
	{
		counter++;
		if (counter >= refreshInterval)
		{
			counter = 0;
			var settings = new MotionBlurModel.Settings
			{
				shutterAngle = defaultAngle + speedModifier * state.Speed,
				sampleCount = defaultSampleCount,
				frameBlending = profile.motionBlur.settings.frameBlending
			};
			profile.motionBlur.settings = settings;
		}
	}
}
