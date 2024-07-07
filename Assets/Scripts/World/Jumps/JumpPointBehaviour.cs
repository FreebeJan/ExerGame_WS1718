using Assets.World.Paths;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPointBehaviour : MonoBehaviour
{

	public Vector3 start, target, rayMinTarget, rayMaxTarget, RayTarget, landingMinPoint, landingMaxPoint, landingPoint;
	public float gravity;

	private int terrainLayerMask = 1 << 8; // terrains are on layer 8;

	private bool DoRayCast(Vector3 start, Vector3 target, out RaycastHit hitInfo, int layerMask)
	{
		return Physics.Raycast(start, (target - start).normalized, out hitInfo, (target - start).magnitude, layerMask);
	}


	private bool DoRayCast(Vector3 start, Vector3 target, out RaycastHit hitInfo)
	{
		return Physics.Raycast(start, (target - start).normalized, out hitInfo, (target - start).magnitude);
	}

	public bool CheckCollisions()
	{
		int noTerrain = int.MaxValue;
		noTerrain = noTerrain ^ terrainLayerMask; // everything but terrain.
		RaycastHit minRay, maxRay, ExactRay, minRay2, maxRay2, ExactRay2;
		// check wheter we hit something in the given range that is not a terrain.
		if (DoRayCast(start, rayMinTarget, out minRay, noTerrain)) return false;
		if (DoRayCast(start, rayMaxTarget, out maxRay, noTerrain)) return false;
		if (DoRayCast(start, RayTarget, out ExactRay, noTerrain)) return false;
		if (DoRayCast(rayMinTarget, landingMinPoint, out minRay2, noTerrain)) return false;
		if (DoRayCast(rayMaxTarget, landingMaxPoint, out maxRay2, noTerrain)) return false;
		if (DoRayCast(RayTarget, landingMaxPoint, out ExactRay2, noTerrain)) return false;
		return true;
	}

	public void getJump(float speed) {
		
	}

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}
}
