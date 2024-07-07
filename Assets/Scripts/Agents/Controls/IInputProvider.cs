using System;
using UnityEngine;

namespace Assets.Scripts.Agents.Controls
{
	public class IInputProvider: MonoBehaviour
	{
		public virtual float GetSpeedNormalized() // values between 0 and 1.
		{
			throw new NotImplementedException();
		}
		public virtual bool GetDirection() {
			throw new NotImplementedException();
		} // true means forward, false means backward.
		
	}
}
