namespace Assets.Scripts.Agents.Controls
{
	/// <summary>
	/// Simple AI that always follows forward direction of the path in full speed.
	/// </summary>
	class SimpleAIProvider : IInputProvider
	{
		public override bool GetDirection()
		{
			return true;
		}

		public override float GetSpeedNormalized()
		{
			return 1;
		}
	}
}
