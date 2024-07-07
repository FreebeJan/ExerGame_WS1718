using System.Collections;
using UnityEngine;
using UnityEngine.PostProcessing;

public class AutoFocusCamera : MonoBehaviour
{

	public Transform Target;
	public Vector3 RelativeOffset;
	PostProcessingProfile profile;
	public bool StaticFocus = false;
	[Range(0.0001f, .99f)] public float RateOfChange;
	[Range(1f, 100f)] public float minDist;
	[Range(1f, 100f)] public float maxDist;
	[Range(.01f, 1000f)] public float MagicValue_c;
	[Range(.01f, 1000f)] public float MagicValue_d;


	// Use this for initialization
	void Start()
	{
		profile = GetComponent<PostProcessingBehaviour>().profile;
		Adjust(false);
	}

	void Adjust(bool increment = true)
	{
		var dof = profile.depthOfField.settings;
		Vector3 CurrentOffset = RelativeOffset.x * Target.forward + RelativeOffset.y * Target.up + RelativeOffset.z * Target.right;
		Vector3 POI = Target.position + minDist * CurrentOffset;

		float dist = MagicValue_d * Vector3.Dot(POI - transform.position, transform.up);
		float d = Screen.width > Screen.height ? Screen.width : Screen.height;
		float aperture = MagicValue_c * dof.focalLength * dof.focalLength * (dist - minDist) / (dist * minDist * d);
		//float aperture = d;
		if (increment)
		{
			dof.focusDistance = (1f - RateOfChange) * dof.focusDistance + RateOfChange * dist;
			dof.aperture = (1f - RateOfChange) * dof.aperture + RateOfChange * aperture;
		}
		else
		{
			dof.focusDistance = dist;
			dof.aperture = aperture;
		}

		// Apply settings
		profile.depthOfField.settings = dof;
	}

	void Update()
	{
		if (!StaticFocus)
		{
			Adjust();
		}
	}
}
