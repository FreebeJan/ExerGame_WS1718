using UnityEngine;

/// <summary>
/// Corrects the location and orientation of the bike by casting rays from the top of the tires to the ground
/// and adjusting so the bike does not intersect the terrain.
/// </summary>
public class PlacementCorrection : MonoBehaviour {

	public float FrontTireOffset = .6f;
	public float BackTireOffset = .6f;
	public float RayOffset = .6f;
	public float RayMaxDist = 1.2f;
	private BikeState State;

	private void Start()
	{
		State = GetComponent<BikeState>();	
	}

	// LateUpdate is called once per frame after all Update() calls.
	void LateUpdate () {
		if (!State.Inflight)
		{
			Ray posRay = new Ray(transform.position + RayOffset * transform.up, -transform.up);
			RaycastHit posHit;
			if (Physics.Raycast(posRay, out posHit, RayMaxDist, 1<<8, QueryTriggerInteraction.Ignore))
			{
				transform.position = posHit.point;
			}

			Vector3 pos1 = transform.forward * BackTireOffset + transform.position;
			Vector3 pos2 = -transform.forward * FrontTireOffset + transform.position;
			Ray ray1 = new Ray(pos1 + RayOffset * transform.up, -transform.up);
			Ray ray2 = new Ray(pos2 + RayOffset * transform.up, -transform.up);
			RaycastHit hit1, hit2;
			if (Physics.Raycast(ray1, out hit1, RayMaxDist, 1<<8, QueryTriggerInteraction.Ignore))
			{
				if (Physics.Raycast(ray2, out hit2, RayMaxDist, 1<<8, QueryTriggerInteraction.Ignore))
				{
					transform.rotation = Quaternion.LookRotation(hit1.point - hit2.point);
				}
			}
		}
	}
}
