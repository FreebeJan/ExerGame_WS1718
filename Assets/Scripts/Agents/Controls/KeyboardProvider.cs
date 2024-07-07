using UnityEngine;

namespace Assets.Scripts.Agents.Controls
{
	/// <summary>
	/// Simple Keyboard interactions: Press up to move. Press space to turn around.
	/// </summary>
	public class KeyboardProvider : IInputProvider
	{

		private bool Direction;
		private float Speed;

		public override bool GetDirection()
		{
			return Direction;
		}

		public override float GetSpeedNormalized()
		{
			return Speed;
		}

		void Update()
		{
			if (Input.GetKey("up"))
			{
				Speed = 1f;
			}
			else
			{
				Speed = 0f;
			}
		}

		void OnGUI()
		{
			if (Event.current.Equals(Event.KeyboardEvent("space")))
			{
				Direction = !Direction;
			}
		}
	}
}
