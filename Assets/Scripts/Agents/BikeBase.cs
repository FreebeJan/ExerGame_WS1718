using Assets.Scripts.Agents.Behavior;
using Assets.Scripts.Agents.Controls;
using Assets.World.Paths;
using UnityEngine;

/// <summary>
/// Base Script of the Bike/Player.
/// Does the Movement on the Update towards the BikeState.target.
/// Calls a ITargetProvider implementation to get the next target.
/// </summary>
public class BikeBase : MonoBehaviour
{

	public enum TargetSource
	{
		Simple_AI_Path_Tracing,
		QTE_Events
	}

	public enum InputSource
	{
		Simple_AI,
		Keyboard,
		RPMSource
	}

	public SurfaceManager SurfaceManager;
	public TerrainChunk ActiveTerrain;
	
	public TargetSource TargetSourceSetting;
	private ITargetProvider TargetProvider;

	public InputSource InputSourceSettings;
	private IInputProvider InputProvider;

	private BikeState State;
	public QTESys QTE_Sys; // only needed if targetSource is QTEProvider

	// path smoothing settings
	public int SmoothCount = 10;
	public float SkipDist = 4;

	// movement settings
	public float MaxSpeed = 10;
	public float MaxRotation = 3;
	public float Inertia = 0.6f;

	// Use this for initialization
	public void Init()
	{

		// Initiate input provider
		switch (InputSourceSettings) {
			case (InputSource.Keyboard):
				KeyboardProvider inputProvider = gameObject.AddComponent(typeof(KeyboardProvider)) as KeyboardProvider;
				InputProvider = inputProvider;
				break;
			case (InputSource.RPMSource):
				RPMProvider inputRPMProvider = gameObject.AddComponent(typeof(RPMProvider)) as RPMProvider;
				InputProvider = inputRPMProvider;
				break;
			default:
				SimpleAIProvider inputAIProvider = gameObject.AddComponent(typeof(SimpleAIProvider)) as SimpleAIProvider;
				InputProvider = inputAIProvider;
				break;
		}
		
		State = GetComponent<BikeState>();
		if (ActiveTerrain == null)
		{
			ActiveTerrain = SurfaceManager.GetTile(new Vector2Int(0, 0)); // cluster in the middle 
		}                                                         // Initial Position
		WayVertex StartingPoint = ActiveTerrain.GetPathFinder().StartingPoint;

		// initially we take the longest path 
		var longestPath = StartingPoint.GetLongest();

		// Initiate target provider
		if (TargetSourceSetting == TargetSource.Simple_AI_Path_Tracing)
		{
			TargetProvider = new SimpleAIPathTracing(longestPath);
		}
		else
		{
			TargetProvider = new QTEProvider(longestPath, QTE_Sys);
		}
		transform.position = TargetProvider.GetCurrentPos();
		State.TargetPos = TargetProvider.GetNextTarget();
		transform.rotation = Quaternion.LookRotation((State.TargetPos - transform.position).normalized);
		State.forward = true;
	}

	// Update is called once per frame
	void Update()
	{
		if (State.forward != InputProvider.GetDirection()) {
			State.forward = !State.forward;
			TargetProvider.TurnAround();
		}
		// choose next target
		{
			int count = 0;
			// If the distance between the player and the next waypoint is less than the distance that can be reached in a unit of time
			// we advance the waypoint
			Vector3 t = State.TargetPos;
			while ((transform.position - t).magnitude < SkipDist && count < SmoothCount)
			{
				t = TargetProvider.GetNextTarget();
				count++;
			}
			State.TargetPos = t;
		}

		Vector3 TargetDir = (State.TargetPos - transform.position).normalized;

		transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(TargetDir.normalized, transform.up), MaxRotation * Time.deltaTime);
		State.handleDirection = TargetDir - transform.forward;
		// if the position of the player is not at the path point
		// move until it reach it
		float dirAngle = Vector3.SignedAngle(transform.forward, TargetDir, transform.up);
		float dist = (State.TargetPos - transform.position).magnitude;
		float Speed = MaxSpeed * .5f * (1f - transform.forward.y) * Mathf.Cos(Mathf.Abs(dirAngle * Mathf.Deg2Rad));
		Speed *= InputProvider.GetSpeedNormalized();
		// calculate inertia independant of fps
		float TimedReverseInertia = (1f - Inertia) * Time.deltaTime;
		TimedReverseInertia = TimedReverseInertia <= 0 ? .01f : TimedReverseInertia;
		State.Speed = (1f - TimedReverseInertia) * State.Speed + TimedReverseInertia * Speed;

		// do not overshoot the target.
		dist = Mathf.Min(dist, Speed * Time.deltaTime);
		transform.position = transform.position + transform.forward * dist;
	}
}
