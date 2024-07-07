using UnityEngine;

/// <summary>
/// Monobehavior responsible to adjusting Bike Animation speed and Leaning of the bike to the current state.
/// </summary>
public class BikeAnimation : MonoBehaviour
{
	// Leaning Configuration
	public float LeaningThreshold = 5f; // Angle threshold in degree.
	public float LeaningSpeed = 10f; // degree/s leaning speed.
	public float MaxLeaningAngle = 20f; // Bike won't lean more then this [degree].

	// Pedaling Animation
	public float Speed2PedalingFactor = .25f; // Pedellaing animation is adjusted based on bike speed multiplied with this number. 

	private BikeState State; // state of the bike/player
	private Transform Model; // transform of the bike mesh
	private Animation PedalingAnimation; // Animation of the pedalling movement.

	// Use this for initialization
	void Start()
	{
		State = GetComponent<BikeState>();
		Model = transform.Find("Model");
		PedalingAnimation = Model.GetComponent<Animation>();
	}

	// LastUpdate is called once per frame after all Update calls have been done.
	/// <summary>
	/// Tilts bike mesh to simulate leaning. Adjust pedaling animation speed.
	/// </summary>
	void LateUpdate()
	{
		Vector3 TargetDir = (State.TargetPos - transform.position).normalized;
		float dirAngle = Vector3.SignedAngle(transform.forward, TargetDir, transform.up);
		if (Mathf.Abs(dirAngle) < LeaningThreshold) dirAngle = 0;
		float oldLeaning = State.Leaning;
		float Leaning = Mathf.MoveTowardsAngle(oldLeaning, (float)System.Math.Round(dirAngle / 90f, 1) * MaxLeaningAngle, LeaningSpeed * Time.deltaTime);
		State.Leaning = (Leaning + oldLeaning) / 2f;
		Model.rotation *= Quaternion.Euler(-oldLeaning, 0, 0);
		Model.rotation *= Quaternion.Euler(State.Leaning, 0, 0);
		// Adjust the bike animation speed to the speed of the bike.
		PedalingAnimation["Take 001"].speed = State.Speed * Speed2PedalingFactor;
	}
}
