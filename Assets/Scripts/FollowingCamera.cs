using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowingCamera : MonoBehaviour {

	public GameObject Target;
	public Vector3 PositionOffset;
	public Vector3 LookAtOffset;
	public float MaxSpeed;
	public float MaxRotationSpeed;
	public Vector3 AdditionalRotation;

	// Use this for initialization
	void Start () {
		SetLocation();
	}

	private void Awake()
	{
		SetLocation();
	}

	public void SetLocation() {
		Vector3 Offset = Target.transform.right * PositionOffset.x + Target.transform.up * PositionOffset.y + Target.transform.forward * PositionOffset.z;
		Vector3 LookAt = Target.transform.right * LookAtOffset.x + Target.transform.up * LookAtOffset.y + Target.transform.forward * LookAtOffset.z;
		transform.position = Target.transform.position + Offset;
		transform.rotation = Quaternion.LookRotation(Target.transform.position + LookAt);
	}

	// Update is called once per frame
	void Update ()
	{
		Vector3 Offset = Target.transform.right * PositionOffset.x + Target.transform.up * PositionOffset.y + Target.transform.forward * PositionOffset.z;
		transform.position = Vector3.MoveTowards(transform.position, Target.transform.position + Offset, MaxSpeed * Time.deltaTime);
		Vector3 LookAt = Target.transform.position + Target.transform.right * LookAtOffset.x + Target.transform.up * LookAtOffset.y + Target.transform.forward * LookAtOffset.z;
		transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation((LookAt - transform.position).normalized, Target.transform.up), MaxRotationSpeed * Time.deltaTime);
	}
}
