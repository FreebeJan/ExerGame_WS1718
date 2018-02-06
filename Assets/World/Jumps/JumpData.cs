﻿using UnityEngine;

namespace Assets.World.Jumps
{
	struct JumpData
	{
		public Vector3 Pos, Dir, LandingPos, RayTarget;
		public float PerfectSpeed, PerfectTime;
		public GameObject Ramp;
		public bool Enabled;
	}
}
