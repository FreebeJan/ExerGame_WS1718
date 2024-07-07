using UnityEngine;

namespace Assets.Scripts.Agents.Behavior
{
	interface ITargetProvider
	{
		Vector3 GetNextTarget();
		Vector3 GetCurrentPos();
		void TurnAround();
	}
}
