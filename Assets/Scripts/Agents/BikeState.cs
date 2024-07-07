using UnityEngine;

/// <summary>
/// Script to store bike state. Used rto communicate between bike scripts.
/// </summary>
public class BikeState : MonoBehaviour {
	public bool Inflight = false; // is the bike flying?
	public bool forward = true; // follow the path in forward direction?
	public float Speed = 0; // total speed in unity units per second
	public float Leaning = 0; // amount of Leaning
	public Vector3 handleDirection = new Vector3(); // Direction of the handles.
	public Vector3 TargetPos = new Vector3(); // Target location
}
